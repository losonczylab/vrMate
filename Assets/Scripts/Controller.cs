using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using System;
using SimpleJSON;

/// <summary>
/// Controller for all aspects of Vr experiments. Handels all of the incomming 
/// UDP messages and taking the correct actions. Turns contexts on and off and
/// populates cues.
/// </summary>
public class Controller : MonoBehaviour {

    /// <summary>
    /// Communicator object for receiving messages from BehaviorMate
    /// </summary>
    private Communicator comm;

    /// <summary>
    /// Mouse controler object for representing the mouse's position in the
    /// environment and updating the main camera.
    /// </summary>
    private MouseController mouse;

    /// <summary>
    /// Controller for setting the view bounds. Permits for the camea to not be
    /// centered in the view.
    /// </summary>
    private ViewController view;

    /// <summary>
    /// Controls weither the main view is with the mouse or on the stopped
    /// camera.
    /// </summary>
    /// <remarks>
    /// Changing the value of stopped will cause the camera to flip on the next
    /// cycle of the event loop.
    /// </remarks>
    private bool stopped;

    /// <summary>
    /// The port to listen for Udp messages on
    /// </summary>
    public int port = 4020;

    /// <summary>
    /// The Camera object to change to when the VR scene is stopped.
    /// </summary>
    public Camera stopped_view;

    /// <summary>
    /// The main camear to display when the VR scene is not stopped.
    /// </summary>
    private Camera main_cam;

    /// <summary>
    /// Dictionary to store the Context objects which correspond to contexts
    /// which have been configured by behaviorMate.
    /// </summary>
    private Dictionary<String, GameObject> contexts;

    /// <summary>
    /// The name of the current context controlling the scene.
    /// </summary>
    /// <remarks>
    /// Set by the last context to be made active which has an associated scene.
    /// Contexts with no scene information are assumed to be "cue" only contexts
    /// and do not cause the camera to switch to the stopped view when their
    /// state changes.
    /// </remarks>
    private String current_scene;

    /// <summary>
    /// Delay start counter causes the delay main camera swich to be delayed by
    /// the number of frames indicated.
    /// </summary>
    /// <remarks>
    /// This allows for any glitch or slowdown due to switching scenes/cue sets
    /// to be hidden. Cannot switch scenes and start viewing on the same frame 
    /// without errors.
    /// </remarks>
    private int delay_start;

    /// <summary>
    /// Initiializes the controller. Sets up comms, finds the mouse and view controllers.
    /// </summary>
    void Start() {
        comm = new Communicator(port);
        //contexts = new Dictionary<String, Context>();
        this.current_scene = null;

        mouse = GameObject.FindGameObjectWithTag(
            "Mouse").GetComponent<MouseController>();
        view = GameObject.FindGameObjectWithTag(
            "MainCamera").GetComponent<ViewController>();

        stopped = false;
        stopped_view.enabled = false;
        main_cam = Camera.main;

        this.delay_start = -1;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fog = false;
        RenderSettings.fogMode = FogMode.Linear;
    }

    /// <summary>
    /// Checks for and handles new messages.
    /// </summary>
    public void Update() {
        if (comm == null) {
            comm = new Communicator(this.port);
        }

        String message = comm.checkMessage();
        while (message != null) {
            ParseMessage(message);
            message = comm.checkMessage();
        }

        if (stopped && main_cam.enabled) {
            stopped_view.enabled = true;
            main_cam.enabled = false;
        } else if (!stopped && !main_cam.enabled) {
            stopped_view.enabled = false;
            main_cam.enabled = true;
        }

        if (delay_start > 0) {
            this.delay_start --;
        } else if (delay_start == 0) {
            this.delay_start = -1;
            stopped = false;
        }
    }

    private void SetScene(String scene_name)
    {
        if (scene_name != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(scene_name);
            this.delay_start = 1;
        }
    }

    private void SetSkybox(Material skybox)
    {
        if (RenderSettings.skybox != skybox)
        {
            RenderSettings.skybox = skybox;
            this.delay_start = 1;
        }
    }

    public void StartContext(String context_id)
    {
        GameObject context_object;
        if (!this.contexts.TryGetValue(context_id, out context_object))
        {
            Debug.Log("Unable to start context" + context_id);
            return;
        }
        //GameObject context_object = contexts[context_id];
        Context context = context_object.GetComponent<Context>();
        if (!String.IsNullOrEmpty(context.GetScene()))
        {
            this.current_scene = context_id;
            this.SetScene(context.GetScene());

        }

        if (context.skybox != null)
        {
            this.SetSkybox(context.skybox);
        }

        foreach (ApplyShader shader in context.shaders)
        {
            shader.enabled = true;
        }

        if (this.delay_start != 1)
        {
            stopped = false;
        }

        context_object.SetActive(true);
    }

    private GameObject CreateCue(JSONNode object_info)
    {
        GameObject new_obj = Instantiate(
            Resources.Load("Prefabs/"+object_info["type"])) as GameObject;
        if (object_info["id"] != null)
        {
            new_obj.name = object_info["id"];
        }

        Transform cue_transform = new_obj.GetComponent<Transform>();

        if (object_info["material"] != null)
        {
            Material mat = Resources.Load(
                "Materials/"+object_info["material"], typeof(Material)) as Material;
            if (object_info["texture_scale"] != null && false)
            {
                mat.mainTextureScale = new Vector2(
                    object_info["texture_scale"][0].AsInt,
                    object_info["texture_scale"][1].AsInt);
            }

            if (cue_transform.childCount > 0)
            {
                foreach (Transform child in cue_transform)
                {
                    if (child.GetComponent<Renderer>() != null)
                    {
                        child.GetComponent<Renderer>().sharedMaterial = mat;
                    }
                }
            }
            else
            {
                if (cue_transform.GetComponent<Renderer>() != null)
                {
                    cue_transform.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
        }

        if (object_info["Position"] != null)
        {
            Vector3 position = new Vector3(
                object_info["Position"][0].AsFloat,
                object_info["Position"][2].AsFloat,
                object_info["Position"][1].AsFloat);
            cue_transform.position = position;
        }

        if (object_info["Rotation"] != null)
        {
            Vector3 rotation = new Vector3(
                object_info["Rotation"][0].AsFloat,
                object_info["Rotation"][2].AsFloat,
                object_info["Rotation"][1].AsFloat);
            cue_transform.rotation = Quaternion.Euler(rotation);
        }

        if (object_info["Scale"] != null)
        {
            Vector3 scale = new Vector3(
                object_info["Scale"][0].AsFloat,
                object_info["Scale"][2].AsFloat,
                object_info["Scale"][1].AsFloat);
            cue_transform.localScale = scale;
        }

        return new_obj;
    }

    public void EditContext(String context_id, JSONNode info)
    {
        if (this.contexts == null)
        {
            this.contexts = new Dictionary<String, GameObject>();
        }

        String type = info["type"];
        GameObject context_object;
        if ((!this.contexts.TryGetValue(context_id, out context_object)) || (context_object == null))
        {
            if (type == "move_cue")
            {
                return;
            }
            context_object = Context.Create(context_id);
            this.contexts[context_id] = context_object;
            context_object.SetActive(false);
        }

        Context context = context_object.GetComponent<Context>();

        switch (type)
        {
            case "cue":
                JSONNode object_info = info["object"];
                if (context_object.transform.Find(object_info["id"]) == null)
                {
                    GameObject cue = CreateCue(object_info);
                    cue.transform.parent = context_object.transform;
                }
                break;

            case "move_cue":
                JSONNode mv_object_info = info["object"];
                Transform move_cue = context_object.transform.Find(mv_object_info["id"]);
                if (move_cue != null)
                {
                    Vector3 position = new Vector3(
                        mv_object_info["Position"][0].AsFloat,
                        mv_object_info["Position"][2].AsFloat,
                        mv_object_info["Position"][1].AsFloat);
                    move_cue.position = position;
                }
                break;

            case "skybox":
                Material skybox_mat = Resources.Load(
                    "Skybox/"+info["skybox"],
                    typeof(Material)) as Material;
                context.skybox = skybox_mat;
                break;

            case "scene":
                context.SetScene(info["scene"]);
                break;

            case "filter":
                foreach (ApplyShader shader in GameObject.FindGameObjectWithTag("MainCamera").GetComponents<ApplyShader>())
                {
                    if (shader.id.Equals(info["id"]))
                    {
                        context.AddFilter(shader);
                        if (info["value"] != null)
                        {
                            shader.setValue("value", info["value"].AsFloat);
                        }

                        break;
                    }
                }
                break;

            default:
                Debug.Log("Failed to understand context update");
                break;
        }

    }

    private bool ParseMessage(String message) {
        JSONNode parsed_message = null;
        try {
            parsed_message = JSON.Parse(message);
        } catch (Exception e) {
            Debug.Log("failed to parse json message");
            Debug.Log(message);
            Debug.Log(e);
            return false;
        }

        if (parsed_message == null) {
            Debug.Log("failed to parse json message");
            Debug.Log(message);
            return false;
        }

        if (parsed_message["position"] != null) {
            if (parsed_message["position"]["y"] != null) {
                mouse.SetY(parsed_message["position"]["y"].AsFloat);
            }
            if (parsed_message["position"]["x"] != null) {
                mouse.SetX(parsed_message["position"]["x"].AsFloat);
            }
            if (parsed_message["position"]["z"] != null) {
                mouse.SetZ(parsed_message["position"]["z"].AsFloat);
            }
        } else if (parsed_message["action"] != null) {
            String action = parsed_message["action"].Value;
            String context_id = parsed_message["context"].Value;
            if (context_id == null)
            {
                context_id = "_current";
            }

            switch (action)
            {
                case "stop":
                    if (this.contexts != null)
                    {
                        GameObject context_object;
                        if (this.contexts.TryGetValue(context_id, out context_object))
                        {
                            if (this.current_scene == context_id)
                            {
                                this.stopped = true;
                                this.current_scene = null;
                            }
                            context_object.SetActive(false);
                        }
                    }
                    break;

                case "start":
                    this.StartContext(context_id);
                    break;

                case "clear":
                    this.RemoveAllObjects(context_id);
                    break;

                case "editContext":
                    this.EditContext(context_id, parsed_message);
                    break;

                default:
                    Debug.Log("Could not resolve action: " + action);
                    break;

           }
        }
        else if (parsed_message["view"] != null)
        {
            JSONNode view_message = parsed_message["view"];
            if (view_message["top"] != null)
            {
                view.top = view_message["top"].AsFloat;
            }

            if (view_message["bottom"] != null)
            {
                view.bottom = view_message["bottom"].AsFloat;
            }
            if (view_message["left"] != null)
            {
                view.left = view_message["left"].AsFloat;
            }
            if (view_message["right"] != null)
            {
                view.right = view_message["right"].AsFloat;
            }
            if (view_message["near"] != null)
            {
                view.near = view_message["near"].AsFloat;
            }
            if (view_message["far"] != null)
            {
                view.near = view_message["far"].AsFloat;
            }
            if (view_message["rotation"] != null)
            {
                mouse.SetViewRotation(view_message["rotation"].AsFloat);
            }
            if (view_message["elevation"] != null)
            {
                mouse.SetViewElevation(view_message["elevation"].AsFloat);
            }
            if (view_message["orientation"] != null)
            {
                mouse.SetViewOrientation(view_message["orientation"].AsFloat);
            }
        }

        if (parsed_message["fog"] != null)
        {
            if (parsed_message["fog"]["action"] != null)
            {
                if (parsed_message["fog"]["action"].Value == "start")
                {
                    RenderSettings.fog = true;
                }
                else if (parsed_message["fog"]["action"].Value == "stop")
                {
                    RenderSettings.fog = false;
                }
            }

            if (parsed_message["fog"]["start"] != null)
            {
                RenderSettings.fogStartDistance = parsed_message["fog"]["start"].AsFloat;
            }

            if (parsed_message["fog"]["end"] != null)
            {
                RenderSettings.fogEndDistance = parsed_message["fog"]["end"].AsFloat;
            }
        }

        return true;
    }

    public void DisableShaders()
    {
        foreach (ApplyShader shader in GameObject.FindGameObjectWithTag(
            "MainCamera").GetComponents<ApplyShader>())
        {
            shader.enabled = false;
        }
    }

    public void RemoveAllObjects(String context_id)
    {
        RenderSettings.fog = false;
        if (this.contexts != null)
        {
            GameObject context_object;
            if (this.contexts.TryGetValue(context_id, out context_object) && (context_object != null))
            {
                this.DisableShaders();
                if (Application.isEditor)
                {
                    DestroyImmediate(context_object);
                }
                else
                {
                    Destroy(context_object);
                }
                this.contexts.Remove(context_id);
            }
        }
    }

    /// <summary>
    /// Accessor method for the current skybox
    /// </summary>
    /// <returns>
    /// The Material currently rendering as the skybox.
    /// </returns>
    public Material getSkybox() 
    {
        return RenderSettings.skybox;
    }
}

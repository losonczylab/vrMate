using UnityEngine;
using UnityEditor;
using System.IO;

using System;
using System.Collections.Generic;
using SimpleJSON;


public class CueCompare : IComparer<vrMatePrefab>
{
    public int Compare(vrMatePrefab x, vrMatePrefab y)
    {
        return string.Compare(x.name, y.name, StringComparison.Ordinal);
    }
}


/// <summary>
/// Class for added the "Write VR File" button to the Controller Inspector in
/// the Unity GUI.
/// </summary>
[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
    /// <summary>
    /// A string to store the filename to write the current scene info into.
    /// </summary>
    /// <remarks>
    /// Defualts to "scene.vr". Stored relative to the project root.
    /// </remarks>
    public String screen_cap_path = null;

    private float last_update;
    public float pos = 300;
    private float frame_delay = 0.1f;
    private MouseController mouse;

    private int screen_capture_idx = -1;
    private JSONNode displays = null;
    private JSONNode views = null;

    private float tracklength = 400.0f;

    private ViewController vc;


    /// <summary>
    /// Defines the layout to add the "Wrote VR File" button
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //this.filename = EditorGUILayout.TextField(filename);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save VR File"))
        {
            string path = EditorUtility.SaveFilePanel("Save VR File", "", "scene.vr", "vr");
            if (!path.Equals(""))
            {
                WriteFile(path);
            }
        }

        if (GUILayout.Button("Read VR File"))
        {
            string path = EditorUtility.OpenFilePanel("Select VR File to Load", "", "vr,tdml");
            if (!path.Equals(""))
            {
                if (Path.GetExtension(path).Equals(".vr"))
                {
                    ReadFile(path, (Controller)target);
                }
                else if (Path.GetExtension(path).Equals(".tdml"))
                {
                    ReadVrFromTDML(path);
                }
            }
            else
            {
                Debug.Log("no new path");
            }
        }
        GUILayout.EndHorizontal();

        this.vc = Camera.main.GetComponent<ViewController>();

        if (GUILayout.Button("Screen Cap"))
        {
            if (this.screen_capture_idx != -1)
            {
                this.screen_capture_idx = -1;
                EditorApplication.update -= OnEditorUpdate;
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Select tdml file for Display Alignment", "", "OK");
                string tdml_path = EditorUtility.OpenFilePanel(
                    "Select tdml for Display Alignment", "", "tdml");
                EditorUtility.DisplayDialog(
                    "Select Base Pathname for Screen Captures", "", "OK");
                this.screen_cap_path = EditorUtility.SaveFilePanel(
                    "Folder Contents Will be Overwritten", "", "screen_cap", "");
                DoScreenCaptures(tdml_path);
            }
        }


        if (GUILayout.Button("Photograph"))
        {
            string path = EditorUtility.SaveFilePanel("Photograph", "", "photo.png", "png");
            if (!path.Equals(""))
            {
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log("screenshot complete");
            }
        }
    }

    private void ReadVrFromTDML(string tdml_path)
    {
        string line;
        JSONNode vr_config = null;

        StreamReader file = new StreamReader(tdml_path);
        while (((line = file.ReadLine()) != null) && ((this.displays == null) || (vr_config == null)))
        {
            JSONNode json = JSONNode.Parse(line);
            if (json["settings"] != null)
            {
                JSONArray contexts = (JSONArray) json["settings"]["contexts"];
                foreach (JSONNode context in contexts)
                {
                    if (context["class"].Value == "vr2")
                    {
                        this.displays = context["display_controllers"];
                        this.views = context["views"];
                    }
                }

                this.tracklength = json["settings"]["track_length"].AsFloat/10.0f;
            }

            if ((json["behavior_mate"] != null) &&
                (json["behavior_mate"]["vr_config"] != null))
            {
                vr_config = json["behavior_mate"]["vr_config"];
            }
        }

        if (vr_config != null)
        {
            CuesFromJson((Controller) target, vr_config, tdml_path.Replace("/", "_"));
        }
   }

   private void DoScreenCaptures(string tdml_path)
   {
        displays = null;
        this.mouse = GameObject.FindGameObjectWithTag("Mouse").GetComponent<MouseController>();

        string[] files = Directory.GetFiles(Path.GetDirectoryName(this.screen_cap_path));
        foreach (string f in files)
        {
            File.Delete(f);
        }

        ReadVrFromTDML(tdml_path);
        SetScreenCapDisplay(0);
        EditorApplication.update += OnEditorUpdate;
    }

    private void SetScreenCapDisplay(int idx)
    {
        this.screen_capture_idx = idx;
        this.pos = 0;

        this.vc.left = this.views[idx]["left"].AsFloat;
        this.vc.right = this.views[idx]["right"].AsFloat;
        this.vc.top = this.views[idx]["top"].AsFloat;
        this.vc.bottom = this.views[idx]["bottom"].AsFloat;
        this.vc.near = this.views[idx]["near"].AsFloat;

        this.mouse.SetViewRotation(views[idx]["rotation"].AsFloat);
    }


    protected virtual void OnEditorUpdate()
    {
        float time = Time.realtimeSinceStartup;
        if ((time - frame_delay) > last_update)
        {
            last_update = time;
            mouse.SetY(pos);
            ScreenCapture.CaptureScreenshot(
                this.screen_cap_path + "_" +
                this.displays[this.screen_capture_idx] + "_" + (pos*10) + ".png");

            if (pos == this.tracklength)
            {
                if (this.screen_capture_idx < this.displays.Count)
                {
                    this.screen_capture_idx += 1;
                    SetScreenCapDisplay(this.screen_capture_idx);
                }
                else
                {
                    this.screen_capture_idx = -1;
                    EditorApplication.update -= OnEditorUpdate;
                }
            }
            else
            {
                pos = pos + 0.5f;
            }
        }
    }


    private static void CuesFromJson(Controller controller, JSONNode json,
                                     string name)
    {
        JSONArray objects = (JSONArray) json["objects"];
        foreach (JSONNode obj in objects)
        {
            Debug.Log(obj["type"]);
            JSONClass tmp = new JSONClass
            {
                ["type"] = "cue",
                ["object"] = obj
            };
            controller.EditContext(name, tmp);
        }

        controller.StartContext(name);
    }



    /// <summary>
    /// Method to write each of the objects currently existing in the Unity
    /// to a JSON file editor
    /// </summary>
    /// <param name="filename">String representing the file to save to. Path
    /// is relative to the project root.</param>
    private static void WriteFile(String filename)
    {
        JSONClass object_list = new JSONClass();
        object_list["objects"] = new JSONArray();
        string materials_path = "Assets/Resources/Materials/";
        string materials_ext = ".mat";

        vrMatePrefab[] cues = (vrMatePrefab[]) UnityEngine
            .Object.FindObjectsOfType<vrMatePrefab>();

        CueCompare name_compare = new CueCompare();
        Array.Sort(cues, name_compare);

        foreach (vrMatePrefab cue in cues)
        {
            if (cue.gameObject.activeInHierarchy)
            {
                // Debug.Log(cue.name + " - " + cue.prefab_name);
                JSONClass obj = new JSONClass();
                obj["type"] = cue.prefab_name;
                obj["id"] = cue.name;
                obj["Position"] = new JSONArray();
                obj["Position"][-1].AsFloat = cue.transform.position.x;
                obj["Position"][-1].AsFloat = cue.transform.position.z;
                obj["Position"][-1].AsFloat = cue.transform.position.y;

                obj["Rotation"] = new JSONArray();
                obj["Rotation"][-1].AsFloat = cue.transform.localEulerAngles.x;
                obj["Rotation"][-1].AsFloat = cue.transform.localEulerAngles.z;
                obj["Rotation"][-1].AsFloat = cue.transform.localEulerAngles.y;

                obj["Scale"] = new JSONArray();
                obj["Scale"][-1].AsFloat = cue.transform.localScale.x;
                obj["Scale"][-1].AsFloat = cue.transform.localScale.z;
                obj["Scale"][-1].AsFloat = cue.transform.localScale.y;

                Renderer renderer;
                if (cue.transform.childCount > 0)
                {
                    renderer = cue.transform.GetChild(0)
                                            .GetComponent<Renderer>();
                }
                else
                {
                    renderer = cue.transform.GetComponent<Renderer>();
                }

                if ((renderer != null) && (renderer.sharedMaterial != null))
                {
                    if (renderer.sharedMaterials.Length > 0)
                    {
                        string mat_path = AssetDatabase.GetAssetPath(renderer.sharedMaterial);
                        if (!mat_path.Equals("")) {
                            obj["material"] = mat_path.Substring(
                                materials_path.Length, mat_path.Length - materials_path.Length - materials_ext.Length);
                        }
                    }
                }

                object_list["objects"][-1] = obj;
            }
        }

        object_list["skybox"] = RenderSettings.skybox.name;
        foreach (ApplyShader shader in GameObject.FindGameObjectWithTag("Mouse").GetComponents<ApplyShader>())
        {
            if (shader.enabled)
            {
                object_list["apply_filter"] = shader.id;
            }
        }
        StreamWriter writer = new StreamWriter(filename, false);

        writer.Write(object_list.ToJSON(1));
        writer.Close();
    }


    private static void ReadFile(String filename, Controller controller)
    {
        string name = filename.Replace("/", "_");
        controller.RemoveAllObjects(name);
        Debug.Log(name);

        StreamReader reader = new StreamReader(filename);
        String file_data = reader.ReadToEnd();
        reader.Close();

        JSONNode json = JSONNode.Parse(file_data);
        JSONArray objects = (JSONArray) json["objects"];
        foreach (JSONNode obj in objects)
        {
            Debug.Log(obj["type"]);
            JSONClass tmp = new JSONClass
            {
                ["type"] = "cue",
                ["object"] = obj
            };
            controller.EditContext(name, tmp);
        }

        if (json["skybox"] != null)
        {
            JSONClass tmp = new JSONClass
            {
                ["type"] = "skybox",
                ["skybox"] = json["skybox"]
            };
            controller.EditContext(name, tmp);
        }
        if (json["apply_filter"] != null)
        {
            JSONClass tmp = new JSONClass
            {
                ["type"] = "filter",
                ["id"] = json["apply_filter"]
            };
            controller.EditContext(name, tmp);
        }

        controller.StartContext(name);
    }



}

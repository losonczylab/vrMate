using UnityEngine;

using System;
using System.Collections.Generic;

/// <summary>
/// Class to contain all of the information regaurding a VR context from
/// BehaviorMate.
/// </summary>
public class Context : MonoBehaviour {

    /// <summary>
    /// The scene name. Must match the name of the scene file in Unity UI.
    /// </summary>
    /// <remarks>
    /// If the scene name is null it will indicate that this context does
    /// not cause Unity to load a new scene when it comes on and just adds
    /// cues into the current scene. Contexts which do not control a sceen
    /// will not result in the VR environment being stopped.
    /// </remarks>
    private String scene;

    /// <summary>
    /// The skybox material to render when this context is displayed.
    /// </summary>
    public Material skybox;

    public List<ApplyShader> shaders = new List<ApplyShader>();

    public static GameObject Create(String name)
    {
        GameObject context = GameObject.Find("/" + name);
        if (context != null)
        {
            Debug.Log("Create - Found");

        }
        else
        {
            context = Instantiate(
                Resources.Load("Prefabs/Context")) as GameObject;
        }
        context.name = name;

        context.GetComponent<Context>().SetScene(null);

        return context;
    }

    public String GetScene()
    {
        if (this.scene == null)
        {
            return null;
        }

        return String.Copy(this.scene);
    }

    public void SetScene(String scene_name)
    {
        if (scene_name != null)
        {
            this.scene = String.Copy(scene_name);
        }
        else
        {
            this.scene = null;
        }
    }

    public void AddFilter(ApplyShader shader)
    {
        this.shaders.Add(shader);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class VrMateEditorWindow
{
    static VrMateEditorWindow ()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            RenderSettings.fog = true;
        }

        GameObject active_object = Selection.activeGameObject;
        if ((active_object != null) &&
            (active_object.GetComponent("vrMatePrefab") == null))
        {
            Transform parent = active_object.transform.parent;
            while (parent != null)
            {
                if (parent.gameObject.GetComponent("vrMatePrefab") != null)
                {
                    Selection.activeGameObject = parent.gameObject;
                    break;
                }

                parent = parent.parent;
            }
        }
    }
}

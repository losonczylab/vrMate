
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {

        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        bool found = false;

        foreach (GameObject obj in objs)
        {
            if ((obj.name == this.name) && (obj != this.gameObject))
            {
                Destroy(this.gameObject);
                found = true;
            }
        }

        if (!found)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

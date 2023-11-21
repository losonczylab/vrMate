using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ApplyShader : MonoBehaviour
{
    [SerializeField]
    private Shader shader;
    private Material material;
    public string id;

    private void Awake()
    {
        // Create a new material with the supplied shader.
        material = new Material(shader);
    }

    // OnRenderImage() is called when the camera has finished rendering.
    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, material);
    }

    public void setValue(string key, float value)
    {
        this.material.SetFloat("_" + key, value);

    }
}

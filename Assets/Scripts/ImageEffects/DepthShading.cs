using UnityEngine;

[ExecuteInEditMode]
public class DepthShading : MonoBehaviour
{
    public Material shadingMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (shadingMaterial != null)
        {
            Graphics.Blit(src, dst, shadingMaterial);
        }
    }
}
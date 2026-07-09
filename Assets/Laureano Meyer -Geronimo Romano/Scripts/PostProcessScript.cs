using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PostProcessScript : MonoBehaviour
{
    [SerializeField] private Material postProcessMaterial;

    private void OnEnable()
    {
        Camera cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, postProcessMaterial);
    }
    public void SetFogIntensity(float value)
    {
        if (postProcessMaterial != null)
        {
            postProcessMaterial.SetFloat("_FogIntensity", value);
        }
    }
    public void SetFogColor(Color color)
    {
        if (postProcessMaterial != null)
        {
            postProcessMaterial.SetColor("_FogColor", color);
        }
    }
}

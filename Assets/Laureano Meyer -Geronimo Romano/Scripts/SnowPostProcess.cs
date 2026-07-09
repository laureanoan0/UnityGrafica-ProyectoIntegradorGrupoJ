using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SnowPostProcess : MonoBehaviour
{
    public Shader shader;

    [Range(0.5f, 1f)] public float snowThreshold = 0.6f;
    [Range(0.001f, 1f)] public float snowSmoothness = 0.2f;
    [Range(0f, 1f)] public float snowOpacity = 1f;
    public Color snowColor = Color.white;

    private float noiseScale = 2f;
    private float noiseThresholdVariation = 0.2f;
    private float noiseColorVariation = 0f;

    private Material mat;
    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    Matrix4x4 GetFrustumCorners(Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        // z = 1 -> corners quedan con componente forward = 1, clave para la reconstruccion
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 1f, Camera.MonoOrStereoscopicEye.Mono, corners);

        // Orden que devuelve Unity: [0]=bottom-left,[1]=top-left,[2]=top-right,[3]=bottom-right
        Vector3 bl = cam.transform.TransformVector(corners[0]);
        Vector3 tl = cam.transform.TransformVector(corners[1]);
        Vector3 tr = cam.transform.TransformVector(corners[2]);
        Vector3 br = cam.transform.TransformVector(corners[3]);

        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, bl);
        m.SetRow(1, tl);
        m.SetRow(2, tr);
        m.SetRow(3, br);
        return m;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (shader == null) { Graphics.Blit(src, dst); return; }
        if (mat == null || mat.shader != shader) mat = new Material(shader);

        mat.SetColor("_SnowColor", snowColor);
        mat.SetFloat("_SnowThreshold", snowThreshold);
        mat.SetFloat("_SnowSmoothness", snowSmoothness);
        mat.SetFloat("_SnowOpacity", snowOpacity);
        mat.SetFloat("_NoiseScale", noiseScale);
        mat.SetFloat("_NoiseStrength", noiseThresholdVariation);
        mat.SetFloat("_NoiseColorVariation", noiseColorVariation);
        mat.SetMatrix("_CamToWorld", cam.cameraToWorldMatrix);
        mat.SetMatrix("_FrustumCorners", GetFrustumCorners(cam));

        Graphics.Blit(src, dst, mat);
    }
}
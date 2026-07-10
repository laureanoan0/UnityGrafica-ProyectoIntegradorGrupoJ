using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeatherController : MonoBehaviour
{
    [Header("Rain")]
    public ParticleSystem rain;
    [Range(0, 1)]
    public float rainIntensity = 1f;
    private float maxEmission = 10000f;
    [Header("Fog Plane")]
    [Range(0, 1)]
    public float fogIntensity = 1f;
    public Material fogPlaneMaterial;
    private float maxFogDensity = 0.6f;

    [Header("Fog Plane Speed")]
    [Range(0, 1)]
    public float fogSpeed = 0.5f;

    [Header("Fog Colors")]
    public Color fogPlaneColor = Color.white;
    public Color postProcessFogColor = Color.gray;
    [Header("Post Process")]
    public PostProcessScript postProcess;
    private float maxPostProcessFog = 5f;
    [Header("Snow")]
    public bool snowEnabled = false;
    public SnowPostProcess snowPostProcess;
    void Update()
    {
        // ---------------- LLUVIA ----------------
        if(rain != null)
        {
            var emission = rain.emission;
            emission.rateOverTime = rainIntensity * maxEmission;
            if (rainIntensity <= 0.001f)
            {
                if (rain.isPlaying)
                    rain.Stop();
            }
            else
            {
                if (!rain.isPlaying)
                    rain.Play();
            }
        }
        // ---------------- NIEBLA DEL PLANO ----------------
        if (fogPlaneMaterial != null)
        {
            fogPlaneMaterial.SetFloat("_FogDensity",
                fogIntensity * maxFogDensity);
            fogPlaneMaterial.SetColor("_FogPlaneColor",
                fogPlaneColor);
            fogPlaneMaterial.SetFloat("_Speed",
                fogSpeed);
        }
        // ---------------- NIEBLA POST PROCESS ----------------
        if (postProcess != null)
        {
            postProcess.SetFogIntensity(
                fogIntensity * maxPostProcessFog);
            postProcess.SetFogColor(
                postProcessFogColor);
        }
        // ---------------- NIEVE ----------------
        if (snowPostProcess != null)
        {
            snowPostProcess.enabled = snowEnabled;
        }
    }
}
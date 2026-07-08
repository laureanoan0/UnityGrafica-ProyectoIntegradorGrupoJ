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

    void Update()
    {
        var emission = rain.emission;

        // Cambia la cantidad de gotas según la intensidad
        emission.rateOverTime = rainIntensity * maxEmission;

        // Si no hay lluvia, dejamos de emitir
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
}
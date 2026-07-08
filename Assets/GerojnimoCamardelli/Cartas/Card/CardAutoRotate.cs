using UnityEngine;
using System.Collections;

/// <summary>
/// Rota automáticamente la carta TCG en una secuencia:
/// 1. Inclina hacia un lado
/// 2. Inclina hacia el otro lado
/// 3. Voltea 180° para mostrar el dorso
/// 4. Vuelve al frente y repite (si loop está activado)
///
/// Colocar este script en el objeto "Carta" (el padre de todo).
/// </summary>
public class CardAutoRotate : MonoBehaviour
{
    [Header("Ángulos de inclinación (grados)")]
    [Tooltip("Cuánto se inclina la carta hacia los costados en el eje Y")]
    public float tiltAngle = 25f;

    [Tooltip("Inclinación opcional en el eje X (arriba/abajo). Dejar en 0 si no se quiere")]
    public float tiltAngleX = 10f;

    [Header("Tiempos (segundos)")]
    [Tooltip("Cuánto tarda cada movimiento de inclinación")]
    public float tiltDuration = 1.2f;

    [Tooltip("Cuánto tarda el giro de 180° (volteo)")]
    public float flipDuration = 1.5f;

    [Tooltip("Pausa entre cada movimiento")]
    public float pauseBetween = 0.5f;

    [Header("Comportamiento")]
    [Tooltip("Si está activo, la secuencia se repite en bucle")]
    public bool loop = true;

    [Tooltip("Curva de suavizado del movimiento (ease in/out por defecto)")]
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Rotación base de la carta al iniciar (su "frente")
    private Quaternion baseRotation;

    void Start()
    {
        baseRotation = transform.localRotation;
        StartCoroutine(RotationSequence());
    }

    IEnumerator RotationSequence()
    {
        do
        {
            // Empezar desde el frente
            yield return RotateTo(baseRotation, tiltDuration);
            yield return Wait();

            // 1. Inclinar hacia un lado (derecha)
            Quaternion tiltRight = baseRotation * Quaternion.Euler(tiltAngleX, tiltAngle, 0f);
            yield return RotateTo(tiltRight, tiltDuration);
            yield return Wait();

            // Volver al frente
            yield return RotateTo(baseRotation, tiltDuration);
            yield return Wait();

            // 2. Inclinar hacia el otro lado (izquierda)
            Quaternion tiltLeft = baseRotation * Quaternion.Euler(tiltAngleX, -tiltAngle, 0f);
            yield return RotateTo(tiltLeft, tiltDuration);
            yield return Wait();

            // Volver al frente
            yield return RotateTo(baseRotation, tiltDuration);
            yield return Wait();

            // 3. Voltear 180° para mostrar el dorso
            Quaternion flipped = baseRotation * Quaternion.Euler(0f, 180f, 0f);
            yield return RotateTo(flipped, flipDuration);
            yield return Wait();

            // Volver al frente (completa el giro o lo deshace)
            yield return RotateTo(baseRotation, flipDuration);
            yield return Wait();

        } while (loop);
    }

    /// <summary>
    /// Interpola suavemente la rotación local hacia un destino.
    /// </summary>
    IEnumerator RotateTo(Quaternion target, float duration)
    {
        Quaternion start = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = easing.Evaluate(t);
            transform.localRotation = Quaternion.Slerp(start, target, eased);
            yield return null;
        }

        transform.localRotation = target;
    }

    IEnumerator Wait()
    {
        if (pauseBetween > 0f)
            yield return new WaitForSeconds(pauseBetween);
    }
}

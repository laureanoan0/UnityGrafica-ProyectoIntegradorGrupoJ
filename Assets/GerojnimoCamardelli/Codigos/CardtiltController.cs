using UnityEngine;

/// <summary>
/// Rota la carta con las flechas/WASD, como si la estuvieras inclinando en la mano, y mueve en
/// conjunto todos los elementos que arman el efecto "diorama": el personaje 3D de adentro, el
/// fondo, y el plano de stencil que recorta al personaje para que solo se vea dentro del marco
/// de la carta.
///
/// Este script NO toca nada del shader holografico (_HoloStrength, _HoloArea, _IsGold,
/// _HoloMovement, el panner con Time, etc.) - eso queda exactamente como estaba antes, animado
/// solo por Time dentro del shader, sin depender de este input.
/// </summary>
public class CardTiltController : MonoBehaviour
{
    [Header("Carta (rota con el input)")]
    [Tooltip("Transform que rota al presionar las flechas/WASD. Si lo dejas vacio, usa este mismo GameObject.")]
    [SerializeField] private Transform cardTransform;

    [Tooltip("Angulo maximo de inclinacion en grados, con la tecla a fondo.")]
    [SerializeField] private float maxTiltAngle = 15f;

    [Header("Plano de Stencil")]
    [Tooltip("Informativo unicamente: el stencil tiene que ser HIJO de Card Transform en la jerarquia. Al ser hijo, hereda la rotacion automaticamente por Unity - este script no le escribe nada.")]
    [SerializeField] private Transform stencilTransform;

    [Header("Personaje 3D (parallax)")]
    [Tooltip("Modelo 3D que se ve 'adentro' de la carta.")]
    [SerializeField] private Transform characterTransform;

    [Tooltip("Cuanto rota el personaje respecto a la carta (1 = igual que la carta, mas alto = exagera la sensacion de profundidad).")]
    [SerializeField] private float characterRotationMultiplier = 1.5f;

    [Tooltip("Cuanto se desplaza el personaje en su plano local al inclinar (simula paralaje/volumen).")]
    [SerializeField] private float characterParallaxStrength = 0.1f;

    [Header("Fondo (parallax)")]
    [Tooltip("Fondo que se ve detras del personaje, dentro de la carta.")]
    [SerializeField] private Transform backgroundTransform;

    [Tooltip("Cuanto se desplaza el fondo al inclinar. Un valor menor al del personaje da sensacion de que esta mas lejos.")]
    [SerializeField] private float backgroundParallaxStrength = 0.03f;

    [Header("Suavizado")]
    [Tooltip("Velocidad de suavizado, tanto al presionar como al soltar la tecla.")]
    [SerializeField] private float tiltSpeed = 4f;

    private Vector2 currentTilt; // -1..1, input suavizado, compartido por todos los elementos

    private Quaternion cardBaseRotation;
    private Quaternion characterBaseRotation;
    private Vector3 characterBasePosition;
    private Vector3 backgroundBasePosition;

    private void Awake()
    {
        if (cardTransform == null) cardTransform = transform;
        cardBaseRotation = cardTransform.rotation;

        // El stencil es hijo de la carta: NO se le aplica rotacion desde este script. Al ser
        // child, ya hereda la rotacion de cardTransform automaticamente por jerarquia de Unity
        // (asi es como Unity compone transforms padre-hijo). Si el script tambien le escribiera
        // rotacion encima, quedaria rotado el doble - que es exactamente el bug que tenias.

        if (characterTransform != null)
        {
            characterBaseRotation = characterTransform.rotation;
            characterBasePosition = characterTransform.localPosition;
        }

        if (backgroundTransform != null)
        {
            backgroundBasePosition = backgroundTransform.localPosition;
        }
    }

    private void Update()
    {
        Vector2 targetTilt = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Suaviza tanto la subida (tecla presionada) como la bajada (tecla soltada), asi todo
        // el conjunto se mueve parejo sin saltos bruscos.
        currentTilt = Vector2.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        ApplyCardTilt(currentTilt);
        ApplyCharacterParallax(currentTilt);
        ApplyBackgroundParallax(currentTilt);
    }

    // Rota la carta principal (el marco/plano con el shader holografico). Usa rotacion MUNDIAL
    // (no local) para que el mismo input produzca el mismo giro visual sin importar la
    // orientacion de origen del objeto. El stencil, al ser hijo de este transform, hereda esta
    // rotacion automaticamente por jerarquia - no necesita codigo propio.
    private void ApplyCardTilt(Vector2 tilt)
    {
        Quaternion tiltDelta = Quaternion.Euler(-tilt.y * maxTiltAngle, tilt.x * maxTiltAngle, 0f);
        cardTransform.rotation = tiltDelta * cardBaseRotation;
    }

    // El personaje rota un poco mas que la carta (characterRotationMultiplier > 1) y ademas se
    // desplaza levemente en su propio plano, para dar sensacion de volumen real en vez de
    // sentirse pegado al marco de la carta. Rotacion MUNDIAL, mismo criterio que la carta: asi
    // gira en la misma direccion percibida sin importar como este orientado el modelo de origen.
    private void ApplyCharacterParallax(Vector2 tilt)
    {
        if (characterTransform == null) return;

        Quaternion tiltDelta = Quaternion.Euler(
            -tilt.y * maxTiltAngle * characterRotationMultiplier,
             tilt.x * maxTiltAngle * characterRotationMultiplier,
             0f);
        characterTransform.rotation = tiltDelta * characterBaseRotation;

        Vector3 offset = new Vector3(tilt.x, tilt.y, 0f) * characterParallaxStrength;
        characterTransform.localPosition = characterBasePosition + offset;
    }

    // El fondo se mueve mas lento que el personaje (backgroundParallaxStrength menor), lo que
    // simula que esta mas lejos, adentro de la carta.
    private void ApplyBackgroundParallax(Vector2 tilt)
    {
        if (backgroundTransform == null) return;

        Vector3 offset = new Vector3(tilt.x, tilt.y, 0f) * backgroundParallaxStrength;
        backgroundTransform.localPosition = backgroundBasePosition + offset;
    }
}
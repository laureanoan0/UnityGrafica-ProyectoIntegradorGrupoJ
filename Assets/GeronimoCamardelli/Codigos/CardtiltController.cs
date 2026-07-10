using UnityEngine;

/// <summary>
/// Rotates the card with arrow keys/WASD, as if tilting it in your hand, and moves all the
/// elements that make up the "diorama" effect together: the 3D character inside, the
/// background, and the stencil plane that clips the character so it only shows within the
/// card frame.
///
/// This script does NOT touch anything in the holographic shader (_HoloStrength, _HoloArea,
/// _IsGold, _HoloMovement, the Time-driven panner, etc.) - that stays exactly as it was,
/// animated only by Time inside the shader, independent of this input.
/// </summary>
public class CardTiltController : MonoBehaviour
{
    [Header("Card (rotates with input)")]
    [Tooltip("Transform that rotates when pressing arrows/WASD. If left empty, uses this same GameObject.")]
    [SerializeField] private Transform cardTransform;

    [Tooltip("Maximum tilt angle in degrees, with the key fully pressed.")]
    [SerializeField] private float maxTiltAngle = 15f;

    [Header("Stencil Plane")]
    [Tooltip("Informational only: the stencil must be a CHILD of Card Transform in the hierarchy. Being a child, it inherits rotation automatically from Unity - this script does not write to it.")]
    [SerializeField] private Transform stencilTransform;

    [Header("3D Character (parallax)")]
    [Tooltip("3D model shown 'inside' the card.")]
    [SerializeField] private Transform characterTransform;

    [Tooltip("How much the character rotates relative to the card (1 = same as the card, higher = exaggerates the sense of depth).")]
    [SerializeField] private float characterRotationMultiplier = 1.5f;

    [Tooltip("How much the character shifts on its own plane when tilting (simulates parallax/volume).")]
    [SerializeField] private float characterParallaxStrength = 0.1f;

    [Header("Background (parallax)")]
    [Tooltip("Background shown behind the character, inside the card.")]
    [SerializeField] private Transform backgroundTransform;

    [Tooltip("How much the background shifts when tilting. A smaller value than the character's gives the sense that it's farther away.")]
    [SerializeField] private float backgroundParallaxStrength = 0.03f;

    [Header("Smoothing")]
    [Tooltip("Smoothing speed, both when pressing and releasing the key.")]
    [SerializeField] private float tiltSpeed = 4f;

    private Vector2 currentTilt; // -1..1, smoothed input, shared by all elements

    private Quaternion cardBaseRotation;
    private Quaternion characterBaseRotation;
    private Vector3 characterBasePosition;
    private Vector3 backgroundBasePosition;

    private void Awake()
    {
        if (cardTransform == null) cardTransform = transform;
        cardBaseRotation = cardTransform.rotation;

        // The stencil is a child of the card: no rotation is applied to it from this script.
        // Being a child, it already inherits cardTransform's rotation automatically through
        // Unity's parent-child hierarchy. If the script also wrote rotation on top of that,
        // it would end up rotated twice.

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

        // Smooths both the rise (key pressed) and the fall (key released), so the whole set
        // moves evenly without sudden jumps.
        currentTilt = Vector2.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        ApplyCardTilt(currentTilt);
        ApplyCharacterParallax(currentTilt);
        ApplyBackgroundParallax(currentTilt);
    }

    // Rotates the main card (the frame/plane with the holographic shader). Uses WORLD rotation
    // (not local) so the same input produces the same visual rotation regardless of the
    // object's original orientation. The stencil, being a child of this transform, inherits
    // this rotation automatically through the hierarchy - it needs no code of its own.
    private void ApplyCardTilt(Vector2 tilt)
    {
        Quaternion tiltDelta = Quaternion.Euler(-tilt.y * maxTiltAngle, tilt.x * maxTiltAngle, 0f);
        cardTransform.rotation = tiltDelta * cardBaseRotation;
    }

    // The character rotates a bit more than the card (characterRotationMultiplier > 1) and
    // also shifts slightly on its own plane, to give a sense of real volume instead of feeling
    // stuck to the card frame. World rotation, same criteria as the card: so it turns in the
    // same perceived direction regardless of the source model's orientation.
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

    // The background moves slower than the character (backgroundParallaxStrength is smaller),
    // which simulates it being farther away, inside the card.
    private void ApplyBackgroundParallax(Vector2 tilt)
    {
        if (backgroundTransform == null) return;

        Vector3 offset = new Vector3(tilt.x, tilt.y, 0f) * backgroundParallaxStrength;
        backgroundTransform.localPosition = backgroundBasePosition + offset;
    }
}
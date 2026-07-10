using UnityEngine;

/// <summary>
/// Goes on the same card prefab (one per design, no longer need separate "normal" and "rare"
/// versions). Turns the holographic effect on/off and picks its area (border/full) by setting
/// float properties on the Amplify shader via MaterialPropertyBlock, so all cards keep sharing
/// the same Material asset (no broken batching, no duplicated materials in memory per
/// instantiated card).
///
/// Shader-side requirements (Amplify):
/// - Float property "_HoloStrength" (Range 0-1): 0 = no holo, 1 = full holo.
/// - Float property "_HoloArea" (Range 0-1): 0 = border mask (BW_Map), 1 = full card.
/// - Float property "_IsGold" (Range 0-1): 0 = rainbow ramp, 1 = gold ramp.
/// - Float property "_HoloMovement" (Range 0-1): 0 = frozen flow map (static holo), 1 = animated.
/// </summary>
public class CardHoloRarity : MonoBehaviour
{
    [Tooltip("Renderer holding the material with the holographic shader. If left empty, finds the first one in children.")]
    [SerializeField] private Renderer holoRenderer;

    [Tooltip("EXACT name of the float property in the shader (including the leading _) that controls holo intensity.")]
    [SerializeField] private string holoStrengthProperty = "_HoloStrength";

    [Tooltip("EXACT name of the float property in the shader (including the leading _) that controls holo area. 0 = border only (BW_Map), 1 = full card.")]
    [SerializeField] private string holoAreaProperty = "_HoloArea";

    [Tooltip("EXACT name of the float property in the shader (including the leading _) that picks the ramp. 0 = rainbow, 1 = gold.")]
    [SerializeField] private string goldProperty = "_IsGold";

    [Tooltip("EXACT name of the float property in the shader (including the leading _) that controls flow map movement. 0 = static, 1 = animated.")]
    [SerializeField] private string movementProperty = "_HoloMovement";

    private MaterialPropertyBlock propBlock;

    public bool IsRare { get; private set; }
    public bool IsFullHolo { get; private set; }
    public bool IsGold { get; private set; }
    public bool HasMovement { get; private set; }

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if (holoRenderer == null) holoRenderer = GetComponentInChildren<Renderer>();
    }

    /// <summary>
    /// Turns this specific card's holographic effect on (true) or off (false).
    /// Call it once, right after Instantiate.
    /// </summary>
    public void SetRarity(bool isRare)
    {
        IsRare = isRare;

        if (holoRenderer == null) return;

        // Important: GetPropertyBlock BEFORE modifying, so as not to overwrite other properties
        // already set (color, _HoloArea, etc.) via property block elsewhere.
        holoRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(holoStrengthProperty, isRare ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// Picks between the two mask variants: border (false) or full card (true).
    /// Independent of SetRarity and doesn't overwrite it (or vice versa): each method does its
    /// own GetPropertyBlock before writing, so they can be called in any order.
    /// </summary>
    public void SetHoloArea(bool isFullHolo)
    {
        IsFullHolo = isFullHolo;

        if (holoRenderer == null) return;

        holoRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(holoAreaProperty, isFullHolo ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// Picks between the two ramp variants: rainbow (false) or gold (true).
    /// Independent of SetRarity/SetHoloArea/SetMovement, can be called in any order.
    /// </summary>
    public void SetGold(bool isGold)
    {
        IsGold = isGold;

        if (holoRenderer == null) return;

        holoRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(goldProperty, isGold ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// Turns the flow map animation on (true) or off (false). When false, the holo stays
    /// visible but static (undistorted UV). Independent of the other methods.
    /// </summary>
    public void SetMovement(bool hasMovement)
    {
        HasMovement = hasMovement;

        if (holoRenderer == null) return;

        holoRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(movementProperty, hasMovement ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);
    }
}
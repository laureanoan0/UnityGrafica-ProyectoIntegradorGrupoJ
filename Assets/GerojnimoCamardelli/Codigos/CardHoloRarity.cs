using UnityEngine;

/// <summary>
/// Va en el mismo prefab de carta (uno solo por diseño, ya no hace falta version
/// "normal" y version "rara" separadas). Prende o apaga el efecto holografico y
/// elige su area (bordes/completa) seteando propiedades float del shader de Amplify
/// via MaterialPropertyBlock, asi todas las cartas siguen compartiendo el mismo
/// Material asset (sin romper batching, sin duplicar materiales en memoria por
/// cada carta instanciada).
///
/// Requisitos del lado del shader (Amplify):
/// - Propiedad float "_HoloStrength" (Range 0-1): 0 = sin holo, 1 = holo completo.
/// - Propiedad float "_HoloArea" (Range 0-1): 0 = mascara de bordes (BW_Map), 1 = carta completa.
/// </summary>
public class CardHoloRarity : MonoBehaviour
{
    [Tooltip("Renderer que tiene el material con el shader holografico. Si lo dejas vacio, busca el primero en hijos.")]
    [SerializeField] private Renderer holoRenderer;

    [Tooltip("Nombre EXACTO de la propiedad float en el shader (con el _ inicial) que controla la intensidad del holo.")]
    [SerializeField] private string holoStrengthProperty = "_HoloStrength";

    [Tooltip("Nombre EXACTO de la propiedad float en el shader (con el _ inicial) que controla el area del holo. 0 = solo bordes (BW_Map), 1 = carta completa.")]
    [SerializeField] private string holoAreaProperty = "_HoloArea";

    private MaterialPropertyBlock propBlock;

    public bool IsRare { get; private set; }
    public bool IsFullHolo { get; private set; }

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        if (holoRenderer == null) holoRenderer = GetComponentInChildren<Renderer>();
    }

    /// <summary>
    /// Prende (true) o apaga (false) el efecto holografico de esta carta puntual.
    /// Llamalo una sola vez, justo despues de Instantiate.
    /// </summary>
    public void SetRarity(bool isRare)
    {
        IsRare = isRare;

        if (holoRenderer == null)
        {
            Debug.LogWarning($"{name}: CardHoloRarity no encontro un Renderer para aplicar el shader holografico.");
            return;
        }

        // Importante: GetPropertyBlock ANTES de modificar, para no pisar otras propiedades
        // que ya le hayan seteado (color, _HoloArea, etc.) via property block en otro lado.
        holoRenderer.GetPropertyBlock(propBlock);

        // Chequeo de seguridad: si el nombre de la propiedad no coincide EXACTO con la
        // "Reference" interna del shader (no con el display name que ves en el Inspector),
        // SetFloat no tira error ni excepcion, simplemente no hace nada. Este check lo saca
        // a la luz en la Console para no perder tiempo adivinando.
        Material mat = holoRenderer.sharedMaterial;
        if (mat != null && !mat.HasProperty(holoStrengthProperty))
        {
            Debug.LogError($"{name}: el shader de '{mat.name}' no tiene una propiedad llamada '{holoStrengthProperty}'. " +
                            "Revisa la Reference exacta en el nodo de la propiedad dentro de Amplify Shader Editor (con el _ inicial incluido).");
        }

        propBlock.SetFloat(holoStrengthProperty, isRare ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);

        Debug.Log($"{name}: CardHoloRarity.SetRarity({isRare}) -> {holoStrengthProperty} = {(isRare ? 1f : 0f)}");
    }

    /// <summary>
    /// Elige entre las dos variantes de mascara: bordes (false) o carta completa (true).
    /// Es independiente de SetRarity y no la pisa (ni al reves): cada metodo hace su propio
    /// GetPropertyBlock antes de escribir, asi que se pueden llamar en cualquier orden.
    /// </summary>
    public void SetHoloArea(bool isFullHolo)
    {
        IsFullHolo = isFullHolo;

        if (holoRenderer == null)
        {
            Debug.LogWarning($"{name}: CardHoloRarity no encontro un Renderer para aplicar el shader holografico.");
            return;
        }

        holoRenderer.GetPropertyBlock(propBlock);

        Material mat = holoRenderer.sharedMaterial;
        if (mat != null && !mat.HasProperty(holoAreaProperty))
        {
            Debug.LogError($"{name}: el shader de '{mat.name}' no tiene una propiedad llamada '{holoAreaProperty}'. " +
                            "Revisa la Reference exacta del nodo _HoloArea en Amplify Shader Editor (con el _ inicial incluido).");
        }

        propBlock.SetFloat(holoAreaProperty, isFullHolo ? 1f : 0f);
        holoRenderer.SetPropertyBlock(propBlock);

        Debug.Log($"{name}: CardHoloRarity.SetHoloArea({isFullHolo}) -> {holoAreaProperty} = {(isFullHolo ? 1f : 0f)}");
    }
}
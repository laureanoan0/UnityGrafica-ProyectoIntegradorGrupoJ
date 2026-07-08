using UnityEngine;

/// <summary>
/// Va en el mismo prefab de carta (uno solo por diseño, ya no hace falta version
/// "normal" y version "rara" separadas). Prende o apaga el efecto holografico
/// seteando una propiedad float del shader de Amplify via MaterialPropertyBlock,
/// asi todas las cartas siguen compartiendo el mismo Material asset (sin romper
/// batching, sin duplicar materiales en memoria por cada carta instanciada).
///
/// Requisito del lado del shader (Amplify): tiene que existir una propiedad float
/// expuesta (por defecto "_HoloStrength", Range 0-1) que multiplique el blend del
/// efecto holografico. En 0 no se ve el holo, en 1 se ve completo.
/// </summary>
public class CardHoloRarity : MonoBehaviour
{
    [Tooltip("Renderer que tiene el material con el shader holografico. Si lo dejas vacio, busca el primero en hijos.")]
    [SerializeField] private Renderer holoRenderer;

    [Tooltip("Nombre EXACTO de la propiedad float en el shader (con el _ inicial) que controla la intensidad del holo.")]
    [SerializeField] private string holoStrengthProperty = "_HoloStrength";

    private MaterialPropertyBlock propBlock;

    public bool IsRare { get; private set; }

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
        // que ya le hayan seteado (color, etc.) via property block en otro lado.
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version 3D del abridor de sobre: Transform normal (no RectTransform), SpriteRenderer para
/// el fade de alpha del sobre (no CanvasGroup), y OnMouseDown para detectar clicks (no IPointerClickHandler).
/// Pensado para objetos con SpriteRenderer + Collider (o Collider2D) en un espacio 3D visto por camara.
///
/// Una sola carta se instancia y renderiza por vez (evita conflictos de Stencil Buffer entre
/// materiales holograficos superpuestos). La rareza (holo si/no) y el area del holo (bordes/completa)
/// se deciden por codigo via CardHoloRarity + MaterialPropertyBlock, no por que prefab se elige:
/// todos los diseños de carta comparten un unico pool (cardPrefabs), y cada slot del sobre tira
/// su propio roll de rareza y de area, independiente del diseño que le toco.
/// </summary>
public class CardPackOpener : MonoBehaviour
{
    [Header("Referencias del sobre")]
    [SerializeField] private Transform packTop;      // parte de arriba, la que se "rompe"
    [SerializeField] private Transform packBottom;    // parte de abajo
    [SerializeField] private Transform packContainer; // padre de PackTop y PackBottom (puede ser este mismo objeto)

    [Header("Cartas")]
    [SerializeField] private GameObject[] cardPrefabs; // un prefab COMPLETO por cada diseño de carta (la rareza ya NO depende del prefab)
    [Range(0f, 1f)]
    [SerializeField] private float rareChance = 0.2f; // probabilidad de que UN slot puntual del sobre salga holografico/rara
    [Range(0f, 1f)]
    [SerializeField] private float fullHoloChance = 0.3f; // de las cartas que salen raras, cuantas usan la mascara "completa" en vez de "bordes"
    [SerializeField] private Transform cardPileParent;
    [SerializeField] private Transform pileRestPoint; // punto donde "descansa" la carta frontal
    [SerializeField] private Transform sideExitPoint;
    [SerializeField] private int cardCount = 5;

    [Header("Tiempos (segundos)")]
    [SerializeField] private float tearDuration = 0.35f;
    [SerializeField] private float packExitDuration = 0.5f;
    [SerializeField] private float cardEnterDuration = 0.5f;
    [SerializeField] private float cardSwipeDuration = 0.3f;

    [Header("Distancias de animacion (ajustar segun la escala de tu escena)")]
    [SerializeField] private Vector3 tearOffset = new Vector3(0f, 3f, 0f);       // cuanto sube PackTop al romperse
    [SerializeField] private Vector3 packExitOffset = new Vector3(0f, -20f, 0f); // cuanto baja el sobre al salir de escena

    [Header("Orden de dibujado del sobre (evita que el sobre tape a las cartas)")]
    [SerializeField] private int packSortingOrder = -10; // el sobre siempre atras

    private void Awake()
    {
        // Forzamos el orden de dibujado del sobre para que quede siempre detras de las cartas,
        // sin importar la distancia real a camara en cada frame de la animacion.
        ForceSortingOrder(packTop, packSortingOrder);
        ForceSortingOrder(packBottom, packSortingOrder);
    }

    private void ForceSortingOrder(Transform t, int order)
    {
        if (t == null) return;
        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = order;
    }

    private enum State { WaitingPackClick, Opening, WaitingCardClick, RevealingNext, Done }
    private State currentState = State.WaitingPackClick;

    private List<GameObject> shuffledDeck;
    private int nextDeckIndex = 0;
    private int nextCardIndex = 0;
    private Transform currentCard;

    // Este mismo GameObject necesita un Collider (o Collider2D) para que esto se dispare.
    private void OnMouseDown()
    {
        if (currentState == State.WaitingPackClick)
        {
            StartCoroutine(OpenPackRoutine());
        }
    }

    public void OnFrontCardClicked()
    {
        if (currentState != State.WaitingCardClick) return;
        StartCoroutine(RevealNextCardRoutine());
    }

    // ---------------- PASO 1: abrir el sobre (todo en paralelo) ----------------

    private IEnumerator OpenPackRoutine()
    {
        currentState = State.Opening;

        // Arranca vacio; se arma (y re-mezcla cuando se agota) a demanda en GetNextCardPrefab(),
        // asi que no importa si cardCount es mayor a la cantidad de diseños cargados.
        shuffledDeck = null;
        nextDeckIndex = 0;
        nextCardIndex = 0;

        // Las tres animaciones arrancan juntas en el mismo instante: la tapa se rompe, el cuerpo
        // del sobre baja, y la primera carta ya empieza a asomar.
        Coroutine tearRoutine = StartCoroutine(TearTopRoutine());
        Coroutine exitRoutine = StartCoroutine(PackExitRoutine());
        Coroutine spawnRoutine = StartCoroutine(SpawnNextCardRoutine());

        yield return tearRoutine;
        yield return exitRoutine;
        yield return spawnRoutine;

        currentState = State.WaitingCardClick;
    }

    // Rompe y desvanece solo la tapa de arriba.
    private IEnumerator TearTopRoutine()
    {
        SpriteRenderer topRenderer = packTop.GetComponent<SpriteRenderer>();
        Vector3 topStart = packTop.position;
        Vector3 topEnd = topStart + tearOffset;
        float startRot = packTop.eulerAngles.z;
        float endRot = startRot + 25f;
        Color topStartColor = topRenderer != null ? topRenderer.color : Color.white;

        float t = 0f;
        while (t < tearDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / tearDuration);
            packTop.position = Vector3.Lerp(topStart, topEnd, p);
            packTop.eulerAngles = new Vector3(0, 0, Mathf.Lerp(startRot, endRot, p));
            if (topRenderer != null)
            {
                Color c = topStartColor;
                c.a = Mathf.Lerp(1f, 0f, p);
                topRenderer.color = c;
            }
            yield return null;
        }
    }

    // Baja y desactiva el cuerpo del sobre (packContainer, que incluye a packBottom como hijo).
    private IEnumerator PackExitRoutine()
    {
        Vector3 packStart = packContainer.position;
        Vector3 packEnd = packStart + packExitOffset;
        float t = 0f;
        while (t < packExitDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / packExitDuration);
            packContainer.position = Vector3.Lerp(packStart, packEnd, p);
            yield return null;
        }
        packContainer.gameObject.SetActive(false);
    }

    // ---------------- PASO 2: instanciar y animar UNA carta por vez ----------------

    // Instancia la siguiente carta del mazo mezclado, decide su rareza y area de holo, y la anima
    // desde el sobre hasta pileRestPoint. Deja el collider deshabilitado hasta que termina de
    // llegar, para que no se pueda clickear a mitad de camino.
    private IEnumerator SpawnNextCardRoutine()
    {
        if (nextCardIndex >= cardCount)
        {
            currentState = State.Done;
            yield break;
        }

        GameObject prefabToUse = GetNextCardPrefab();
        if (prefabToUse == null)
        {
            Debug.LogWarning("No hay prefabs de carta asignados en Card Prefabs.");
            nextCardIndex++;
            yield break;
        }

        GameObject cardGO = Instantiate(prefabToUse, cardPileParent.position, Quaternion.identity, cardPileParent);
        cardGO.SetActive(true);
        currentCard = cardGO.transform;
        nextCardIndex++;

        // La rareza y el area del holo se deciden aca, por slot, independientemente del diseño
        // que salio, y se aplican sobre ESTA instancia via MaterialPropertyBlock. CardHoloRarity
        // vive en un hijo del prefab (ej: CardFrontal), por eso GetComponentInChildren.
        CardHoloRarity holoRarity = cardGO.GetComponentInChildren<CardHoloRarity>();
        if (holoRarity != null)
        {
            bool esRara = Random.value < rareChance;
            holoRarity.SetRarity(esRara);

            // El area solo se nota si la carta es rara (si no es rara, HoloStrength = 0 y no se
            // ve nada de todos modos), pero igual seteamos el valor para dejarlo consistente.
            bool esCompleta = esRara && Random.value < fullHoloChance;
            holoRarity.SetHoloArea(esCompleta);
        }
        else
        {
            Debug.LogWarning($"{cardGO.name}: no tiene CardHoloRarity en su jerarquia, no se pudo asignar rareza.");
        }

        // Enganchamos el click de esta carta puntual al metodo que pasa a la siguiente.
        CardClickRelay relay = cardGO.AddComponent<CardClickRelay>();
        relay.onClicked = OnFrontCardClicked;

        // No interactuable hasta que termine de llegar a destino.
        SetCardInteractable(currentCard, false);

        Vector3 startPos = currentCard.position;
        Vector3 endPos = pileRestPoint.position;

        float t = 0f;
        while (t < cardEnterDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / cardEnterDuration);
            currentCard.position = Vector3.Lerp(startPos, endPos, p);
            yield return null;
        }
        currentCard.position = endPos;

        SetCardInteractable(currentCard, true);
    }

    // Devuelve el siguiente diseño de carta del mazo mezclado, sin repetir hasta que el mazo se
    // agote y se re-mezcle (Fisher-Yates). La rareza NO se decide aca: es independiente del diseño,
    // se resuelve en SpawnNextCardRoutine con un roll aparte via CardHoloRarity.
    private GameObject GetNextCardPrefab()
    {
        if (cardPrefabs == null || cardPrefabs.Length == 0) return null;

        if (shuffledDeck == null || nextDeckIndex >= shuffledDeck.Count)
        {
            shuffledDeck = new List<GameObject>(cardPrefabs);
            for (int i = shuffledDeck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffledDeck[i], shuffledDeck[j]) = (shuffledDeck[j], shuffledDeck[i]);
            }
            nextDeckIndex = 0;
        }

        GameObject result = shuffledDeck[nextDeckIndex];
        nextDeckIndex++;
        return result;
    }

    // ---------------- PASO 3: pasar cartas una por una ----------------

    private IEnumerator RevealNextCardRoutine()
    {
        currentState = State.RevealingNext;

        Transform outgoing = currentCard;
        SetCardInteractable(outgoing, false);

        SpriteRenderer renderer = outgoing.GetComponent<SpriteRenderer>();
        Color startColor = renderer != null ? renderer.color : Color.white;

        Vector3 startPos = outgoing.position;
        Vector3 endPos = sideExitPoint.position;

        float t = 0f;
        while (t < cardSwipeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / cardSwipeDuration);
            outgoing.position = Vector3.Lerp(startPos, endPos, p);
            outgoing.eulerAngles = new Vector3(0, 0, Mathf.Lerp(0f, -20f, p));
            if (renderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, p);
                renderer.color = c;
            }
            yield return null;
        }

        // Destruimos la carta saliente (en vez de SetActive(false) para siempre): como se
        // instancia una nueva por cada carta del sobre, dejarlas desactivadas iba acumulando
        // objetos muertos en memoria sobre tras sobre.
        Destroy(outgoing.gameObject);
        currentCard = null;

        if (nextCardIndex < cardCount)
        {
            yield return StartCoroutine(SpawnNextCardRoutine());
            currentState = State.WaitingCardClick;
        }
        else
        {
            currentState = State.Done;
        }
    }

    // En 3D controlamos si una carta puede recibir click habilitando/deshabilitando su Collider,
    // en vez de CanvasGroup.blocksRaycasts como haciamos en UI.
    private void SetCardInteractable(Transform card, bool interactable)
    {
        Collider col = card.GetComponent<Collider>();
        if (col != null) col.enabled = interactable;

        Collider2D col2D = card.GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = interactable;
    }

    // ---------------- Utilidad: reabrir el sobre (util para probar en el editor / demo) ----------------

    public void ResetPack()
    {
        StopAllCoroutines();

        if (currentCard != null)
        {
            Destroy(currentCard.gameObject);
            currentCard = null;
        }

        nextCardIndex = 0;
        shuffledDeck = null;
        nextDeckIndex = 0;
        currentState = State.WaitingPackClick;

        packContainer.gameObject.SetActive(true);
        packContainer.position -= packExitOffset; // ojo: solo valido si no se movio nada mas mientras tanto

        SpriteRenderer topRenderer = packTop.GetComponent<SpriteRenderer>();
        if (topRenderer != null)
        {
            Color c = topRenderer.color;
            c.a = 1f;
            topRenderer.color = c;
        }
        packTop.eulerAngles = Vector3.zero;
    }
}
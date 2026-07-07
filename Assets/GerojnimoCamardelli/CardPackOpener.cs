using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version 3D del abridor de sobre: Transform normal (no RectTransform), SpriteRenderer para
/// el fade de alpha (no CanvasGroup), y OnMouseDown para detectar clicks (no IPointerClickHandler).
/// Pensado para objetos con SpriteRenderer + Collider (o Collider2D) en un espacio 3D visto por camara.
/// Version simple a proposito: prioriza que funcione sin crashear por sobre ser la solucion optima.
/// </summary>
public class CardPackOpener : MonoBehaviour
{
    [Header("Referencias del sobre")]
    [SerializeField] private Transform packTop;      // parte de arriba, la que se "rompe"
    [SerializeField] private Transform packBottom;    // parte de abajo
    [SerializeField] private Transform packContainer; // padre de PackTop y PackBottom (puede ser este mismo objeto)

    [Header("Cartas")]
    [SerializeField] private GameObject[] cardPrefabs; // un prefab COMPLETO por cada diseño de carta (frente + fondo + objeto 3D ya armados)
    [SerializeField] private Transform cardPileParent;
    [SerializeField] private Transform pileRestPoint;
    [SerializeField] private Transform sideExitPoint;
    [SerializeField] private int cardCount = 5;

    [Header("Tiempos (segundos)")]
    [SerializeField] private float tearDuration = 0.35f;
    [SerializeField] private float packExitDuration = 0.5f;
    [SerializeField] private float pileRiseDuration = 0.5f;
    [SerializeField] private float cardSwipeDuration = 0.3f;

    [Header("Distancias de animacion (ajustar segun la escala de tu escena)")]
    [SerializeField] private Vector3 tearOffset = new Vector3(0f, 3f, 0f);       // cuanto sube PackTop al romperse
    [SerializeField] private Vector3 packExitOffset = new Vector3(0f, -20f, 0f); // cuanto baja el sobre al salir de escena
    [SerializeField] private float cardSpawnSpread = 0.4f; // que tan separadas nacen las cartas del sobre (antes de juntarse en el pilon)

    [Header("Orden de dibujado (evita que el sobre tape a las cartas)")]
    [SerializeField] private int packSortingOrder = -10;    // el sobre siempre atras
    [SerializeField] private int cardBaseSortingOrder = 100; // las cartas siempre adelante del sobre

    private void Awake()
    {
        // Forzamos el orden de dibujado del sobre para que quede siempre detras de las cartas,
        // sin importar la distancia real a camara en cada frame de la animacion (eso es lo que
        // causaba que la tapa se viera por delante de las cartas al cruzarse en el espacio 3D).
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

    private readonly List<Transform> spawnedCards = new List<Transform>();
    private int frontCardIndex = 0;

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

        // Las tres animaciones arrancan juntas en el mismo instante:
        // la tapa se rompe, el cuerpo del sobre baja, y las cartas ya empiezan a asomar.
        // Antes esperabamos que terminara cada una para recien arrancar la siguiente (por eso
        // las cartas parecian salir "de la nada" una vez que el sobre ya habia desaparecido).
        Coroutine tearRoutine = StartCoroutine(TearTopRoutine());
        Coroutine exitRoutine = StartCoroutine(PackExitRoutine());
        Coroutine riseRoutine = StartCoroutine(SpawnAndRiseCards());

        // Esperamos a que las tres hayan terminado antes de habilitar el click sobre la carta.
        yield return tearRoutine;
        yield return exitRoutine;
        yield return riseRoutine;

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

    private IEnumerator SpawnAndRiseCards()
    {
        Vector3 spawnPos = cardPileParent.position;

        // Armamos un orden mezclado de que prefab le toca a cada posicion, para que las 5
        // cartas del sobre no salgan repetidas (mientras tengas 5 o mas prefabs cargados).
        List<GameObject> shuffledPrefabs = BuildShuffledPrefabList();

        for (int i = 0; i < cardCount; i++)
        {
            GameObject prefabToUse = shuffledPrefabs.Count > 0 ? shuffledPrefabs[i % shuffledPrefabs.Count] : null;
            if (prefabToUse == null)
            {
                Debug.LogWarning("No hay prefabs de carta asignados en Card Prefabs.");
                continue;
            }

            GameObject cardGO = Instantiate(prefabToUse, cardPileParent);
            cardGO.SetActive(true);

            Transform ct = cardGO.transform;
            ct.position = spawnPos + new Vector3(Random.Range(-cardSpawnSpread, cardSpawnSpread), Random.Range(-cardSpawnSpread, cardSpawnSpread), 0f);
            ct.eulerAngles = new Vector3(0, 0, Random.Range(-3f, 3f));

            // Sorting order alto y decreciente por indice: la carta 0 (la que se ve primero)
            // queda con el numero mas alto, asegurando que este por delante de TODO el sobre
            // y tambien por delante de las demas cartas del mazo.
            SpriteRenderer cardRenderer = cardGO.GetComponent<SpriteRenderer>();
            if (cardRenderer != null) cardRenderer.sortingOrder = cardBaseSortingOrder + (cardCount - i);

            // Enganchamos el click de esta carta puntual al metodo que pasa a la siguiente.
            CardClickRelay relay = cardGO.AddComponent<CardClickRelay>();
            relay.onClicked = OnFrontCardClicked;

            spawnedCards.Add(ct);
        }

        // En UI usabamos el orden de la jerarquia para decidir que carta se ve "encima".
        // En 3D usamos el eje Z: la carta 0 (primera en revelarse) queda mas cerca de la camara.
        SetCardZOrder();

        SetCardInteractable(spawnedCards[0], true);
        for (int i = 1; i < spawnedCards.Count; i++)
            SetCardInteractable(spawnedCards[i], false);

        // Cada carta tiene su propio destino: mismo X/Y que PileRestPoint, pero con
        // un pequenio escalonado en Z (basado en su indice) para que se sigan viendo apiladas
        // incluso una vez que llegaron a destino.
        Vector3 targetBase = pileRestPoint.position;
        Vector3[] startPositions = new Vector3[spawnedCards.Count];
        Vector3[] targetPositions = new Vector3[spawnedCards.Count];
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            startPositions[i] = spawnedCards[i].position;
            targetPositions[i] = new Vector3(targetBase.x, targetBase.y, targetBase.z + i * 0.01f);
        }

        float t = 0f;
        while (t < pileRiseDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / pileRiseDuration);
            for (int i = 0; i < spawnedCards.Count; i++)
            {
                // Ahora SI viaja en X, Y y Z: si PileRestPoint esta mas cerca de la camara
                // que el punto de spawn, la carta se va a acercar de verdad, no solo "flotar".
                spawnedCards[i].position = Vector3.Lerp(startPositions[i], targetPositions[i], p);
            }
            yield return null;
        }

        for (int i = 0; i < spawnedCards.Count; i++)
            spawnedCards[i].position = targetPositions[i];

        frontCardIndex = 0;
    }

    // Devuelve una copia mezclada de cardPrefabs (Fisher-Yates), para no repetir diseños
    // mientras el mazo alcance.
    private List<GameObject> BuildShuffledPrefabList()
    {
        List<GameObject> deck = new List<GameObject>(cardPrefabs);
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
        return deck;
    }

    // Acomoda el eje Z de cada carta para que la carta "de adelante" quede mas cerca de la camara.
    // Asume una camara mirando hacia +Z. Si tu camara mira para el otro lado, invertí el signo.
    private void SetCardZOrder()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            Vector3 pos = spawnedCards[i].position;
            pos.z = i * 0.01f;
            spawnedCards[i].position = pos;
        }
    }

    // ---------------- PASO 3: pasar cartas una por una ----------------

    private IEnumerator RevealNextCardRoutine()
    {
        currentState = State.RevealingNext;

        Transform current = spawnedCards[frontCardIndex];
        SetCardInteractable(current, false);

        SpriteRenderer renderer = current.GetComponent<SpriteRenderer>();
        Color startColor = renderer != null ? renderer.color : Color.white;

        Vector3 startPos = current.position;
        Vector3 endPos = sideExitPoint.position;

        float t = 0f;
        while (t < cardSwipeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / cardSwipeDuration);
            current.position = Vector3.Lerp(startPos, endPos, p);
            current.eulerAngles = new Vector3(0, 0, Mathf.Lerp(0f, -20f, p));
            if (renderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, p);
                renderer.color = c;
            }
            yield return null;
        }
        current.gameObject.SetActive(false);

        frontCardIndex++;

        if (frontCardIndex < spawnedCards.Count)
        {
            Transform next = spawnedCards[frontCardIndex];
            SetCardInteractable(next, true);
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
}
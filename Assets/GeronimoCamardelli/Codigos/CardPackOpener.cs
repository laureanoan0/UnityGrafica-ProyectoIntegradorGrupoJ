using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 3D version of the pack opener: regular Transform (not RectTransform), SpriteRenderer for
/// the pack's alpha fade (not CanvasGroup), and OnMouseDown to detect clicks (not
/// IPointerClickHandler). Meant for objects with a SpriteRenderer + Collider (or Collider2D)
/// in a 3D space viewed by a camera.
///
/// Only one card is instantiated and rendered at a time (avoids Stencil Buffer conflicts
/// between overlapping holographic materials). Rarity (holo on/off) and holo area
/// (border/full) are decided in code via CardHoloRarity + MaterialPropertyBlock, not by which
/// prefab is chosen: all card designs share a single pool (cardPrefabs), and each pack slot
/// rolls its own rarity and area independently of the design it got.
/// </summary>
public class CardPackOpener : MonoBehaviour
{
    [Header("Pack references")]
    [SerializeField] private Transform packTop;      // top part, the one that "tears"
    [SerializeField] private Transform packBottom;    // bottom part
    [SerializeField] private Transform packContainer; // parent of PackTop and PackBottom (can be this same object)

    [Header("Cards")]
    [SerializeField] private GameObject[] cardPrefabs; // one COMPLETE prefab per card design (rarity no longer depends on the prefab)
    [Range(0f, 1f)]
    [SerializeField] private float rareChance = 0.2f; // probability that ONE specific pack slot turns out holo/rare
    [Range(0f, 1f)]
    [SerializeField] private float fullHoloChance = 0.3f; // of the cards that turn out rare, how many use the "full" mask instead of "border"
    [Range(0f, 1f)]
    [SerializeField] private float goldChance = 0.25f; // of the cards that turn out rare, how many use the gold ramp instead of rainbow
    [Range(0f, 1f)]
    [SerializeField] private float staticChance = 0.2f; // of the cards that turn out rare, how many end up WITHOUT movement (static holo)
    [SerializeField] private Transform cardPileParent;
    [SerializeField] private Transform pileRestPoint; // point where the front card "rests"
    [SerializeField] private Transform sideExitPoint;
    [SerializeField] private int cardCount = 5;

    [Header("Timings (seconds)")]
    [SerializeField] private float tearDuration = 0.35f;
    [SerializeField] private float packExitDuration = 0.5f;
    [SerializeField] private float cardEnterDuration = 0.5f;
    [SerializeField] private float cardSwipeDuration = 0.3f;

    [Header("Animation distances (adjust to your scene scale)")]
    [SerializeField] private Vector3 tearOffset = new Vector3(0f, 3f, 0f);       // how much PackTop rises when torn
    [SerializeField] private Vector3 packExitOffset = new Vector3(0f, -20f, 0f); // how much the pack drops when leaving the scene

    [Header("Pack draw order (keeps the pack from covering the cards)")]
    [SerializeField] private int packSortingOrder = -10; // pack always behind

    private void Awake()
    {
        // Force the pack's draw order so it always stays behind the cards, regardless of the
        // actual distance to camera on any given animation frame.
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

    // Fired once this pack has revealed every card it had (cardCount reached). Used by
    // PackQueueController to know when to advance the row and promote the next pack.
    public event Action OnPackFinished;

    // Only the pack in the active/center slot should respond to clicks. Defaults to true so a
    // single standalone pack (no PackQueueController involved) keeps working exactly as before.
    // A queue manager sets this to false on every pack sitting in the waiting row.
    private bool isActive = true;

    public void SetActive(bool active) => isActive = active;

    private List<GameObject> shuffledDeck;
    private int nextDeckIndex = 0;
    private int nextCardIndex = 0;
    private Transform currentCard;

    // This same GameObject needs a Collider (or Collider2D) for this to fire.
    private void OnMouseDown()
    {
        if (!isActive) return;
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

    // ---------------- STEP 1: open the pack (all in parallel) ----------------

    private IEnumerator OpenPackRoutine()
    {
        currentState = State.Opening;

        // Starts empty; it gets built (and reshuffled once exhausted) on demand in
        // GetNextCardPrefab(), so it doesn't matter if cardCount is larger than the number of
        // loaded designs.
        shuffledDeck = null;
        nextDeckIndex = 0;
        nextCardIndex = 0;

        // The three animations start together at the same instant: the flap tears, the pack
        // body drops, and the first card already starts to peek out.
        Coroutine tearRoutine = StartCoroutine(TearTopRoutine());
        Coroutine exitRoutine = StartCoroutine(PackExitRoutine());
        Coroutine spawnRoutine = StartCoroutine(SpawnNextCardRoutine());

        yield return tearRoutine;
        yield return exitRoutine;
        yield return spawnRoutine;

        currentState = State.WaitingCardClick;
    }

    // Tears and fades out only the top flap.
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

    // Lowers and deactivates the pack body (packContainer, which includes packBottom as a child).
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

    // ---------------- STEP 2: instantiate and animate ONE card at a time ----------------

    // Instantiates the next card from the shuffled deck, decides its rarity and holo area, and
    // animates it from the pack to pileRestPoint. Leaves the collider disabled until it
    // finishes arriving, so it can't be clicked mid-transit.
    private IEnumerator SpawnNextCardRoutine()
    {
        if (nextCardIndex >= cardCount)
        {
            currentState = State.Done;
            OnPackFinished?.Invoke();
            yield break;
        }

        GameObject prefabToUse = GetNextCardPrefab();
        if (prefabToUse == null)
        {
            nextCardIndex++;
            yield break;
        }

        GameObject cardGO = Instantiate(prefabToUse, cardPileParent.position, Quaternion.identity, cardPileParent);
        cardGO.SetActive(true);
        currentCard = cardGO.transform;
        nextCardIndex++;

        // Rarity and holo area are decided here, per slot, independently of the design that
        // got picked, and applied to THIS instance via MaterialPropertyBlock. CardHoloRarity
        // lives on a child of the prefab (e.g. CardFrontal), hence GetComponentInChildren.
        CardHoloRarity holoRarity = cardGO.GetComponentInChildren<CardHoloRarity>();
        if (holoRarity != null)
        {
            bool isRare = Random.value < rareChance;
            holoRarity.SetRarity(isRare);

            // Area only matters visually if the card is rare (if not rare, HoloStrength = 0
            // and nothing shows anyway), but it's still set to keep the material state consistent.
            bool isFull = isRare && Random.value < fullHoloChance;
            holoRarity.SetHoloArea(isFull);

            // Same criteria: only matters visually if the card is rare, but always set to keep
            // the material in a consistent state.
            bool isGold = isRare && Random.value < goldChance;
            holoRarity.SetGold(isGold);

            bool hasMovement = !(isRare && Random.value < staticChance);
            holoRarity.SetMovement(hasMovement);
        }

        // Hook this specific card's click to the method that advances to the next one.
        CardClickRelay relay = cardGO.AddComponent<CardClickRelay>();
        relay.onClicked = OnFrontCardClicked;

        // Not interactable until it finishes arriving at its destination.
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

    // Returns the next card design from the shuffled deck, without repeating until the deck
    // runs out and gets reshuffled (Fisher-Yates). Rarity is NOT decided here: it's independent
    // of the design, resolved separately in SpawnNextCardRoutine via CardHoloRarity.
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

    // ---------------- STEP 3: pass cards one at a time ----------------

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

        // Destroy the outgoing card (instead of SetActive(false) forever): since a new one is
        // instantiated for every card in the pack, leaving them disabled would keep piling up
        // dead objects in memory pack after pack.
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
            OnPackFinished?.Invoke();
        }
    }

    // In 3D we control whether a card can receive a click by enabling/disabling its Collider,
    // instead of CanvasGroup.blocksRaycasts like in UI.
    private void SetCardInteractable(Transform card, bool interactable)
    {
        Collider col = card.GetComponent<Collider>();
        if (col != null) col.enabled = interactable;

        Collider2D col2D = card.GetComponent<Collider2D>();
        if (col2D != null) col2D.enabled = interactable;
    }

    // ---------------- Utility: reopen the pack (useful for testing in the editor / demo) ----------------

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
        packContainer.position -= packExitOffset; // note: only valid if nothing else moved in the meantime

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
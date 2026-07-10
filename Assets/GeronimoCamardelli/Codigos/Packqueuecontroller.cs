using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages several packs waiting in a row at the bottom of the scene. The pack sitting in the
/// active/center slot is the only one that responds to clicks (via CardPackOpener.SetActive).
/// When it finishes revealing all of its cards (CardPackOpener.OnPackFinished), it gets
/// removed, the whole row slides forward by one slot, the next pack in line becomes active,
/// and a fresh pack appears at the far end of the row if the box still has any left.
/// </summary>
public class PackQueueController : MonoBehaviour
{
    [Header("Pack prefab")]
    [Tooltip("Prefab with a CardPackOpener component. Must be self-contained (its own PackTop/PackBottom/card references already set on the prefab).")]
    [SerializeField] private CardPackOpener packPrefab;

    [Header("Slots")]
    [Tooltip("Position where the currently-opening pack sits (center, the only interactable one).")]
    [SerializeField] private Transform activeSlot;

    [Tooltip("Side positions, in order: index 0 is the next pack up (closest to the active slot), the last index is the farthest one.")]
    [SerializeField] private Transform[] queueSlots;

    [Header("Box")]
    [Tooltip("Total number of packs available to open. Set to -1 for unlimited (keeps spawning new ones as the row empties).")]
    [SerializeField] private int totalPacksInBox = 10;

    [Header("Timing")]
    [Tooltip("How long it takes for the row to slide forward by one slot.")]
    [SerializeField] private float slideDuration = 0.4f;

    private CardPackOpener currentActivePack;
    private readonly List<CardPackOpener> waitingQueue = new List<CardPackOpener>();
    private int packsSpawnedSoFar = 0;

    private void Start()
    {
        currentActivePack = SpawnPackAt(activeSlot);
        if (currentActivePack != null)
        {
            currentActivePack.SetActive(true);
            currentActivePack.OnPackFinished += HandleActivePackFinished;
        }

        // Fill every side slot, front to back, stopping early if the box runs out.
        foreach (Transform slot in queueSlots)
        {
            CardPackOpener pack = SpawnPackAt(slot);
            if (pack == null) break;

            pack.SetActive(false);
            waitingQueue.Add(pack);
        }
    }

    private CardPackOpener SpawnPackAt(Transform slot)
    {
        if (totalPacksInBox >= 0 && packsSpawnedSoFar >= totalPacksInBox) return null;
        if (packPrefab == null || slot == null) return null;

        CardPackOpener pack = Instantiate(packPrefab, slot.position, slot.rotation);
        packsSpawnedSoFar++;
        return pack;
    }

    private void HandleActivePackFinished()
    {
        currentActivePack.OnPackFinished -= HandleActivePackFinished;
        Destroy(currentActivePack.gameObject);
        currentActivePack = null;

        StartCoroutine(AdvanceQueueRoutine());
    }

    private IEnumerator AdvanceQueueRoutine()
    {
        if (waitingQueue.Count == 0) yield break;

        // The pack at the front of the line becomes the new active pack.
        CardPackOpener nextActive = waitingQueue[0];
        waitingQueue.RemoveAt(0);

        List<Coroutine> slides = new List<Coroutine>
        {
            StartCoroutine(SlidePack(nextActive.transform, activeSlot.position, activeSlot.rotation))
        };

        // Everyone still waiting moves up one slot at the same time (index i now points to the
        // slot that used to belong to index i+1, since the front pack was just removed).
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            Transform targetSlot = queueSlots[i];
            slides.Add(StartCoroutine(SlidePack(waitingQueue[i].transform, targetSlot.position, targetSlot.rotation)));
        }

        // If the box still has packs left, a new one appears at the far end of the row.
        Transform lastSlot = queueSlots[queueSlots.Length - 1];
        CardPackOpener newPack = SpawnPackAt(lastSlot);
        if (newPack != null)
        {
            newPack.SetActive(false);
            waitingQueue.Add(newPack);
        }

        foreach (Coroutine slide in slides)
        {
            yield return slide;
        }

        currentActivePack = nextActive;
        currentActivePack.SetActive(true);
        currentActivePack.OnPackFinished += HandleActivePackFinished;
    }

    private IEnumerator SlidePack(Transform pack, Vector3 targetPosition, Quaternion targetRotation)
    {
        Vector3 startPosition = pack.position;
        Quaternion startRotation = pack.rotation;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / slideDuration);
            pack.position = Vector3.Lerp(startPosition, targetPosition, p);
            pack.rotation = Quaternion.Slerp(startRotation, targetRotation, p);
            yield return null;
        }

        pack.position = targetPosition;
        pack.rotation = targetRotation;
    }
}
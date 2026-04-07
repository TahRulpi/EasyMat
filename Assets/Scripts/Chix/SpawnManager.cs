using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SpawnManager : MonoBehaviour
{
    [Header("Holes")]
    public Transform[] holes;

    [Header("Prefabs")]
    public GameObject YH; public GameObject BH;
    public GameObject GH; public GameObject Bomb;

    [Header("Settings")]
    public float spawnDelay = 1.2f;
    public float visibleDuration = 2.0f;
    public float popupHeight = 60f;
    public float animDuration = 0.4f;

    private int lastHoleIndex = -1;
    private Coroutine spawnRoutine;

    void Start()
    {
        DOTween.Init();
       // spawnRoutine = StartCoroutine(SpawnLoop());
    }


    public void SpawnFirstYellowChicken()
    {
        int holeIndex = Random.Range(0, holes.Length);
        Transform hole = holes[holeIndex];

        if (hole.childCount > 0) return;

        GameObject chicken = Instantiate(YH, hole);

        RectTransform rect = chicken.GetComponent<RectTransform>();

        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.zero;

        rect.DOAnchorPos(new Vector2(0, popupHeight), animDuration).SetEase(Ease.OutBack);
        rect.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
    }

    /*IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnChicken();

            float difficultyMult = ChixGameManager.Instance != null ? ChixGameManager.Instance.spawnSpeedMultiplier : 1f;

            // Base delay divided by difficulty
            float delay = spawnDelay / difficultyMult;

            // --- THE FIX ---
            // 1. Removed the "delay *= 2.5f" block entirely.
            // 2. Switched to WaitForSecondsRealtime so the freeze doesn't 
            //    accidentally stretch the spawn timer.

            yield return new WaitForSeconds(delay);
        }
    }*/

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float difficultyMult = ChixGameManager.Instance != null ? ChixGameManager.Instance.spawnSpeedMultiplier : 1f;
            float delay = spawnDelay / difficultyMult;

            yield return new WaitForSeconds(delay); // ? handles pause automatically

            if (!ChixGameManager.Instance.isBossMode)
            {
                SpawnChicken();
            }
        }
    }

    void SpawnChicken()
    {
        int holeIndex = GetRandomHole();
        Transform hole = holes[holeIndex];

        // Don't spawn if a chicken is already there
        if (hole.childCount > 0) return;

        GameObject prefab = GetWeightedChicken();
        GameObject chicken = Instantiate(prefab, hole);

        RectTransform rect = chicken.GetComponent<RectTransform>();
        Chicken script = chicken.GetComponent<Chicken>();

        // Apply speed multiplier (Green chickens are naturally faster)
        float speed = animDuration * (script != null ? script.speedMultiplier : 1f);

        // Adjust animation speed based on global difficulty
        float difficultyMult = ChixGameManager.Instance != null ? ChixGameManager.Instance.spawnSpeedMultiplier : 1f;
        speed /= difficultyMult;

        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.zero;

        // Pop up animation
        rect.DOAnchorPos(new Vector2(0, popupHeight), speed).SetEase(Ease.OutBack);
        rect.DOScale(Vector3.one, speed).SetEase(Ease.OutBack);

        StartCoroutine(DespawnChicken(chicken, rect, script));
    }


    /*IEnumerator DespawnChicken(GameObject chicken, RectTransform rect, Chicken script)
    {
        float difficultyMult = ChixGameManager.Instance != null ? ChixGameManager.Instance.spawnSpeedMultiplier : 1f;
        float adjustedDuration = visibleDuration / difficultyMult;

        yield return new WaitForSeconds(adjustedDuration);

        if (chicken != null)
        {
            // 1. Check if the chicken script still thinks it hasn't been clicked
            // Note: You need to make 'isClicked' public in Chicken.cs or add a getter.
            if (script != null && script.type == Chicken.ChickenType.Yellow && !script.IsClicked())
            {
                ChixGameManager.Instance.MissYellow();
            }

            rect.DOKill();
            rect.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.InBack);
            rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (chicken != null) Destroy(chicken);
            });
        }
    }*/


    IEnumerator DespawnChicken(GameObject chicken, RectTransform rect, Chicken script)
    {
        float difficultyMult = ChixGameManager.Instance != null ? ChixGameManager.Instance.spawnSpeedMultiplier : 1f;
        float adjustedDuration = visibleDuration / difficultyMult;

        // Use WaitForSeconds so the chicken stays visible while paused!
        yield return new WaitForSeconds(adjustedDuration);

        if (chicken != null)
        {
            // Only trigger a Miss if the chicken wasn't clicked
            if (script != null && script.type == Chicken.ChickenType.Yellow && !script.IsClicked())
            {
                ChixGameManager.Instance.MissYellow();
            }

            rect.DOKill();
            rect.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.InBack);
            rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (chicken != null) Destroy(chicken);
            });
        }
    }

    int GetRandomHole()
    {
        int index;
        do { index = Random.Range(0, holes.Length); } while (index == lastHoleIndex);
        lastHoleIndex = index;
        return index;
    }

    GameObject GetWeightedChicken()
    {
        float rand = Random.Range(0f, 100f);

        // 94% Chance for Yellow
        if (rand < 94f) return YH;

        // 2% Chance for Blue
        if (rand < 96f) return BH;

        // 1% Chance for Green
        if (rand < 97f) return GH;

        // 3% Chance for Bomb
        return Bomb;
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null; // ?? MUST
        }
    }
    /*public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }*/

    public void StartSpawning()
    {
        StopSpawning(); // ?? ALWAYS reset first
        spawnRoutine = StartCoroutine(SpawnLoop());
    }
}
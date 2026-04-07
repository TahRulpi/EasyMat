using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class EggSpawner : MonoBehaviour
{
    [Header("AR Ready List")]
    public List<EggBehavior> spawnedEggs = new List<EggBehavior>();

    [Header("Map Settings")]
    public AbstractMap map;
    public Transform player;

    /*[System.Serializable]
    public class EggData
    {
        public EggType eggType;     // Red, Green, Purple, Golden
        public double latitude;
        public double longitude;
    }*/

    [Header("Egg Prefabs")]
    public GameObject redEggPrefab;
    public GameObject greenEggPrefab;
    public GameObject purpleEggPrefab;
    public GameObject goldenEggPrefab;

    [Header("Egg Input List (GPS based)")]
    public List<EggData> eggInputList = new List<EggData>();

    private bool spawned = false;

    void Awake()
    {
        Debug.Log("?? EggSpawner AWAKE");
    }

    IEnumerator Start()
    {
        Debug.Log("?? EggSpawner START (Coroutine)");

        // Wait for map reference
        while (map == null)
            yield return null;

        // Wait until Mapbox finishes loading tiles
        while (map.MapVisualizer == null || map.MapVisualizer.ActiveTiles.Count == 0)
            yield return null;

        Debug.Log("?? Map READY ? Spawning eggs");
        SpawnEggs();
    }

    void SpawnEggs()
    {
        if (spawned) return;
        spawned = true;

        spawnedEggs.Clear();

        if (eggInputList == null || eggInputList.Count == 0)
        {
            Debug.LogWarning("? No egg input provided!");
            return;
        }

        foreach (EggData data in eggInputList)
        {
            if (data.isCollected) continue; // <-- skip if already collected in AR
            GameObject prefab = GetPrefabByType(data.eggType);
            if (prefab == null) continue;

            Vector2d gpsPos = new Vector2d(data.latitude, data.longitude);
            Vector3 worldPos = map.GeoToWorldPosition(gpsPos, true);
            worldPos.y += 2f; // Lift above map

            GameObject egg = Instantiate(prefab, worldPos, Quaternion.identity);
            egg.transform.SetParent(map.transform, true);

            // Start bounce animation
            StartCoroutine(BounceEgg(egg.transform));

            EggBehavior behavior = egg.GetComponent<EggBehavior>();
            if (behavior != null)
            {
                behavior.geoPosition = gpsPos;
                behavior.map = map;
                behavior.player = player;
                behavior.spawnTime = DateTime.Now;
                behavior.eggType = data.eggType;
                behavior.isCollectable = true;

                spawnedEggs.Add(behavior);
            }

            Debug.Log($"Egg spawned: {data.eggType} @ {gpsPos}");
        }



    }

    IEnumerator BounceEgg(Transform egg)
    {
        egg.localScale = Vector3.zero;
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one * 1300f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(elapsed / duration * Mathf.PI * 0.5f);
            egg.localScale = targetScale * t;
            yield return null;
        }
        egg.localScale = targetScale;

        // Small up-and-down bounce
        Vector3 startPos = egg.position;
        float bounceHeight = 1f;
        float bounceSpeed = 2f;
        for (int i = 0; i < 2; i++)
        {
            float upTime = 0f;
            while (upTime < 0.3f)
            {
                upTime += Time.deltaTime * bounceSpeed;
                egg.position = startPos + Vector3.up * Mathf.Sin(upTime * Mathf.PI) * bounceHeight;
                yield return null;
            }
        }
        egg.position = startPos;
    }

    GameObject GetPrefabByType(EggType type)
    {
        switch (type)
        {
            case EggType.Red: return redEggPrefab;
            case EggType.Green: return greenEggPrefab;
            case EggType.Purple: return purpleEggPrefab;
            case EggType.Golden: return goldenEggPrefab;
            default: return redEggPrefab;
        }
    }
}

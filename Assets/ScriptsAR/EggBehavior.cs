/*using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using UnityEngine;

public class EggBehavior : MonoBehaviour
{
    [Header("Mapbox")]
    public AbstractMap map;
    public Transform player;

    public DateTime spawnTime;

    [Header("Egg GPS Position")]
    public Vector2d geoPosition;

    [Header("AR Settings")]
    public float arDistance = 5f;

    [Header("Distance Settings")]
    public float visibleDistance = 50f;

    private Renderer eggRenderer;
    private Material eggMaterial;

    [Header("Egg Info")]
    public EggType eggType;
    public bool isCollectable = true;

    [HideInInspector] public bool isCollected;

    [Header("AR Mode")]
    public bool isARMode = false; // NEW: stop Mapbox movement in AR




    void Start()
    {
        // AR Camera = Player
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }

        // Map reference
        if (map == null)
        {
            map = FindObjectOfType<AbstractMap>();
        }

        // Subscribe to AR collection event
        EggManager.OnEggCollected += (collectedEgg) =>
        {
            if (Vector2d.Distance(collectedEgg.LatLon, geoPosition) < 0.00001)
                Destroy(gameObject);
        };

        // Renderer & material
        eggRenderer = GetComponentInChildren<Renderer>();
        if (eggRenderer != null)
        {
            eggMaterial = eggRenderer.material;
            eggMaterial.DisableKeyword("_EMISSION");
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        // Map eggs do not move in AR
        if (isARMode) return;

        if (player == null || map == null) return;

        Vector3 worldPos = map.GeoToWorldPosition(geoPosition, true);
        worldPos.y += 0.5f; // lift above ground
        transform.position = worldPos;
    }

    // Optional: CanSeeEgg logic
    public bool CanSeeEgg()
    {
        SubscriptionTier tier = GameManager.Instance.currentTier;
        switch (tier)
        {
            case SubscriptionTier.None:
                return eggType == EggType.Red || eggType == EggType.Green;
            case SubscriptionTier.Pro:
            case SubscriptionTier.Premium:
                return true;
            default:
                return false;
        }
    }
}
*/

using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using UnityEngine;

public class EggBehavior : MonoBehaviour
{
    [Header("Mapbox")]
    public AbstractMap map;
    public Transform player;

    public DateTime spawnTime;

    [Header("Egg GPS Position")]
    public Vector2d geoPosition;

    [Header("AR Settings")]
    public float arDistance = 5f;

    [Header("Distance Settings")]
    public float visibleDistance = 50f;

    private Renderer eggRenderer;
    private Material eggMaterial;

    [Header("Egg Info")]
    public EggType eggType;
    public bool isCollectable = true;

    [HideInInspector] public bool isCollected;

    [Header("AR Mode")]
    public bool isARMode = false; // NEW: stop Mapbox movement in AR

    void Start()
    {
        // AR Camera = Player
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }

        // Map reference (only if really needed)
        if (map == null)
        {
            map = FindObjectOfType<AbstractMap>();
        }

        /*EggManager.OnEggCollected += (collectedEgg) =>
        {
            if (Vector2d.Distance(collectedEgg.LatLon, geoPosition) < 0.00001)
                Destroy(gameObject);
        };*/
        // Renderer & material
        eggRenderer = GetComponentInChildren<Renderer>();
        if (eggRenderer != null)
        {
            eggMaterial = eggRenderer.material;
            eggMaterial.DisableKeyword("_EMISSION");
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (isCollected)
        {
            gameObject.SetActive(false);
            return;
        }

        if (isARMode)
            return;

        if (player == null || map == null)
            return;

        Vector3 worldPos = map.GeoToWorldPosition(geoPosition, true);
        worldPos.y += 0.5f;
        transform.position = worldPos;
    }

   /* void Update()
    {
        if (isARMode) return; // Already AR egg logic

        if (EggManager.Instance != null)
        {
            foreach (var data in EggManager.Instance.eggsToSpawn)
            {
                if (data.isCollected && Vector2d.Distance(data.LatLon, geoPosition) < 0.00001)
                {
                    // Hide or destroy the map egg
                    if (gameObject.activeSelf)
                        gameObject.SetActive(false); // or Destroy(gameObject)
                    return;
                }
            }
        }

        // Normal Map position update
        Vector3 worldPos = map.GeoToWorldPosition(geoPosition, true);
        worldPos.y += 0.5f;
        transform.position = worldPos;
    }*/




    // Optional: CanSeeEgg logic simplified
    public bool CanSeeEgg()
    {
        SubscriptionTier tier = GameManagerMap.Instance.currentTier;
        switch (tier)
        {
            case SubscriptionTier.None:
                return eggType == EggType.Red || eggType == EggType.Green;
            case SubscriptionTier.Pro:
            case SubscriptionTier.Premium:
                return true;
            default:
                return false;
        }
    }
}



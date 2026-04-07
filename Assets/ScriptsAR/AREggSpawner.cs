using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using TMPro;
using Mapbox.Utils;
using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;

public class AREggSpawner : MonoBehaviour
{
    [Header("AR Setup")]
    public XROrigin arOrigin;
    public ARRaycastManager raycastManager;
    public TextMeshProUGUI statusText;

    [Header("Egg Prefabs")]
    public GameObject redEggPrefab;
    public GameObject greenEggPrefab;
    public GameObject purpleEggPrefab;
    public GameObject goldenEggPrefab;

    [Header("Egg Settings")]
    public float eggScale = 1f;// Bigger for 3D
    public float floorOffset = 0.08f; // Lift egg above floor
    
    private bool spawned = false;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    int index = 0;
    void Update()
    {
        if (spawned) return;
        if (EggManager.Instance == null || EggManager.Instance.eggsToSpawn.Count == 0) return;

        // 1?? Raycast at screen center to detect floor
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneEstimated | TrackableType.FeaturePoint))
        {
            statusText?.SetText("?? Move phone slowly to find floor...");
            return;
        }

        Vector3 floorPos = hits[0].pose.position;

        // 2?? Get GPS location with fallback
        var playerLoc = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation;
        Vector2d playerLatLon;

        /*if (!playerLoc.IsLocationUpdated)
        {
            // Use default test coordinates (allows editor testing or GPS not ready)
            playerLatLon = new Vector2d(23.752407, 90.352974);
            statusText?.SetText("?? GPS not ready ? using test coordinates");
            Debug.Log("?? GPS not ready ? using test coordinates");
        }
        else
        {
            playerLatLon = playerLoc.LatitudeLongitude;
            statusText?.SetText("?? GPS ready ? placing eggs");
        }*/
        if (!playerLoc.IsLocationUpdated)
        {
            // Use location from Map scene
            playerLatLon = PlayerLocationHolder.LastKnownLocation;
            statusText?.SetText("?? Using map location for AR");
            Debug.Log($"?? GPS not ready ? using map location: {playerLatLon.x}, {playerLatLon.y}");
        }
        else
        {
            playerLatLon = playerLoc.LatitudeLongitude;
            statusText?.SetText("?? GPS ready ? placing eggs");
        }


        // 3?? Spawn eggs in AR
        foreach (var data in EggManager.Instance.eggsToSpawn)
        {
            if (data.isCollected)
                continue; // ?? DO NOT SPAWN COLLECTED EGGS
            GameObject prefab = GetPrefabByType(data.eggType);
            if (prefab == null) continue;

            Vector2d eggLatLon = new Vector2d(data.latitude, data.longitude);

            // Convert GPS to meters relative to player
            Vector2d playerMeters = Conversions.LatLonToMeters(playerLatLon);
            Vector2d eggMeters = Conversions.LatLonToMeters(eggLatLon);
            Vector2d relativeMeters = eggMeters - playerMeters;

            Vector3 offset = new Vector3((float)relativeMeters.x, 0, (float)relativeMeters.y);

            // Limit distance for AR visibility
            if (offset.magnitude > 15f)
                offset = offset.normalized * 15f;

            float eggHeight = prefab.GetComponent<Renderer>().bounds.size.y;
            // Vector3 finalPos = floorPos + offset + Vector3.up * floorOffset;
            float spacingRadius = 0.4f; // 40 cm
            float angle = index * 60f * Mathf.Deg2Rad;

            Vector3 spreadOffset = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * spacingRadius;

            Vector3 finalPos =
                floorPos
                + spreadOffset
                + Vector3.up * floorOffset
                - Vector3.up * (eggHeight / 2f);

            // Spawn egg
            GameObject egg = Instantiate(prefab, finalPos, Quaternion.identity, arOrigin.transform);

            // Scale
            egg.transform.localScale = Vector3.one * eggScale;

            // Face camera (optional)
            Vector3 lookTarget = new Vector3(
                arOrigin.Camera.transform.position.x,
                egg.transform.position.y,
                arOrigin.Camera.transform.position.z
            );
            egg.transform.LookAt(lookTarget);
            egg.transform.Rotate(0, 180f, 0);

            // Assign behavior
            EggBehavior eb = egg.GetComponent<EggBehavior>();
            if (eb != null)
            {
                eb.isARMode = true;
                eb.geoPosition = eggLatLon;
            }
            index++; // ?? VERY IMPORTANT
        }

            


            spawned = true;
        statusText?.SetText("?? AR Eggs spawned on real floor!");
    }

    GameObject GetPrefabByType(EggType type)
    {
        return type switch
        {
            EggType.Red => redEggPrefab,
            EggType.Green => greenEggPrefab,
            EggType.Purple => purpleEggPrefab,
            EggType.Golden => goldenEggPrefab,
            _ => redEggPrefab
        };
    }
}


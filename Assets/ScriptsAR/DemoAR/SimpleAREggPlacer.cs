using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SimpleAREggPlacer : MonoBehaviour
{
    public GameObject eggPrefab;

    private ARRaycastManager raycastManager;
    private GameObject spawnedEgg;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Place ONLY once
        if (spawnedEgg != null) return;

        // Raycast from center of screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            spawnedEgg = Instantiate(eggPrefab, hitPose.position, hitPose.rotation);

            // Make sure egg sits on floor
            spawnedEgg.transform.rotation = Quaternion.identity;

            // Scale fix (important)
            spawnedEgg.transform.localScale = Vector3.one * 0.3f;

            Debug.Log("?? Egg spawned on FLOOR");
        }
    }
}

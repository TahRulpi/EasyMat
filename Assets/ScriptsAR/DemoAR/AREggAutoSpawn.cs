using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AREggAutoSpawn : MonoBehaviour
{
    [Header("Settings")]
    public GameObject eggPrefab;
    public float distanceFromCamera = 1.0f; // 1 meter away
    public Vector3 forcedScale = new Vector3(0.2f, 0.2f, 0.2f); // Forces egg to be 20cm

    private ARRaycastManager raycastManager;
    private Camera arCamera;
    private bool spawned = false;
    private GameObject spawnedInstance;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    IEnumerator Start()
    {
        Debug.Log("Waiting for AR Session to start tracking...");

        // 1. Wait until AR system is actually ready
        while (ARSession.state != ARSessionState.SessionTracking)
        {
            yield return null;
        }

        // 2. Wait 2 seconds for the camera to stabilize its position
        yield return new WaitForSeconds(2.0f);

        SpawnEgg();
    }

    void SpawnEgg()
    {
        if (spawned || eggPrefab == null) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager != null && raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        {
            // Spawn on detected floor
            Pose hitPose = hits[0].pose;
            spawnedInstance = Instantiate(eggPrefab, hitPose.position, hitPose.rotation);
            spawnedInstance.name = "[AUTO_EGG_PLANE]";
            Debug.Log("EGG SPAWNED ON PLANE");
        }
        else
        {
            // Fallback: Spawn in air in front of camera
            Vector3 spawnPos = arCamera.transform.position + (arCamera.transform.forward * distanceFromCamera);
            spawnedInstance = Instantiate(eggPrefab, spawnPos, Quaternion.identity);
            spawnedInstance.name = "[AUTO_EGG_FALLBACK]";
            Debug.Log("EGG SPAWNED IN FRONT OF CAMERA");
        }

        // FORCE SCALE so you can see it
        // spawnedInstance.transform.localScale = forcedScale;
        // This keeps your 1000 scale and multiplies it by the forced scale
        spawnedInstance.transform.localScale = Vector3.Scale(spawnedInstance.transform.localScale, forcedScale);
        spawned = true;
    }

    void Update()
    {
        // Debugging: Keep track of where the egg is
        if (spawned && spawnedInstance != null)
        {
            float dist = Vector3.Distance(arCamera.transform.position, spawnedInstance.transform.position);
            // This will spam your log so you can find it like a metal detector
            if (Time.frameCount % 30 == 0) // Log every 30 frames so it's readable
                Debug.Log($"Egg is {dist:F2}m away at {spawnedInstance.transform.position}");
        }
    }
}
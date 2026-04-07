using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AREggOnPlane : MonoBehaviour
{
    public GameObject eggPrefab;
    public Vector3 forcedScale = new Vector3(0.2f, 0.2f, 0.2f);

    private ARRaycastManager raycastManager;
    private Camera arCamera;
    private List<GameObject> spawnedEggs = new List<GameObject>();

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    void Update()
    {
        // Check for touch
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Raycast from the touch position
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            GameObject newEgg = Instantiate(eggPrefab, hitPose.position, hitPose.rotation);

            // Set scale and name
            newEgg.transform.localScale = forcedScale;
            newEgg.name = "Tapped_Egg_" + spawnedEggs.Count;

            spawnedEggs.Add(newEgg);
            Debug.Log("Egg spawned at touch location: " + hitPose.position);
        }

        // Distance Check for the most recent egg
        if (spawnedEggs.Count > 0)
        {
            GameObject lastEgg = spawnedEggs[spawnedEggs.Count - 1];
            float dist = Vector3.Distance(arCamera.transform.position, lastEgg.transform.position);
            Debug.Log("Distance to latest tapped egg: " + dist + " meters");
        }
    }
}
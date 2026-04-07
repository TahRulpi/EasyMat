

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class AREggCollector : MonoBehaviour
{
    private Camera arCamera;
    private static int totalCollectedThisSession = 0;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        // FIX: Only allow collection if exactly ONE finger is touching the screen.
        // This prevents the script from firing while the user is pinch-zooming the map.
        if (Input.touchCount > 1) return;
        if (AppModeManager.Instance.currentMode != AppMode.AR)
            return;
        // New Input System check for mobile touch
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Pointer.current.position.ReadValue();

            // Logcat will still show this so you can verify it's working
            Debug.Log("?? Touch Detected at: " + touchPosition);
            PerformRaycast(touchPosition);
        }
    }



    /*void PerformRaycast(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("Egg"))
            {
                Debug.Log("?? Hit: " + hit.collider.name);
                EggBehavior eggScript = hit.collider.GetComponent<EggBehavior>();

                if (eggScript != null)
                {
                    CheckCollectionEligibility(eggScript);
                }
            }
        }
    }*/
    void PerformRaycast(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // ?? IMPORTANT: get EggBehavior from parent OR self
            EggBehavior egg = hit.collider.GetComponentInParent<EggBehavior>();

            if (egg == null)
            {
                Debug.Log("? Hit object but not an egg");
                return;
            }

            Debug.Log("?? Egg touched: " + egg.eggType);
            CheckCollectionEligibility(egg);
        }
    }


    // --- LOGIC UNCHANGED ---
    void CheckCollectionEligibility(EggBehavior egg)
    {
        if (egg == null || egg.isCollected)
        {
            Debug.Log("?? Egg already collected or null");
            return;
        }


        SubscriptionTier tier = GameManagerMap.Instance.currentTier;

        switch (tier)
        {
            case SubscriptionTier.None: // Free
                if ((egg.eggType == EggType.Red || egg.eggType == EggType.Green) && totalCollectedThisSession < 1)
                {
                    CollectEgg(egg);
                }
                else if (totalCollectedThisSession >= 1)
                {
                    Debug.Log("None Tier: Limit reached (1 egg max).");
                }
                else
                {
                    Debug.Log("None Tier: Cannot collect this egg type.");
                }
                break;

            case SubscriptionTier.Pro:
                if (totalCollectedThisSession < 1)
                {
                    CollectEgg(egg);
                }
                else
                {
                    Debug.Log("Pro Tier: Limit reached (1 egg max).");
                }
                break;

            case SubscriptionTier.Premium:
                CollectEgg(egg);
                break;
        }
    }

    

    void CollectEgg(EggBehavior egg)
    {
        if (egg == null || egg.isCollected)
            return;

        egg.isCollected = true;
        totalCollectedThisSession++;

        // ?? IMMEDIATELY hide EVERYTHING visible
        foreach (var r in egg.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;

        foreach (var c in egg.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        // Mark collected in data
        if (EggManager.Instance != null)
        {
            foreach (var data in EggManager.Instance.eggsToSpawn)
            {
                if (Mathf.Abs((float)data.latitude - (float)egg.geoPosition.x) < 0.00001f &&
                    Mathf.Abs((float)data.longitude - (float)egg.geoPosition.y) < 0.00001f)
                {
                    data.isCollected = true;
                    break;
                }
            }
        }

        // ?? Destroy AFTER visuals are gone
        Destroy(egg.gameObject, 0.1f);

        Debug.Log("? Egg visually removed + destroyed");
    }








    public static void ResetCollectionCount()
    {
        totalCollectedThisSession = 0;
    }
}



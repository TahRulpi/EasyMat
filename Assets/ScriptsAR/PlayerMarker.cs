using System.Collections;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;

public class PlayerMarker : MonoBehaviour
{
    [Header("Mapbox Settings")]
    public AbstractMap map;

    [Header("Player UI Marker")]
    public RectTransform playerMarker; // UI Image inside Canvas
    public Canvas canvas;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public int minZoom = 14;
    public int maxZoom = 18;

    [Header("Fallback GPS Settings")]
    public bool useSimulatedGPS = true; // fallback if GPS fails
    public Vector2d simulatedLocation = new Vector2d(23.8103, 90.4125); // Dhaka

    private Vector2d currentGPS;
    private bool gpsReady = false;

    void Start()
    {
        Debug.Log("Starting PlayerMarker...");

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("Requesting location permission...");
            Permission.RequestUserPermission(Permission.FineLocation);
        }
#endif
        StartCoroutine(StartLocationService());

        if (map != null)
        {
            map.OnInitialized += () => Debug.Log("Map initialized");
            map.OnUpdated += () => Debug.Log("Map updated with tiles");
        }
    }

    IEnumerator StartLocationService()
    {
        Debug.Log($"Location enabled by user: {Input.location.isEnabledByUser}");
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location service not enabled by user");
            if (useSimulatedGPS)
            {
                Debug.Log("Using simulated GPS as fallback");
                currentGPS = simulatedLocation;
                map.UpdateMap(currentGPS, map.Zoom);
                gpsReady = true;
            }
            yield break;
        }

        Input.location.Start();
        Debug.Log("Starting location service...");

        // Wait until GPS is ready
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            Debug.Log("Waiting for valid GPS signal...");
            yield return new WaitForSeconds(1f);
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning($"GPS failed to start. Status: {Input.location.status}");
            if (useSimulatedGPS)
            {
                Debug.Log("Using simulated GPS as fallback");
                currentGPS = simulatedLocation;
                map.UpdateMap(currentGPS, map.Zoom);
                gpsReady = true;
            }
            yield break;
        }

        currentGPS = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);
        gpsReady = true;
        map.UpdateMap(currentGPS, map.Zoom);
        Debug.Log($"GPS ready: Lat={currentGPS.x}, Lon={currentGPS.y}");
    }

    void Update()
    {
        if (!gpsReady || map == null || playerMarker == null || canvas == null)
            return;

        // Update GPS from device
        if (Input.location.status == LocationServiceStatus.Running)
        {
            currentGPS = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);
        }

        // Update Map
        map.UpdateMap(currentGPS, map.Zoom);

        // Move UI player marker over map
        Vector3 worldPos = map.GeoToWorldPosition(currentGPS, true);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out canvasPos
        );

        // Smooth movement
        playerMarker.anchoredPosition = Vector2.Lerp(playerMarker.anchoredPosition, canvasPos, Time.deltaTime * 5f);

        // Optional debug
        Debug.Log(
            $"GPS STATUS: {Input.location.status} | " +
            $"Lat: {currentGPS.x}, Lon: {currentGPS.y}"
        );

        // Pinch zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevDist = (t0Prev - t1Prev).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = currDist - prevDist;

            float newZoom = Mathf.Clamp(map.Zoom + delta * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            map.UpdateMap(currentGPS, newZoom);
            Debug.Log($"Zoom updated: {newZoom}");
        }
    }
}

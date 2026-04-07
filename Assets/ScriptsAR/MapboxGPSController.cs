using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Mapbox.Unity.Utilities;

public class MapboxGPSControllerMobile : MonoBehaviour
{
    [Header("Mapbox Reference")]
    public AbstractMap map;

    [Header("UI References")]
    public RectTransform playerMarker;
    public RectTransform accuracyCircle;
    public RectTransform compassImage;
    public TMP_Text debugText;
    public TMP_Text gpsLabel;

    [Header("Zoom Settings")]
    public float minZoom = 14f;
    public float maxZoom = 20f;
    public float zoomSensitivity = 0.01f;

    [Header("GPS & Smoothing")]
    public float gpsSmoothSpeed = 5f;
    public float maxAllowedAccuracy = 30f; // meters

    [Header("Map Follow Mode")]
    public bool followPlayer = true;

    // Private variables
    private Vector2d gpsLatLon;
    private Vector2d smoothGps;
    private Vector2d mapCenter;
    private float currentZoom;
    private float currentHeading;
    private Vector2 dragVelocity;
    private Vector2 lastTouchPos;
    private bool isDragging = false;
    private bool gpsReady = false;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Start()
    {
        currentZoom = (float)map.Zoom;
        mapCenter = new Vector2d(40.7128, -74.0060); // default NYC
        map.Initialize(mapCenter, (int)currentZoom);

        StartCoroutine(GPSRoutine());
    }

    void Update()
    {
        HandleTouch();

        if (!isDragging && !followPlayer && dragVelocity.magnitude > 0.01f)
            ApplyInertia();

        if (!gpsReady) return;

        UpdateGPS();
        UpdateVisuals();
        UpdateCompass();
        UpdateDebug();
    }

    // ================= GPS INITIALIZATION =================
    IEnumerator GPSRoutine()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            if (debugText) debugText.text = "GPS Error: Location Disabled";
            yield break;
        }

        Input.location.Start(1f, 1f); // 1m accuracy, 1m distance
        Input.compass.enabled = true;

        int compassWait = 2;
        while (Input.compass.headingAccuracy <= 0 && compassWait > 0)
        {
            if (debugText) debugText.text = $"Calibrating Compass... {compassWait}";
            yield return new WaitForSeconds(1);
            compassWait--;
        }

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            if (debugText) debugText.text = $"Searching GPS... {maxWait}";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed || maxWait <= 0)
        {
            if (debugText) debugText.text = "GPS Failed: No Signal";
            yield break;
        }

        // GPS ready
        gpsLatLon = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);
        smoothGps = gpsLatLon;
        mapCenter = gpsLatLon;
        map.UpdateMap(mapCenter, currentZoom);
        gpsReady = true;
    }

    // ================= UPDATE GPS =================
    void UpdateGPS()
    {
        if (Input.location.status != LocationServiceStatus.Running) return;

        Vector2d latestRaw = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);
        float horizontalAccuracy = Input.location.lastData.horizontalAccuracy;

        if (horizontalAccuracy > maxAllowedAccuracy) return;

        // Distance in meters
        // double distance = Conversions.LatLonToMeters(gpsLatLon.x, gpsLatLon.y, latestRaw.x, latestRaw.y);
        double distance = Vector2d.Distance(gpsLatLon, latestRaw) * 111000.0;


        if (distance < 1.0f) return; // ignore GPS noise

        gpsLatLon = latestRaw;

        // Smooth player movement
        float smoothFactor = Mathf.Clamp01(gpsSmoothSpeed * Time.deltaTime * 2f);
        smoothGps = Vector2d.Lerp(smoothGps, gpsLatLon, smoothFactor);

        // Update map center if following player
        if (followPlayer)
        {
            mapCenter = smoothGps;
            map.UpdateMap(mapCenter, currentZoom);
        }

        UpdateGPSLabel();
        Debug.Log($"[GPS] Lat:{smoothGps.x:F6}, Lon:{smoothGps.y:F6}, Acc:{horizontalAccuracy:F1}m, Moved:{distance:F2}m");
    }

    // ================= TOUCH INPUT =================
    void HandleTouch()
    {
        var touches = Touch.activeTouches;

        if (touches.Count == 1)
            HandleDrag(touches[0]);
        else if (touches.Count == 2)
            HandleZoom(touches);
        else
            isDragging = false;
    }

    void HandleDrag(Touch t)
    {
        if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            isDragging = true;
            followPlayer = false;
            dragVelocity = Vector2.zero;
            lastTouchPos = t.screenPosition;
        }

        if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 delta = t.screenPosition - lastTouchPos;
            lastTouchPos = t.screenPosition;

            float zoomFactor = Mathf.Pow(2, currentZoom);
            double latStep = (delta.y * 0.05f) / zoomFactor;
            double lonStep = (delta.x * 0.05f) / zoomFactor;

            mapCenter.x -= latStep;
            mapCenter.y -= lonStep;

            dragVelocity = delta;
            map.UpdateMap(mapCenter, currentZoom);
        }

        if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            isDragging = false;
    }

    void ApplyInertia()
    {
        dragVelocity *= 0.85f;
        float zoomFactor = Mathf.Pow(2, currentZoom);
        mapCenter.x -= (dragVelocity.y * 0.05f) / zoomFactor;
        mapCenter.y -= (dragVelocity.x * 0.05f) / zoomFactor;
        map.UpdateMap(mapCenter, currentZoom);
    }

    void HandleZoom(ReadOnlyArray<Touch> touches)
    {
        followPlayer = false;
        float currDist = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
        float prevDist = Vector2.Distance(touches[0].screenPosition - touches[0].delta, touches[1].screenPosition - touches[1].delta);
        float delta = currDist - prevDist;
        currentZoom = Mathf.Clamp(currentZoom + (delta * zoomSensitivity), minZoom, maxZoom);
        map.UpdateMap(mapCenter, currentZoom);
    }

    // ================= VISUALS =================
    void UpdateVisuals()
    {
        if (playerMarker == null || map == null || Camera.main == null) return;

        Vector3 worldPos = map.GeoToWorldPosition(smoothGps, false);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        Canvas canvas = playerMarker.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                playerMarker.position = screenPos;
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                  canvas.transform as RectTransform,
                  screenPos,
                  canvas.worldCamera,
                  out Vector2 localPoint
                );
                playerMarker.localPosition = localPoint;
            }
            else if (canvas.renderMode == RenderMode.WorldSpace)
                playerMarker.position = worldPos;
        }

        // Accuracy circle
        if (accuracyCircle != null)
        {
            accuracyCircle.position = playerMarker.position;

            float metersPerPixel = (float)(40075016.686 * Mathf.Cos((float)smoothGps.x * Mathf.Deg2Rad) / Mathf.Pow(2, currentZoom + 8));
            float pixelSize = Input.location.lastData.horizontalAccuracy / metersPerPixel;

            accuracyCircle.sizeDelta = Vector2.Lerp(
              accuracyCircle.sizeDelta,
              new Vector2(pixelSize * 2, pixelSize * 2),
              Time.deltaTime * 5f
            );

            var img = accuracyCircle.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = Input.location.lastData.horizontalAccuracy > 20 ?
                      new Color(1, 0, 0, 0.2f) : new Color(0, 0.5f, 1, 0.2f);
        }

        UpdateGPSLabel();
    }

    void UpdateCompass()
    {
        if (!gpsReady) return;

        float rawHeading = Input.compass.trueHeading;
        if (rawHeading == 0 && Input.compass.magneticHeading != 0)
            rawHeading = Input.compass.magneticHeading;

        currentHeading = Mathf.LerpAngle(currentHeading, rawHeading, Time.deltaTime * 5f);

        if (playerMarker != null)
            playerMarker.localRotation = Quaternion.Euler(0, 0, -currentHeading);

        if (compassImage != null)
            compassImage.localRotation = Quaternion.Euler(0, 0, currentHeading);
    }

    void UpdateDebug()
    {
        if (debugText == null) return;

        /*debugText.text = $"<b>STATUS:</b> {Input.location.status}\n" +
                $"<b>MODE:</b> {(followPlayer ? "FOLLOW" : "FREE")}\n" +
                $"<b>ZOOM:</b> {currentZoom:F1}\n" +
                $"<b>GPS:</b> {smoothGps.x:F6}, {smoothGps.y:F6}\n" +
                $"<b>ACC:</b> {Input.location.lastData.horizontalAccuracy:F1}m";*/

        debugText.text = 
                $"<b>GPS:</b> {smoothGps.x:F6}, {smoothGps.y:F6}\n" +
                $"<b>ACC:</b> {Input.location.lastData.horizontalAccuracy:F1}m";
    }

    void UpdateGPSLabel()
    {
        if (gpsLabel == null || playerMarker == null) return;

        gpsLabel.text = $"Lat: {smoothGps.x:F6}\nLon: {smoothGps.y:F6}\nAcc: {Input.location.lastData.horizontalAccuracy:F1}m";

        RectTransform labelRect = gpsLabel.GetComponent<RectTransform>();
        if (labelRect != null)
            labelRect.position = playerMarker.position + new Vector3(0, 50f, 0);
    }

    public void ToggleFollow()
    {
        followPlayer = true;
        dragVelocity = Vector2.zero;
        if (gpsReady)
            map.UpdateMap(smoothGps, currentZoom);
    }
}

using UnityEngine;
using Mapbox.Unity.Map;

public class MapboxTouchZoom : MonoBehaviour
{
    public AbstractMap map;
    public float zoomSpeed = 0.1f;
    public int minZoom = 14;
    public int maxZoom = 20;

    private float lastTouchDistance;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                lastTouchDistance = currentDistance;
                return;
            }

            float delta = currentDistance - lastTouchDistance;

            if (Mathf.Abs(delta) > 2f)
            {
                float newZoom = map.Zoom + delta * zoomSpeed * Time.deltaTime;
                map.UpdateMap(map.CenterLatitudeLongitude, Mathf.Clamp((int)newZoom, minZoom, maxZoom));
            }

            lastTouchDistance = currentDistance;
        }
    }
}

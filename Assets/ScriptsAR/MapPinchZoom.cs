/*using UnityEngine;
using Mapbox.Unity.Map;

public class MapPinchZoom : MonoBehaviour
{
    public AbstractMap map;
    public float zoomSpeed = 0.02f;
    public float minZoom = 14f;
    public float maxZoom = 19f;

    float lastDistance;

    void Update()
    {
        if (Input.touchCount != 2) return;

        Touch t1 = Input.GetTouch(0);
        Touch t2 = Input.GetTouch(1);

        float currentDistance = Vector2.Distance(t1.position, t2.position);

        if (lastDistance == 0)
        {
            lastDistance = currentDistance;
            return;
        }

        float delta = currentDistance - lastDistance;

        float newZoom = map.Zoom + delta * zoomSpeed;
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

        if (Mathf.Abs(newZoom - map.Zoom) > 0.01f)
        {
            map.UpdateMap(map.CenterLatitudeLongitude, Mathf.RoundToInt(newZoom));
        }

        lastDistance = currentDistance;

        if (t1.phase == TouchPhase.Ended || t2.phase == TouchPhase.Ended)
            lastDistance = 0;
    }
}
*/
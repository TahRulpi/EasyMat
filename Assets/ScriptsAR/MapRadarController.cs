using UnityEngine;
using Mapbox.Utils;

public class MapRadarController : MonoBehaviour
{
    public RectTransform radarRoot;
    public float radarRadius = 300f;      // UI pixels
    public float metersToPixels = 6f;

    void Update()
    {
        Vector2 playerGPS = new Vector2(
            (float)LocationManager.Instance.latitude,
            (float)LocationManager.Instance.longitude
        );

        foreach (EggMarkerUI egg in FindObjectsOfType<EggMarkerUI>())
        {
            Vector2 eggGPS = new Vector2(
                (float)egg.gpsPosition.x,
                (float)egg.gpsPosition.y
            );

            Vector2 offset = (eggGPS - playerGPS) * metersToPixels;

            bool insideRadar = offset.magnitude <= radarRadius;
            egg.gameObject.SetActive(insideRadar);

            if (insideRadar)
            {
                egg.transform.localPosition = offset;
                egg.isScannable = offset.magnitude <= 80f; // ~20m
            }
        }
    }
}

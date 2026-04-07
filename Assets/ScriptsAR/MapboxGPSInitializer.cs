using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;

public class MapboxGPSInitializer : MonoBehaviour
{
    public AbstractMap map;

    IEnumerator Start()
    {
        // Safety check
        if (map == null)
        {
            Debug.LogError("? AbstractMap not assigned");
            yield break;
        }

        // Start GPS
        Input.location.Stop();
        yield return new WaitForSeconds(1f);
        Input.location.Start(1f, 1f);

        int maxWait = 30;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("? Waiting for valid GPS signal...");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError("? GPS FAILED");
            yield break;
        }

        // ? THIS IS WHERE YOU ADD IT ?
        Vector2d gpsPos = new Vector2d(
            Input.location.lastData.latitude,
            Input.location.lastData.longitude
        );

        Debug.Log("? MAP INIT AT: " + gpsPos);

        map.Initialize(gpsPos, Mathf.RoundToInt(map.Zoom));

    }
}

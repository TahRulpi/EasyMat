using UnityEngine;

public class GPSDebug : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(StartGPS());
    }

    System.Collections.IEnumerator StartGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("GPS timeout");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            Debug.Log("MY Latitude: " + Input.location.lastData.latitude);
            Debug.Log("MY Longitude: " + Input.location.lastData.longitude);
        }

       // Input.location.Stop();
        if (Input.location.status == LocationServiceStatus.Running)
        {
            var data = Input.location.lastData;
           // Debug.Log("MY Latitude: " + myLatitude.ToString("F6"));
        }

    }

}

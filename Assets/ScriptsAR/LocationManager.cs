using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance;

    public float latitude;
    public float longitude;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        if (!Input.location.isEnabledByUser)
            yield break;

        Input.location.Start(1f, 1f);  // accuracy, update distance

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
            yield break;

        InvokeRepeating(nameof(UpdateLocation), 0f, 1f);
    }

    void UpdateLocation()
    {
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
    }
}

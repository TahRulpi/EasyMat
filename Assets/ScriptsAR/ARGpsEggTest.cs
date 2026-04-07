using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ARGpsEggTest : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public GameObject eggPrefab;
    public TextMeshProUGUI debugText;

    void Start()
    {
        StartCoroutine(StartGPS());
    }

    IEnumerator StartGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            debugText.text = "? GPS not enabled";
            yield break;
        }

        Input.location.Start(1f, 0.1f);

        int wait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && wait > 0)
        {
            yield return new WaitForSeconds(1);
            wait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            debugText.text = "? GPS failed";
            yield break;
        }

        var data = Input.location.lastData;
        debugText.text =
            $"?? LAT: {data.latitude}\n?? LON: {data.longitude}";

        Debug.Log($"Latitude: {data.latitude}");
        Debug.Log($"Longitude: {data.longitude}");

        SpawnEgg();
    }

    void SpawnEgg()
    {
        Transform cam = Camera.main.transform;

        Vector3 spawnPos = cam.position + cam.forward * 2f;
        spawnPos.y -= 0.5f;

        Instantiate(eggPrefab, spawnPos, Quaternion.identity);

        debugText.text += "\n?? Egg spawned!";
    }
}

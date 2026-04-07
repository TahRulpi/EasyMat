using UnityEngine;
using UnityEngine.SceneManagement;

public class ARScanButton : MonoBehaviour
{
    public EggSpawner mapEggSpawner;

    public void OnGoARButtonPressed()
    {
        AppModeManager.Instance.currentMode = AppMode.AR;
        Debug.Log("AR Button Pressed!");

        if (EggManager.Instance == null)
        {
            Debug.LogError("EggManager.Instance is NULL!");
            return;
        }

        // Set player GPS
        EggManager.Instance.playerLatitude = Input.location.lastData.latitude;
        EggManager.Instance.playerLongitude = Input.location.lastData.longitude;

        // Only populate eggs if the list is empty
        if (EggManager.Instance.eggsToSpawn.Count == 0 && mapEggSpawner != null)
        {
            foreach (var egg in mapEggSpawner.spawnedEggs)
            {
                if (egg == null) continue;

                EggData data = new EggData
                {
                    eggType = egg.eggType,
                    latitude = egg.geoPosition.x,
                    longitude = egg.geoPosition.y
                };

                EggManager.Instance.eggsToSpawn.Add(data);
            }
        }

        // If still empty, add default red egg at player location
        if (EggManager.Instance.eggsToSpawn.Count == 0)
        {
            EggData defaultEgg = new EggData
            {
                eggType = EggType.Red,
                latitude = EggManager.Instance.playerLatitude,
                longitude = EggManager.Instance.playerLongitude
            };
            EggManager.Instance.eggsToSpawn.Add(defaultEgg);
            Debug.Log("No eggs found, added default red egg at player location.");
        }

        Debug.Log("Eggs sent to AR: " + EggManager.Instance.eggsToSpawn.Count);

        // Load AR scene
        SceneManager.LoadScene("ARScene");
    }

}

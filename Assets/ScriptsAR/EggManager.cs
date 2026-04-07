

using System.Collections.Generic;
using UnityEngine;

public class EggManager : MonoBehaviour
{
    public static EggManager Instance;

    public List<EggData> eggsToSpawn = new List<EggData>();
    public double playerLatitude;
    public double playerLongitude;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("EggManager initialized and persistent.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


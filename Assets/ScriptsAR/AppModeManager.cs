using UnityEngine;

public enum AppMode
{
    Map,
    AR
}

public class AppModeManager : MonoBehaviour
{
    public static AppModeManager Instance;
    public AppMode currentMode = AppMode.Map;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}

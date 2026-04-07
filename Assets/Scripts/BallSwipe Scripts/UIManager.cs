using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public GameObject exitPanel;

    private bool isPaused = false;

    // ---------------------------
    // SETTINGS PANEL
    // ---------------------------
    public void ToggleSettingsPanel()
    {
        // If exit panel is open, close it first
        if (exitPanel.activeSelf)
            exitPanel.SetActive(false);

        isPaused = !isPaused;
        settingsPanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void CloseSettingsPanel()
    {
        isPaused = false;
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ---------------------------
    // EXIT PANEL
    // ---------------------------
    public void OpenExitPanel()
    {
        // If settings panel is open, close it
        if (settingsPanel.activeSelf)
            settingsPanel.SetActive(false);

        exitPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseExitPanel()
    {
        exitPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}

using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;      // The very first screen (Play, Settings, etc.)
    public GameObject allGamesPanel;  // The screen with the chicken game icon

    void Start()
    {
        // Check if we came from the GlobalNav "Back" button
        if (GlobalNav.OpenAllGamesOnLoad)
        {
            ShowAllGames();

            // CRITICAL: Reset the flag so it doesn't stay true forever!
            GlobalNav.OpenAllGamesOnLoad = false;
        }
        else
        {
            ShowMain();
        }
    }

    public void ShowAllGames()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (allGamesPanel != null) allGamesPanel.SetActive(true);
    }

    public void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (allGamesPanel != null) allGamesPanel.SetActive(false);
    }
}
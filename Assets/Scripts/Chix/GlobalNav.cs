using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalNav : MonoBehaviour
{
    public static bool OpenAllGamesOnLoad = false;

    public void BackToAllGames()
    {
        // 1. Reset Time Scale! If you don't, the Main Menu will be frozen.
        Time.timeScale = 1f;

        // 2. Set the flag for the MainMenuManager to read
        GlobalNav.OpenAllGamesOnLoad = true;

        // 3. Load the scene
        SceneManager.LoadScene("MainMenu");
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // This 'static' variable stays alive between scene loads
    public static string PanelToOpen = "";

    public void LoadSceneAndOpenPanel(string sceneName, string panelName)
    {
        PanelToOpen = panelName;
        SceneManager.LoadScene(sceneName);
    }
}
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    public void ShowGameOver()
    {
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }
}

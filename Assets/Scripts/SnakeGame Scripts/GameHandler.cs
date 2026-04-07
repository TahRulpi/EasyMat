using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    private static int score;

    [SerializeField] private Snake snake;
    [SerializeField] private Text metalAppleWarningText;
    public Canvas mainCanvas;

    public LevelGrid levelGrid { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public Canvas MainCanvas => mainCanvas;

    void Start()
    {
        levelGrid = new LevelGrid(16, 30);

        snake.Setup(levelGrid);
        levelGrid.Setup(snake);
    }

    private void Update()
    {
        if (levelGrid != null)
            levelGrid.Update();
    }


    public static int GetScore()
    {
        return score;
    }

    public static void AddScore(int amount)
    {
        score += amount;
    }

    public static void SnakeDied()
    {
        //GameOverWindow.ShowStatic();
    }

    public static void ShowMetalWarning(string msg)
    {
        instance.metalAppleWarningText.text = msg;
        instance.metalAppleWarningText.gameObject.SetActive(true);
    }

    public static void HideMetalWarning()
    {
        instance.metalAppleWarningText.gameObject.SetActive(false);
    }
}

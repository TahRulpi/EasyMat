using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIManager_Game : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject exitConfirmPanel;

    [Header("Buttons")]
    public Button settingsButton;
    public Button continueButton;
    public Button crossButton;
    public Button exitButton;
    public Button exitYesButton;
    public Button exitNoButton;

    private Stack<GameObject> panelStack = new Stack<GameObject>();
    private bool isPaused = false;

    private void Start()
    {
        settingsButton.onClick.AddListener(OpenSettings);
        continueButton.onClick.AddListener(ResumeGame);
        crossButton.onClick.AddListener(ResumeGame);
        exitButton.onClick.AddListener(OpenExitConfirm);
        exitYesButton.onClick.AddListener(ExitGameToMenu);
        exitNoButton.onClick.AddListener(GoBack);

        // Make sure panels start hidden
        settingsPanel.SetActive(false);
        exitConfirmPanel.SetActive(false);
    }

    // ------------------------------------------------------------
    // PANEL MANAGEMENT
    // ------------------------------------------------------------
    private void ShowPanel(GameObject panel)
    {
        // Disable previous panel
        if (panelStack.Count > 0)
            panelStack.Peek().SetActive(false);

        panel.SetActive(true);
        AnimatePopup(panel);

        panelStack.Push(panel);
    }

    private void ClosePanel(GameObject panel)
    {
        if (panelStack.Count == 0) return;

        GameObject closing = panelStack.Pop();
        AnimateClose(closing);

        StartCoroutine(HideAfterClose(closing));
    }

    private IEnumerator HideAfterClose(GameObject panel)
    {
        yield return new WaitForSecondsRealtime(0.25f);
        panel.SetActive(false);

        if (panelStack.Count > 0)
        {
            GameObject previous = panelStack.Peek();
            previous.SetActive(true);
            AnimatePopup(previous);
        }
    }

    private void GoBack()
    {
        ClosePanel(panelStack.Peek());
    }

    // ------------------------------------------------------------
    // ANIMATIONS
    // ------------------------------------------------------------
    private void AnimatePopup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        panel.transform.DOKill();
        cg.DOKill();

        panel.transform.localScale = Vector3.one * 0.4f;
        cg.alpha = 0f;

        panel.transform
            .DOScale(1f, 0.35f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        cg.DOFade(1f, 0.25f)
            .SetUpdate(true);
    }

    private void AnimateClose(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        panel.transform.DOKill();
        cg.DOKill();

        panel.transform
            .DOScale(0.6f, 0.22f)
            .SetEase(Ease.InBack)
            .SetUpdate(true);

        cg.DOFade(0f, 0.22f)
            .SetUpdate(true);
    }

    // ------------------------------------------------------------
    // SETTINGS
    // ------------------------------------------------------------
    private void OpenSettings()
    {
        if (isPaused) return;

        Time.timeScale = 0f;
        isPaused = true;

        ShowPanel(settingsPanel);
    }

    private void ResumeGame()
    {
        StartCoroutine(ResumeAfterDelay());
    }

    private IEnumerator ResumeAfterDelay()
    {
        AnimateClose(settingsPanel);
        yield return new WaitForSecondsRealtime(0.25f);

        ClosePanel(settingsPanel);

        yield return new WaitForSecondsRealtime(0.1f);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // ------------------------------------------------------------
    // EXIT CONFIRM
    // ------------------------------------------------------------
    private void OpenExitConfirm()
    {
        ShowPanel(exitConfirmPanel);
    }

    private void ExitGameToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

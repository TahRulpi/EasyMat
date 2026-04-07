using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandscapeScene : MonoBehaviour
{
    [Header("Add a black UI Image covering full screen")]
    public Image fadeImage; // Assign in Inspector

    void Start()
    {
        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        // Start fully black
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1);

        Time.timeScale = 0f;

        // Switch orientation while screen is black (hidden)
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Wait for orientation to actually apply
        yield return new WaitForSecondsRealtime(0.3f);

        Time.timeScale = 1f;

        // Fade in nicely
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }
}
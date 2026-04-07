using UnityEngine;
using System.Collections;

public class SnakeHitFeedback : MonoBehaviour
{
    public SpriteRenderer headRenderer;

    public IEnumerator PlayDeathEffect()
    {
        // Flash white
        headRenderer.color = Color.white;

        // Small shake
        Vector3 originalPos = transform.localPosition;
        float shakeAmount = 0.1f;
        float shakeTime = 0.15f;
        float timer = 0;

        while (timer < shakeTime)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
            timer += Time.deltaTime;
            yield return null;
        }

        // Reset
        transform.localPosition = originalPos;
        headRenderer.color = Color.white;
    }
}

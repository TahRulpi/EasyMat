using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.2f;

    private Vector3 originalPos;
    private float shakeTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTime > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeStrength;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0;
            transform.localPosition = originalPos;
        }
    }

    public void Shake()
    {
        shakeTime = shakeDuration;
    }
}

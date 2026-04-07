using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFix : MonoBehaviour
{
    [Header("Reference Settings")]
    [Tooltip("Reference screen height in world units (this should match your base camera size * 2).")]
    public float referenceHeight = 10f; // For orthographic size 5 → height = 10 units.

    [Tooltip("Reference resolution you designed your game for (width x height).")]
    public Vector2 referenceResolution = new Vector2(1080f, 1920f);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

#if UNITY_EDITOR
    void Update()
    {
        // Update live in editor for preview
        AdjustCamera();
    }
#endif

    void AdjustCamera()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraFix works only with Orthographic projection.");
            return;
        }

        float targetAspect = referenceResolution.x / referenceResolution.y;
        float windowAspect = (float)Screen.width / Screen.height;

        // Calculate how much to adjust
        float scale = windowAspect / targetAspect;

        // Adjust orthographic size to maintain consistent height
        cam.orthographicSize = (referenceHeight / 2f) / Mathf.Max(scale, 1f);
    }
}

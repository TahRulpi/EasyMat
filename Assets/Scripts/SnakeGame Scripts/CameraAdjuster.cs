using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAdjuster : MonoBehaviour
{
    public int gridWidth = 15;
    public int gridHeight = 25;
    public float margin = 0.5f; // Extra space around edges

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    void AdjustCamera()
    {
        // Set the camera position to center of grid
        cam.transform.position = new Vector3(gridWidth / 2f, gridHeight / 2f, -10f);

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = (float)gridWidth / (float)gridHeight;

        if (screenRatio >= targetRatio)
        {
            cam.orthographicSize = (gridHeight / 2f) + margin;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            cam.orthographicSize = ((gridHeight / 2f) * differenceInSize) + margin;
        }
    }
}

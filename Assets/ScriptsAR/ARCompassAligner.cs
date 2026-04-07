using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARCompassAligner : MonoBehaviour
{
    public bool isAligned = false;

    IEnumerator Start()
    {
        // 1. Start the compass service
        Input.compass.enabled = true;
        Input.location.Start();

        // 2. Wait for the compass to initialize
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Give it a moment to get a stable reading
        yield return new WaitForSeconds(1.0f);

        AlignWithNorth();
    }

    public void AlignWithNorth()
    {
        // Get the magnetic heading (0 = North, 90 = East, etc.)
        float heading = Input.compass.trueHeading;

        // Rotate the AR Origin so the camera's forward matches True North
        // We rotate the Parent (Origin) so the Camera moves with it
        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentRotation.x, heading, currentRotation.z);

        isAligned = true;
        Debug.Log($"AR Aligned to True North: {heading} degrees");
    }
}
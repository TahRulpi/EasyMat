using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionRestarter : MonoBehaviour
{
    public ARSession arSession;

    public void ResetSession()
    {
        // This clears the 'kNotTracking' state and restarts the search
        arSession.Reset();
        Debug.Log("AR Session Reset Requested");
    }
}
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionDebug : MonoBehaviour
{
    void Update()
    {
        Debug.Log("AR Session State: " + ARSession.state);
        Debug.Log("Camera background enabled: " +
    GetComponent<ARCameraBackground>().enabled);

    }
}

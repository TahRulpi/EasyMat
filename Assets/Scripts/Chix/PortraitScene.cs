using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PortraitScene : MonoBehaviour
{
    void Awake()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        Screen.orientation = ScreenOrientation.Portrait;
    }
}
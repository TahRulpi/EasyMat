using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.SceneManagement;

public class GotoMapScene : MonoBehaviour
{
    public void GoToMap()
    {
        SceneManager.LoadScene("MapScene"); // use your exact map scene name
    }
}

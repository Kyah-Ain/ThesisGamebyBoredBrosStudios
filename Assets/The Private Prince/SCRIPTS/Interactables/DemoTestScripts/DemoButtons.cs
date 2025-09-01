using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoButtons : MonoBehaviour
{
    public void ResetTest()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitTest()
    {
        Application.Quit();
    }
}

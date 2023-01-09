using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public void RestartScene()
    {
        SceneManager.LoadScene(1);
    }

    public void MainMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Restart2PlayScene()
    {
        SceneManager.LoadScene(2);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneScript : MonoBehaviour
{
    public GameObject MainMenuCanvas;
    public GameObject HowtoPlayCanvas;

    // Start is called before the first frame update
    void Start()
    {
        HowtoPlayCanvas.SetActive(false);
        MainMenuCanvas.SetActive(true);
    }

    public void HowToPlayClick()
    {
        MainMenuCanvas.SetActive(false);
        HowtoPlayCanvas.SetActive(true);
    }

    public void backtoMainMenu()
    {
        HowtoPlayCanvas.SetActive(false);
        MainMenuCanvas.SetActive(true);
    }

    public void play1Click()
    {
        SceneManager.LoadScene(1);
    }

    public void play2Click()
    {
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

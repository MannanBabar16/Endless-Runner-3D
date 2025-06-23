using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay"); // Replace with your actual gameplay scene name
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

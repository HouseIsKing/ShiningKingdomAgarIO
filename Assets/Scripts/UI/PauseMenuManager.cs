using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject optionsObject = null;
    public void UnpauseGame()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
    public void ExitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenuTest");
    }
    public void OptionsMenuOpen()
    {
        optionsObject.SetActive(true);
        optionsObject.GetComponentInChildren<OptionsMenu>().menuRef = gameObject;
        gameObject.SetActive(false);
    }
}

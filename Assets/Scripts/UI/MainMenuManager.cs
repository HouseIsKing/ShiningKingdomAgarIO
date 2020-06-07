using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public UnityEngine.UI.Button startButton;
    public GameObject optionsMenu;
    public GameObject profileMenu;
    private void Start()
    {
        optionsMenu.SetActive(false);
        profileMenu.SetActive(false);
        if (SaveLoad.Load() == null)
        {
            SaveLoad.Save(new PlayerData());
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene("TestMap");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void Options()
    {
        optionsMenu.SetActive(true);
        optionsMenu.GetComponentInChildren<OptionsMenu>().menuRef = gameObject;
        gameObject.SetActive(false);
    }
    public void Profile()
    {
        profileMenu.SetActive(true);
        profileMenu.GetComponent<ProfileMenuManager>().menuRef = gameObject;
        gameObject.SetActive(false);
    }
}

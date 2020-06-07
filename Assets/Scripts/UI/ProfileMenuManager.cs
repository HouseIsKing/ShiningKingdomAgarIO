using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileMenuManager : MonoBehaviour
{
    public TMP_InputField profileName;
    public GameObject menuRef;
    public void OnEnable()
    {
        PlayerData playerData = SaveLoad.Load();
        profileName.text = playerData.playerName;
    }
    public void Cancel()
    {
        menuRef.SetActive(true);
        gameObject.SetActive(false);
    }
    public void Apply()
    {
        string profName = profileName.text;
        PlayerData playerData = SaveLoad.Load();
        playerData.playerName = profName;
        SaveLoad.Save(playerData);
        menuRef.SetActive(true);
        gameObject.SetActive(false);
    }
}

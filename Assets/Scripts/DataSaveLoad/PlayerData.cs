using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerData
{
    public string playerName;
    public List<bool> levelsStatus;
    public PlayerData()
    {
        playerName = "Player";
        levelsStatus = new List<bool>();
    }
    public PlayerData(string playerName)
    {
        this.playerName = playerName;
        levelsStatus = new List<bool>();
    }
}

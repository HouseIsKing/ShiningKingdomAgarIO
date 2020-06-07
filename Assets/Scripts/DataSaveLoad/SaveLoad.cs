using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static void Save(PlayerData saveData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile;
        if (File.Exists(Application.persistentDataPath + "/PlayerData.pd"))
        {
            saveFile = File.Open(Application.persistentDataPath + "/PlayerData.pd", FileMode.Open);
            binaryFormatter.Serialize(saveFile, saveData);
            saveFile.Close();
        }
        else
        {
            saveFile = File.Create(Application.persistentDataPath + "/PlayerData.pd");
            binaryFormatter.Serialize(saveFile, saveData);
            saveFile.Close();
        }
    }
    public static PlayerData Load()
    {
        PlayerData result;
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile;
        if (File.Exists(Application.persistentDataPath + "/PlayerData.pd"))
        {
            saveFile = File.Open(Application.persistentDataPath + "/PlayerData.pd", FileMode.Open);
            result = binaryFormatter.Deserialize(saveFile) as PlayerData;
            saveFile.Close();
            return result;
        }
        else
        {
            return null;
        }
    }
}

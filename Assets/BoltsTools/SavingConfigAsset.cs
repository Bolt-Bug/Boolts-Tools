using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace BoltsTools
{
    [Icon("Assets/BoltsTools/Sprites/SettingsLogo.png")]
    public class SavingConfigAsset : ScriptableObject
    {
        public List <string> fileName = new(){"save.json"};
        public bool usePersistentDataPath = true;
        public bool useEncryption;


        public string GetFullPath(string saveFile = "save.json")
        {
            string path = usePersistentDataPath ? Application.persistentDataPath : Application.dataPath;
            return Path.Combine(path, saveFile);
        }
    }
}
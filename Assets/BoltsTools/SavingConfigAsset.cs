using System.IO;
using UnityEngine;

namespace BoltsTools
{
    [Icon("Assets/BoltsTools/Sprites/SettingsLogo.png")]
    public class SavingConfigAsset : ScriptableObject
    {
        public string fileName = "save.json";
        public bool usePersistentDataPath = true;
        public bool useEncryption;


        public string GetFullPath()
        {
            string path = usePersistentDataPath ? Application.persistentDataPath : Application.dataPath;
            return Path.Combine(path, fileName);
        }
    }
}
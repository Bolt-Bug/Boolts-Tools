using System.IO;
using UnityEngine;

namespace BoltsTools
{
    public class SavingConfigAsset : ScriptableObject
    {
        public string fileName = "save.json";
        public bool usePersistentDataPath = true;
        public bool useEncryption;

        public string path;

        public string GetFullPath()
        { return Path.Combine(path, fileName); }
    }
}
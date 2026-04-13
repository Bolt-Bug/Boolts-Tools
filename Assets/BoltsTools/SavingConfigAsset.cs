using System;
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

        // [HideInInspector]
        public List<SaveFileDefaults> defaults = new();

        public string GetFullPath(int saveFile = 0)
        {
            string path = usePersistentDataPath ? Application.persistentDataPath : Application.dataPath;
            return Path.Combine(path, fileName[saveFile]);
        }
    }

    [Serializable]
    public class SaveFileDefaults
    {
        public List<SaveFloat> floats = new();
        public List<SaveInt> ints = new();
        public List<SaveVector3> Vector3s = new();
        public List<SaveVector2> Vector2s = new();
        public List<SaveString> strings = new();
        public List<SaveBool> bools = new();
    }
}
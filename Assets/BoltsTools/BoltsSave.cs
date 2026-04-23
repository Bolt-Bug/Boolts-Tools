using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoltsTools
{
    public class BoltsSave
    {
        public static SavingConfigAsset _settings;
        static bool _isLoading;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void InitializeInEditor()
        {
            Initialize();
        }
        
        [MenuItem("Tools/Bolts Tools/Reset Save &#s")]
        public static void ResetTheSave()
        {
            bool confirm = EditorUtility.DisplayDialog("Are You Sure?",
                "Are You Sure You Want To Reset Your Save?" + 
                "\nThis will Remove Your Current Save File", "Yes", "Cancel");

            if (confirm)
            {
                ResetSave();
                Debug.Log("Saved Reset");
            }
        }
#endif

        /// <summary>
        /// Save A Float
        /// </summary>
        /// <param name="name">Name Of The Float To Be Saved</param>
        /// <param name="value">The Value Of The Saved Float</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveFloatValue(string name, float value, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveFloat sf = new() { name = name, value = value };

            int index = -1;
            if (sd.floats != null)
                index = sd.floats.FindIndex(x => x.name == name);

            if (index > -1)
                sd.floats[index].value = value;
            else
                sd.floats.Add(sf);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A Int
        /// </summary>
        /// <param name="name">Name Of The Int To Be Saved</param>
        /// <param name="value">The Value Of The Saved Int</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveIntValue(string name, int value, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveInt si = new() { name = name, value = value };

            int index = -1;
            if (sd.ints != null)
                index = sd.ints.FindIndex(x => x.name == name);

            if (index > -1)
                sd.ints[index].value = value;
            else
                sd.ints.Add(si);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A Vector3
        /// </summary>
        /// <param name="name">Name Of The Vector3 To Be Saved</param>
        /// <param name="value">The Value Of The Saved Vector3</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveVector3Value(string name, Vector3 value, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveVector3 sv = new() { name = name, value = value };

            int index = -1;

            if (sd.Vector3s != null)
                index = sd.Vector3s.FindIndex(x => x.name == name);

            if (index > -1)
                sd.Vector3s[index].value = value;
            else
                sd.Vector3s.Add(sv);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A Vector2
        /// </summary>
        /// <param name="name">Name Of The Vector2 To Be Saved</param>
        /// <param name="value">The Value Of The Saved Vector2</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveVector2Value(string name, Vector2 value, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveVector2 sv = new() { name = name, value = value };

            int index = -1;

            if (sd.Vector2s != null)
                index = sd.Vector2s.FindIndex(x => x.name == name);

            if (index > -1)
                sd.Vector2s[index].value = value;
            else
                sd.Vector2s.Add(sv);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A String
        /// </summary>
        /// <param name="name">Name Of The String To Be Saved</param>
        /// <param name="value">The Value Of The Saved String</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveStringValue(string name, string value, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveString ss = new() { name = name, value = value };

            int index = -1;
            if (sd.strings != null)
                index = sd.strings.FindIndex(x => x.name == name);

            if (index > -1)
                sd.strings[index].value = value;
            else
                sd.strings.Add(ss);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A Bool
        /// </summary>
        /// <param name="name">Name Of The Bool To Be Saved</param>
        /// <param name="value">The Value Of The Saved Bool</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveBoolValue(string name, bool value, int saveFile = 0)
        {
            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveData sd = LoadOrCreate(saveFile);

            SaveBool sb = new() { name = name, value = value };

            int index = -1;
            if (sd.bools != null)
                index = sd.bools.FindIndex(x => x.name == name);

            if (index > -1)
                sd.bools[index].value = value;
            else
                sd.bools.Add(sb);

            SaveFile(sd, saveFile);
        }

        /// <summary>
        /// Save A Class
        /// </summary>
        /// <param name="name">Name Of The Class To Be Saved</param>
        /// <param name="classInstance">The Class</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static void SaveClassVariable<T>(string name, T classInstance, int saveFile = 0) where T : class
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Saving");

                return;
            }

            SaveClass sc = new SaveClass() { name = name, value = JsonUtility.ToJson(classInstance) };

            int index = -1;

            if (sd.classes != null)
                index = sd.classes.FindIndex(x => x.name == name);

            if (index > -1)
                sd.classes[index].value = JsonUtility.ToJson(classInstance);
            else
                sd.classes.Add(sc);

            SaveFile(sd, saveFile);
        }

        public static void SaveFile(SaveData sd, int saveFile = 0)
        {
            string fullPath = _settings.GetFullPath(saveFile);

            string newJson = JsonUtility.ToJson(sd, _settings.useEncryption);
            File.WriteAllText(fullPath, newJson);
        }

        /// <summary>
        /// Returns A Saved Float
        /// </summary>
        /// <param name="name">Name Of The Saved Float</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static float GetFloat(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return -1;
            }

            int index = sd.floats.FindIndex(x => x.name == name);

            if (index > -1)
                return sd.floats[index].value;

            Debug.LogError($"Could Not Find Float Named: {name}\nReturned -1");
            return -1;
        }

        /// <summary>
        /// Returns A Saved Int
        /// </summary>
        /// <param name="name">Name Of The Saved Int</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static int GetInt(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return -1;
            }

            int index = sd.ints.FindIndex(x => x.name == name);

            if (index > -1)
                return sd.ints[index].value;

            Debug.LogError($"Could Not Find Int Named: {name}\nReturned -1");
            return -1;
        }

        /// <summary>
        /// Returns A Saved Vector3
        /// </summary>
        /// <param name="name">Name Of The Saved Vector3</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static Vector3 GetVector3(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return Vector3.zero;
            }

            int index = sd.Vector3s.FindIndex(x => x.name == name);

            if (index > -1)
                return sd.Vector3s[index].value;

            Debug.LogError($"Could Not Find Vector3 Named: {name}\nReturned Vector3.zero");
            return Vector3.zero;
        }

        /// <summary>
        /// Returns A Saved Vector2
        /// </summary>
        /// <param name="name">Name Of The Saved Vector2</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static Vector2 GetVector2(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return Vector2.zero;
            }

            int index = sd.Vector2s.FindIndex(x => x.name == name);

            if (index > -1)
                return sd.Vector2s[index].value;

            Debug.LogError($"Could Not Find Vector2 Named: {name}\nReturned Vector2.zero");
            return Vector2.zero;
        }

        /// <summary>
        /// Returns A Saved String
        /// </summary>
        /// <param name="name">Name Of The Saved String</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static string GetString(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return String.Empty;
            }

            int index = sd.strings.FindIndex(x => x.name == name);

            if (index > -1)
                return sd.strings[index].value;

            Debug.LogError($"Could Not Find String Named: {name}\nReturned Empty String");
            return String.Empty;
        }

        /// <summary>
        /// Return A Saved Bool
        /// </summary>
        /// <param name="name">Name Of The Saved Bool</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static bool GetBool(string name, int saveFile = 0)
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return false;
            }

            int index = sd.bools.FindIndex(x => x.name == name);
            if (index > -1)
                return sd.bools[index].value;

            Debug.LogError($"Could Not Find Bool Named: {name}\nReturned False");
            return false;
        }

        /// <summary>
        /// Return A Saved Class
        /// </summary>
        /// <param name="name">Name Of The Saved Class</param>
        /// <param name="saveFile">Name Od The Save File</param>
        public static T LoadClass<T>(string name, int saveFile = 0) where T : class, new()
        {
            SaveData sd = LoadOrCreate(saveFile);

            if (_settings == null)
            {
                Debug.LogError("BoltSave Not Initialized. Call 'BoltSave.Initialize' Once Before Getting Value");

                return new T();
            }

            int index = sd.classes.FindIndex(x => x.name == name);

            if (index > -1)
                return JsonUtility.FromJson<T>(sd.classes[index].value);

            Debug.LogError($"Could Not Find Class Named: {name}\nReturned New Class");
            return new T();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_settings != null || _isLoading)
                return;

            _isLoading = true;

            _settings = Resources.Load<SavingConfigAsset>("SaveSettings");

            for (int i = 0; i <_settings.fileName.Count; i++)
                LoadOrCreate(i);
            
            Debug.Log("Save Settings Loaded.");

            _isLoading = false;
        }

        public static SaveData LoadOrCreate(int saveFile = 0)
        {
            var fullPath = _settings.GetFullPath(saveFile);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            SaveData sd = new();

            if (!File.Exists(fullPath))
            {
                if (_settings.defaults != null && saveFile < _settings.defaults.Count)
                {
                    SaveFileDefaults def = _settings.defaults[saveFile];
                    sd.floats = new(def.floats);
                    sd.ints = new(def.ints);
                    sd.Vector3s = new(def.Vector3s);
                    sd.Vector2s = new(def.Vector2s);
                    sd.strings = new(def.strings);
                    sd.bools = new(def.bools);
                }
                
                string newJsonFile = JsonUtility.ToJson(sd, _settings.useEncryption);
                File.WriteAllText(fullPath, newJsonFile);
            }

            string jsonFile = File.ReadAllText(fullPath);
            sd = JsonUtility.FromJson<SaveData>(jsonFile);

            return sd;
        }

        public static void ResetSave()
        {
            SaveData newSD = new SaveData();
            SaveFile(newSD);
            Debug.Log("Saved Is Reset");
        }
    }

    [Serializable]
    public class SaveData
    {
        public List<SaveFloat> floats;
        public List<SaveInt> ints;
        public List<SaveVector3> Vector3s;
        public List<SaveVector2> Vector2s;
        public List<SaveString> strings;
        public List<SaveBool> bools;
        public List<SaveClass> classes;
    }

    [Serializable]
    public class SaveFloat
    {
        public string name;
        public float value;
    }

    [Serializable]
    public class SaveInt
    {
        public string name;
        public int value;
    }

    [Serializable]
    public class SaveVector3
    {
        public string name;
        public Vector3 value;
    }

    [Serializable]
    public class SaveVector2
    {
        public string name;
        public Vector2 value;
    }

    [Serializable]
    public class SaveString
    {
        public string name;
        public string value;
    }

    [Serializable]
    public class SaveBool
    {
        public string name;
        public bool value;
    }

    [Serializable]
    public class SaveClass
    {
        public string name;
        public string value;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class BoltsSave
{
    private const string SETTINGS_ADDRESS = "SaveSettings.savecfg";
    public static SavingConfigAsset _settings;
    private static bool _isLoading;

    #if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void InitializeInEditor()
    {
        Initialize();
    }
    #endif

    public static void SaveFloatValue(string name, float value)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return;
        }

        SaveFloat sf = new() { name = name, value = value };

        int index = -1;
        if(sd.floats != null)
            index = sd.floats.FindIndex(x => x.name == name);

        if (index > -1)
            sd.floats[index].value = value;
        else
            sd.floats.Add(sf);

        SaveFile(sd);
    }

    public static void SaveIntValue(string name, int value)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return;
        }

        SaveInt si = new() { name = name, value = value };

        int index = -1;
        if(sd.ints != null)
            index = sd.ints.FindIndex(x => x.name == name);

        if (index > -1)
            sd.ints[index].value = value;
        else
            sd.ints.Add(si);

        SaveFile(sd);
    }

    public static void SaveStringValue(string name, string value)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return;
        }

        SaveString ss = new() { name = name, value = value };

        int index = -1;
        if(sd.strings != null)
            index = sd.strings.FindIndex(x => x.name == name);

        if (index > -1)
            sd.strings[index].value = value;
        else
            sd.strings.Add(ss);

        SaveFile(sd);
    }

    public static void SaveBoolValue(string name, bool value)
    {
        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return;
        }

        SaveData sd = LoadOrCreate();

        SaveBool sb = new() { name = name, value = value };

        int index = -1;
        if(sd.bools != null)
            index = sd.bools.FindIndex(x => x.name == name);
        
        if (index > -1)
            sd.bools[index].value = value;
        else
            sd.bools.Add(sb);

        SaveFile(sd);
    }

    public static void SaveClassVariable<T>(string name, T classInstance) where T : class
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return;
        }

        SaveClass sc = new SaveClass() { name = name, value = JsonUtility.ToJson(classInstance) };

        int index = -1;
        
        if(sd.classes != null)
            index = sd.classes.FindIndex(x => x.name == name);
        
        if (index > -1)
            sd.classes[index].value = JsonUtility.ToJson(classInstance);
        else
            sd.classes.Add(sc);

        SaveFile(sd);
    }

    public static void SaveFile(SaveData sd)
    {
        string fullPath = _settings.GetFullPath();

        string newJson = JsonUtility.ToJson(sd, _settings.useEncryption);
        File.WriteAllText(fullPath, newJson);
    }

    public float GetFloat(string name)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return -1;
        }

        int index = sd.floats.FindIndex(x => x.name == name);

        if (index > -1)
            return sd.floats[index].value;

        Debug.LogError($"Could Not Find Float Named: {name}");
        return -1;
    }

    public int GetInt(string name)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return -1;
        }

        int index = sd.ints.FindIndex(x => x.name == name);

        if (index > -1)
            return sd.ints[index].value;

        Debug.LogError($"Could Not Find Int Named: {name}");
        return -1;
    }

    public static string GetString(string name)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return String.Empty;
        }

        int index = sd.strings.FindIndex(x => x.name == name);

        if (index > -1)
            return sd.strings[index].value;

        Debug.LogError($"Could Not Find String Named: {name}");
        return String.Empty;
    }

    public static bool GetBool(string name)
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return false;
        }

        int index = sd.bools.FindIndex(x => x.name == name);
        if (index > -1)
            return sd.bools[index].value;

        Debug.LogError($"Could Not Find Bool Named: {name}");
        return false;
    }

    public static T LoadClass<T>(string name) where T : class
    {
        SaveData sd = LoadOrCreate();

        if (_settings == null)
        {
            Debug.LogError("SaveSystem not initialized. Call SaveSystem.Initialize() once before saving.");

            return null;
        }

        int index = sd.bools.FindIndex(x => x.name == name);

        if (index > -1)
            return JsonUtility.FromJson<T>(sd.classes[index].value);

        Debug.LogError($"Could Not Find Class Named: {name}");
        return null;
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if(_settings != null || _isLoading)
            return;

        _isLoading = true;

        _settings = Resources.Load<SavingConfigAsset>("SaveSettings");

        
        if(_settings == null)
            Debug.LogError($"SaveSettings failed to load. Check Resources path: {SETTINGS_ADDRESS}");
        else
            Debug.Log("SaveSettings loaded.");

        _isLoading = false;
    }

    public static SaveData LoadOrCreate()
    {
        var fullPath = _settings.path;
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        SaveData sd = new();

        if (!File.Exists(fullPath))
        {
            string newJsonFile = JsonUtility.ToJson(sd, _settings.useEncryption);
            File.WriteAllText(fullPath, newJsonFile);
        }

        string jsonFile = File.ReadAllText(fullPath);
        sd = JsonUtility.FromJson<SaveData>(jsonFile);

        return sd;
    }
}

[Serializable]
public class SaveData
{
    public List<SaveFloat> floats;
    public List<SaveInt> ints;
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
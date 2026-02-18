using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class BoltsSavingWindow : EditorWindow
{
    SavingConfigAsset config;
    SerializedObject serializedConfig;
    private Vector2 scrollPos;

    string jsonFilePath;
    SaveData sd;
    const string ConfigPath = "Assets/BoltsTools/SaveSettings.savecfg";

    private static Rect pos;

    [MenuItem("Tools/Bolts Tools/Save Settings")]
    static void ShowWindow()
    {
        BoltsSavingWindow window = GetWindow<BoltsSavingWindow>(true,"Save Settings Window", true);
        
        window.minSize = new(400, 400);
        window.maxSize = new(400, 1000);
    }

    void OnEnable()
    {
        LoadConfig();
    }

    void LoadConfig()
    {
        config = AssetDatabase.LoadAssetAtPath<SavingConfigAsset>(ConfigPath);

        if (config != null)
        {
            serializedConfig = new(config);
        }
        else
        {
            Debug.Log("Could Not Find Settings... Making One");

            SavingConfigAsset newFile = new();
            AssetDatabase.CreateAsset(newFile, ConfigPath);

            serializedConfig = new(AssetDatabase.LoadAssetAtPath<SavingConfigAsset>(ConfigPath));
        }
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        pos = position;
        
        if (config == null)
        {
            EditorGUILayout.HelpBox($"Config file not found at:\n{ConfigPath}", MessageType.Error);

            if (GUILayout.Button("Reload"))
            {
                LoadConfig();
            }

            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        serializedConfig.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
        config.fileName = EditorGUILayout.TextField("Save File Name", config.fileName);
        config.usePersistentDataPath = EditorGUILayout.Toggle("Use Persistent Data Path", config.usePersistentDataPath);
        config.useEncryption = EditorGUILayout.Toggle("Use Encryption", config.useEncryption);

        if(EditorGUI.EndChangeCheck())
            SaveToFile();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Show Saved Data"))
        {
            if(BoltsSave._settings != null)
            {
                SavingConfigAsset sca = BoltsSave._settings;

                if (!File.Exists(sca.GetFullPath()))
                {
                    string path = EditorUtility.OpenFilePanel("Select JSON File", "Assets", "json");
                    if (!string.IsNullOrEmpty(path))
                        jsonFilePath = path;
                }

                jsonFilePath = sca.GetFullPath();

                LoadSaveData();
            }
            else
                BoltsSave.Initialize();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reload JSON"))
        {
            LoadSaveData();
        }

        EditorGUILayout.Space(20);

        if (sd != null)
        {
            EditorGUILayout.LabelField("Saved Data Variables", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            ShowValues();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Save to File", GUILayout.Height(30)))
            SaveToFile();
    }

    void SaveToFile()
    {
        if(config == null) return;

        string path = AssetDatabase.GetAssetPath(config);

        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine($"saveFileName={config.fileName}");
            writer.WriteLine($"usePersistentDataPath={config.usePersistentDataPath}");
            writer.WriteLine($"useEncryption={config.useEncryption}");
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Debug.Log($"✓ Configuration saved");
    }

    void LoadSaveData()
    {
        if (File.Exists(jsonFilePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                sd = JsonUtility.FromJson<SaveData>(jsonContent);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading JSON: {e.Message}");
            }
        }
        else
            Debug.LogWarning($"JSON file not found at: {jsonFilePath}");
    }

    void ShowValues()
    {
        bool needSave = false;

        if (sd.floats is { Count: > 0 })
        {
                EditorGUILayout.LabelField("Floats:", EditorStyles.boldLabel);
                for (int i = 0; i < sd.floats.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    sd.floats[i].name = EditorGUILayout.TextField(sd.floats[i].name, GUILayout.Width(150));
                    sd.floats[i].value = EditorGUILayout.FloatField(sd.floats[i].value);

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        sd.floats.RemoveAt(i);
                        needSave = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space(5);
        }

            // Display and edit Ints
        if (sd.ints is { Count: > 0 })
        {
            EditorGUILayout.LabelField("Ints:", EditorStyles.boldLabel);
                for (int i = 0; i < sd.ints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    sd.ints[i].name = EditorGUILayout.TextField(sd.ints[i].name, GUILayout.Width(150));
                    sd.ints[i].value = EditorGUILayout.IntField(sd.ints[i].value);

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        sd.ints.RemoveAt(i);
                        needSave = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space(5);
        }

        // Display and edit Strings
        if (sd.strings is { Count: > 0 })
        {
            EditorGUILayout.LabelField("Strings:", EditorStyles.boldLabel);
            for (int i = 0; i < sd.strings.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                sd.strings[i].name = EditorGUILayout.TextField(sd.strings[i].name, GUILayout.Width(150));
                sd.strings[i].value = EditorGUILayout.TextField(sd.strings[i].value);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    sd.strings.RemoveAt(i);
                    needSave = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);
        }

        // Display and edit Bools
        if (sd.bools is { Count: > 0 })
        {
            EditorGUILayout.LabelField("Bools:", EditorStyles.boldLabel);
            for (int i = 0; i < sd.bools.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                sd.bools[i].name = EditorGUILayout.TextField(sd.bools[i].name, GUILayout.Width(150));
                sd.bools[i].value = EditorGUILayout.Toggle(sd.bools[i].value);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    sd.bools.RemoveAt(i);
                    needSave = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);
        }

        // Display Classes (read-only for now)
        if (sd.classes is { Count: > 0 })

        {
            EditorGUILayout.LabelField("Classes:", EditorStyles.boldLabel);
            for (int i = 0; i < sd.classes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                sd.classes[i].name = EditorGUILayout.TextField(sd.classes[i].name, GUILayout.Width(150));
                EditorGUILayout.TextField(sd.classes[i].value);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    sd.classes.RemoveAt(i);
                    needSave = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(5);
        }

        if (needSave)
        {
            BoltsSave.SaveFile(sd);
            Repaint();
        }
    }

    void OnDestroy()
    {
        BoltsSave.SaveFile(sd);
    }
}
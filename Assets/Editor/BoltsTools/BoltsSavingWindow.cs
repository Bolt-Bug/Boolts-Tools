using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BoltsSavingWindow : EditorWindow
{
    SavingConfigAsset config;
    SerializedObject serializedConfig;
    private Vector2 scrollPos;

    private const string CONFIG_PATH = "Assets/BoltsTools/SaveSettings.savecfg";

    [MenuItem("Tools/Bolts Tools/Save Settings")]
    static void ShowWindow()
    {
        GetWindow<BoltsSavingWindow>("Save Settings Window");
    }

    void OnEnable()
    {
        LoadConfig();
    }

    void LoadConfig()
    {
        config = AssetDatabase.LoadAssetAtPath<SavingConfigAsset>(CONFIG_PATH);

        if (config != null)
        {
            serializedConfig = new(config);
        }
        else
        {
            Debug.Log("Could Not Find Settings... Making One");

            SavingConfigAsset newFile = new SavingConfigAsset();
            AssetDatabase.CreateAsset(newFile, CONFIG_PATH);

            serializedConfig = new(AssetDatabase.LoadAssetAtPath<SavingConfigAsset>(CONFIG_PATH));
        }
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (config == null)
        {
            EditorGUILayout.HelpBox($"Config file not found at:\n{CONFIG_PATH}", MessageType.Error);

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
}
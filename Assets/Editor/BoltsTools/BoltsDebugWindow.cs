using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace editor.BoltsTools
{
    public class BoltsDebugWindow : EditorWindow
    {
        BoltsDebugMenuSettings config;
        SerializedObject serializedConfig;
        Vector2 scrollPos;

        string jsonFilePath;
        const string ConfigPath = "Assets/Resources/DebugSettings.debug";

        [MenuItem("Tools/Bolts Tools/Debug Settings")]
        public static void OpenWindow()
        {
            BoltsDebugWindow window = GetWindow<BoltsDebugWindow>(true, "Debug Settings Window", true);
            
            window.minSize = new(400, 200);
            window.maxSize = new(400, 200);
        }

        void OnEnable()
        {
            LoadConfig();
        }

        void LoadConfig()
        {
            config = CreateInstance<BoltsDebugMenuSettings>();

            string[] settingsFile = File.ReadAllLines(ConfigPath);
            foreach (string line in settingsFile)
            {
                if (line.StartsWith("keyToOpen"))
                    config.keyToOpenDebug = Enum.Parse<KeyCode>(line.Split("=")[1]);
            }
            
            if(config != null)
                serializedConfig = new(config);
            else
            {
                Debug.Log("Could Not Find Settings... Making One");

                BoltsDebugMenuSettings newFile = new();
                AssetDatabase.CreateAsset(newFile, ConfigPath);

                serializedConfig = new(AssetDatabase.LoadAssetAtPath<BoltsDebugMenuSettings>(ConfigPath));
            }
        }
        
        void OnGUI()
        {
            EditorGUILayout.LabelField("Debug Window Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (config == null)
            {
                EditorGUILayout.HelpBox($"Config file not found at:{ConfigPath}", MessageType.Error);

                if (GUILayout.Button("Reload"))
                {
                    LoadConfig();
                }

                return;
            }
            
            serializedConfig.Update();
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            config.keyToOpenDebug = (KeyCode)EditorGUILayout.EnumPopup("Key To Open", config.keyToOpenDebug);
            
            if(EditorGUI.EndChangeCheck())
                SaveToFile();
        }
        
        void SaveToFile()
        {
            if (config == null) return;
            
            using (StreamWriter writer = new StreamWriter(ConfigPath))
            {
                writer.WriteLine($"keyToOpen={config.keyToOpenDebug}");
            }

            AssetDatabase.ImportAsset(ConfigPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"✓ Configuration saved");
        }
    }
}


using System;
using System.IO;
using BoltsTools;
using UnityEditor;
using UnityEngine;

namespace Editor.BoltsTools
{
    public class BoltsSavingWindow : EditorWindow
    {
        SavingConfigAsset config;
        SerializedObject serializedConfig;
        Vector2 listScrollPos;
        Vector2 scrollPos;

        string jsonFilePath;
        SaveData sd;
        
        int index;

        [MenuItem("Tools/Bolts Tools/Save Settings &s")]
        static void ShowWindow()
        {
            BoltsSavingWindow window = GetWindow<BoltsSavingWindow>(true, "Save Settings Window", true);

            window.minSize = new(400, 400);
            window.maxSize = new(400, 1000);
        }

        void OnEnable()
        {
            LoadConfig();
        }

        void LoadConfig()
        {
            config = Resources.Load<SavingConfigAsset>("SaveSettings");
            
            serializedConfig = new SerializedObject(config);
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            serializedConfig.Update();

            EditorGUI.BeginChangeCheck();
            
            if (config == null)
            {
                EditorGUILayout.HelpBox($"Config file not found", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);

            float listHeight = Mathf.Min(config.fileName.Count * 22 + 10, 200);
            listScrollPos = GUILayout.BeginScrollView(listScrollPos, GUILayout.Height(listHeight));
            for (int i = 0; i < config.fileName.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                config.fileName[i] = EditorGUILayout.TextField("Save Files Name", config.fileName[i]);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            
            config.usePersistentDataPath =
                EditorGUILayout.Toggle("Use Persistent Data Path", config.usePersistentDataPath);
            config.useEncryption = EditorGUILayout.Toggle("Use Encryption", config.useEncryption);

            string fileToCheck = "";
            
            index = EditorGUILayout.Popup("Save File", index, config.fileName.ToArray());
            fileToCheck = config.fileName[index];
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Show Saved Data"))
            {
                if (BoltsSave._settings != null)
                {
                    SavingConfigAsset sca = BoltsSave._settings;

                    if (!File.Exists(sca.GetFullPath(fileToCheck)))
                    {
                        BoltsSave.LoadOrCreate(fileToCheck);
                    }

                    jsonFilePath = sca.GetFullPath(fileToCheck);

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

            // Display And Edit Vector3
            if (sd.Vector3s is { Count: > 0 })
            {
                EditorGUILayout.LabelField("Vector3s:", EditorStyles.boldLabel);
                for (int i = 0; i < sd.Vector3s.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUIContent vectorLabel = new GUIContent(sd.Vector3s[i].name);
                    sd.Vector3s[i].value = EditorGUILayout.Vector3Field(vectorLabel, sd.Vector3s[i].value);

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        sd.Vector3s.RemoveAt(i);
                        needSave = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5);
            }

            // Display And Edit Vector2
            if (sd.Vector2s is { Count: > 0 })
            {
                EditorGUILayout.LabelField("Vector2s:", EditorStyles.boldLabel);
                for (int i = 0; i < sd.Vector2s.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUIContent vectorLabel = new GUIContent(sd.Vector2s[i].name);
                    sd.Vector2s[i].value = EditorGUILayout.Vector2Field(vectorLabel, sd.Vector2s[i].value);

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        sd.Vector2s.RemoveAt(i);
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
            if (sd != null)
                BoltsSave.SaveFile(sd);
        }
    }
}
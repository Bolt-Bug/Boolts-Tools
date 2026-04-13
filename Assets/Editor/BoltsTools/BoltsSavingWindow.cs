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
            
            index = EditorGUILayout.Popup("Save File", index, config.fileName.ToArray());
            int fileToCheck = index;
            
            if (GUILayout.Button("Add Default Save Values"))
            {
                AddDefaultValue.OpenWindow(fileToCheck);
            }
            
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

    public class AddDefaultValue : EditorWindow
    {
        static int fileToChange;
        SaveTypes saveType;

        string valueName;
        
        string savedString;
        float savedFloat;
        int savedInt;
        Vector3 savedVector3;
        Vector2 savedVector2;
        bool savedBool;
        
        public static void OpenWindow(int file)
        {
            AddDefaultValue window = GetWindow<AddDefaultValue>(true, "Save Settings Window", true);

            window.minSize = new(400, 400);
            window.maxSize = new(400, 1000);

            fileToChange = file;
        }

        void OnGUI()
        {
            saveType = (SaveTypes)EditorGUILayout.EnumPopup(saveType);
            
            valueName = EditorGUILayout.TextField("Value Name",valueName);
            
            switch (saveType)
            {
                case SaveTypes.Float:
                    savedFloat = EditorGUILayout.FloatField(savedFloat);
                    break;
                
                case SaveTypes.Int:
                    savedInt = EditorGUILayout.IntField(savedInt);
                    break;
                
                case SaveTypes.Vector3:
                    savedVector3 = EditorGUILayout.Vector3Field("",savedVector3);
                    break;
                
                case SaveTypes.Vector2:
                    savedVector2 = EditorGUILayout.Vector2Field("", savedVector2);
                    break;
                
                case SaveTypes.String:
                    savedString = EditorGUILayout.TextField(savedString);
                    break;
                
                case SaveTypes.Bool:
                    savedBool = EditorGUILayout.Toggle(savedBool);
                    break;
            }

            if (GUILayout.Button("Add Value"))
            {
                if (string.IsNullOrEmpty(valueName))
                {
                    Debug.LogError("Value Name Can Not Be Empty");
                    return;
                }
                
                SavingConfigAsset settings = BoltsSave._settings;

                while (settings.defaults.Count <= fileToChange)
                    settings.defaults.Add(new SaveFileDefaults());
                
                SaveFileDefaults def = settings.defaults[fileToChange];
                
                switch (saveType)
                {
                    case SaveTypes.Float:
                        def.floats.Add(new SaveFloat { name = valueName, value = savedFloat });
                        break;
                    case SaveTypes.Int:
                        def.ints.Add(new SaveInt { name = valueName, value = savedInt });
                        break;
                    case SaveTypes.Vector3:
                        def.Vector3s.Add(new SaveVector3 { name = valueName, value = savedVector3 });
                        break;
                    case SaveTypes.Vector2:
                        def.Vector2s.Add(new SaveVector2 { name = valueName, value = savedVector2 });
                        break;
                    case SaveTypes.String:
                        def.strings.Add(new SaveString { name = valueName, value = savedString });
                        break;
                    case SaveTypes.Bool:
                        def.bools.Add(new SaveBool { name = valueName, value = savedBool });
                        break;
                }
                
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();

                valueName = "";
                Repaint();
            }
            
            SavingConfigAsset s = BoltsSave._settings;
            
            if (s == null || s.defaults == null || fileToChange >= s.defaults.Count)
                return;
            
            SaveFileDefaults current = s.defaults[fileToChange];

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Existing Defaults", EditorStyles.boldLabel);

            bool dirty = false;

            switch (saveType)
            {
                case SaveTypes.Float:
                    for (int i = 0; i < current.floats.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.floats[i].name = EditorGUILayout.TextField(current.floats[i].name, GUILayout.Width(150));
                        current.floats[i].value = EditorGUILayout.FloatField(current.floats[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.floats.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                
                case SaveTypes.Int:
                    for (int i = 0; i < current.ints.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.ints[i].name = EditorGUILayout.TextField(current.ints[i].name, GUILayout.Width(150));
                        current.ints[i].value = EditorGUILayout.IntField(current.ints[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.ints.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                
                case SaveTypes.Vector3:
                    for (int i = 0; i < current.Vector3s.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.Vector3s[i].name = EditorGUILayout.TextField(current.Vector3s[i].name, GUILayout.Width(150));
                        current.Vector3s[i].value = EditorGUILayout.Vector3Field("", current.Vector3s[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.Vector3s.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                
                case SaveTypes.Vector2:
                    for (int i = 0; i < current.Vector2s.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.Vector2s[i].name = EditorGUILayout.TextField(current.Vector2s[i].name, GUILayout.Width(150));
                        current.Vector2s[i].value = EditorGUILayout.Vector2Field("", current.Vector2s[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.Vector2s.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                
                case SaveTypes.String:
                    for (int i = 0; i < current.strings.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.strings[i].name = EditorGUILayout.TextField(current.strings[i].name, GUILayout.Width(150));
                        current.strings[i].value = EditorGUILayout.TextField(current.strings[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.strings.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                
                case SaveTypes.Bool:
                    for (int i = 0; i < current.bools.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        current.bools[i].name = EditorGUILayout.TextField(current.bools[i].name, GUILayout.Width(150));
                        current.bools[i].value = EditorGUILayout.Toggle(current.bools[i].value);
                        
                        if(GUILayout.Button("X", GUILayout.Width(25))) { current.bools.RemoveAt(i); dirty = true; }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(s);
                AssetDatabase.SaveAssets();
                Repaint();
            }
        }
    }
    
    public enum SaveTypes {Float, Int, Vector3, Vector2, String, Bool,}
}
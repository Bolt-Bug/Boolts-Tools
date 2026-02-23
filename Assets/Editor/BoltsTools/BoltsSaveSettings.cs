using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "savecfg")]
public class BoltsSaveSettings : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var config = ScriptableObject.CreateInstance<SavingConfigAsset>();

        if (File.Exists(ctx.assetPath))
        {
            string[] lines = File.ReadAllLines(ctx.assetPath);

            foreach (string line in lines)
            {
                if (line.StartsWith("saveFileName"))
                    config.fileName = line.Split("=")[1];
                else if (line.StartsWith("usePersistentDataPath"))
                    config.usePersistentDataPath = bool.Parse(line.Split("=")[1]);
                else if (line.StartsWith("useEncryption"))
                    config.useEncryption = bool.Parse(line.Split("=")[1]);
            }

            config.path = config.usePersistentDataPath ? Application.persistentDataPath : Application.dataPath;
        }

        config.hideFlags = HideFlags.None;

        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BoltsTools/Sprites/SettingsLogo.png");

        ctx.AddObjectToAsset("main", config, icon);
        ctx.SetMainObject(config);
    }
}

[CustomEditor(typeof(SavingConfigAsset))]
public class SaveConfigEditor : Editor
{
    SavingConfigAsset config
    {
        get { return (target as SavingConfigAsset); }
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = true;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);
        config.fileName = EditorGUILayout.TextField("Save File Name", config.fileName);
        config.usePersistentDataPath = EditorGUILayout.Toggle("Use Persistent Data Path", config.usePersistentDataPath);
        config.useEncryption = EditorGUILayout.Toggle("Use Encryption", config.useEncryption);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Edit values above, then click Save to write to .savecfg file", MessageType.Info);

        if (GUILayout.Button("Save Changes") || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(config);

            string path = AssetDatabase.GetAssetPath(config);

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"saveFileName={config.fileName}");
                writer.WriteLine($"usePersistentDataPath={config.usePersistentDataPath}");
                writer.WriteLine($"useEncryption={config.useEncryption}");
            }

            AssetDatabase.ImportAsset(path);
        }
    }
}
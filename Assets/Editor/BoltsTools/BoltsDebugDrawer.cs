using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.BoltsTools
{
    [ScriptedImporter(1, ".debug")]
    public class BoltsDebugDrawer : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var config = ScriptableObject.CreateInstance<BoltsDebugMenuSettings>();

            if (File.Exists(ctx.assetPath))
            {
                string[] lines = File.ReadAllLines(ctx.assetPath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("keyToOpenDebug"))
                        config.keyToOpenDebug = Enum.Parse<KeyCode>(line.Split("=")[1]);
                    else if (line.StartsWith("keyToOpenCommands"))
                        config.keyToOpenCommands = Enum.Parse<KeyCode>(line.Split("=")[1]);
                    else if (line.StartsWith("showFPS"))
                        config.showFPS = bool.Parse(line.Split("=")[1]);
                    else if (line.StartsWith("showPlayerPos"))
                        config.showPlayerPos = bool.Parse(line.Split("=")[1]);
                    else if (line.StartsWith("playerTag"))
                        config.playerTag = line.Split("=")[1];
                }
            }

            config.hideFlags = HideFlags.None;
            
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BoltsTools/Sprites/DebugLogo.png");
            
            ctx.AddObjectToAsset("main", config, icon);
            ctx.SetMainObject(config);
        }
    }

    [CustomEditor(typeof(BoltsDebugMenuSettings))]
    public class DebugConfigEditor : UnityEditor.Editor
    {
        BoltsDebugMenuSettings config
        {
            get { return (target as BoltsDebugMenuSettings); }
        }
        
        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            config.keyToOpenDebug = (KeyCode)EditorGUILayout.EnumPopup("Key To Open", config.keyToOpenDebug);
            config.keyToOpenCommands = (KeyCode)EditorGUILayout.EnumPopup("Key To Open Commands", config.keyToOpenCommands);
            config.showFPS = EditorGUILayout.Toggle("Show FPS", config.showFPS);
            config.showPlayerPos = EditorGUILayout.Toggle("Show Player Position", config.showPlayerPos);

            string[] allTags = UnityEditorInternal.InternalEditorUtility.tags;
            int index = Array.IndexOf(allTags, config.playerTag);

            index = EditorGUILayout.Popup("Player Tag", index, allTags);
            config.playerTag = allTags[index];
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Edit values above to write to .config file", MessageType.Info);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(config);

                string path = AssetDatabase.GetAssetPath(config);

                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"keyToOpenDebug={config.keyToOpenDebug}");
                    writer.WriteLine($"keyToOpenCommands={config.keyToOpenCommands}");
                    writer.WriteLine($"showFPS={config.showFPS}");
                    writer.WriteLine($"showPlayerPos={config.showPlayerPos}");
                    writer.WriteLine($"playerTag={config.playerTag}");
                }
                
                AssetDatabase.ImportAsset(path);
            }
        }
    }
}

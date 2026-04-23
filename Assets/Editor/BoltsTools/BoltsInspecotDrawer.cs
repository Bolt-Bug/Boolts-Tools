using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BoltsTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor.Animations;

namespace editor.BoltsTools
{
    [CustomPropertyDrawer(typeof(BoltsCommentAttribute))]
    public class BoltsCommentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BoltsCommentAttribute comment = (BoltsCommentAttribute)attribute;

            float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
            float commentHeight = EditorGUIUtility.singleLineHeight * 1.3f;

            float y = position.y;

            Rect commentRect = new Rect(position.x, y, position.width, commentHeight);

            EditorGUI.HelpBox(commentRect, comment.comment, MessageType.None);

            Rect fieldRect = new Rect(position.x, y + commentHeight + 2, position.width, fieldHeight);
            EditorGUI.PropertyField(fieldRect, property, label, true);

            EditorGUILayout.Space(comment.space);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float commentHeight = EditorGUIUtility.singleLineHeight * 1.3f;
            float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);

            return fieldHeight + commentHeight + 4;
        }
    }

    [CustomPropertyDrawer(typeof(BoltsInputActionAttribute))]
    public class BoltsInputActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (BoltsInputActionAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [InputActionMap] on a string field.");
                EditorGUI.EndProperty();

                return;
            }

            var assetProperty = property.serializedObject.FindProperty(attr.actionAssetField);

            if (assetProperty == null || assetProperty.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.HelpBox(position, $"BoltsInputActionAttribute '{attr.actionAssetField}' not found",
                    MessageType.Warning);

                return;
            }

            var asset = assetProperty.objectReferenceValue as InputActionAsset;

            if (asset == null)
            {
                EditorGUI.LabelField(position, label.text, "Field is not an BoltsInputActionAttribute.");
                EditorGUI.EndProperty();

                return;
            }

            var maps = asset.actionMaps;

            if (maps.Count == 0)
            {
                EditorGUI.LabelField(position, label.text, "No Action Maps in asset.");
                EditorGUI.EndProperty();

                return;
            }

            string[] mapNames = maps.Select(m => m.name).ToArray();

            int index = Mathf.Max(0, Array.IndexOf(mapNames, property.stringValue));
            if (index >= mapNames.Length)
                index = 0;

            int newIndex = EditorGUI.Popup(position, label.text, index, mapNames);
            property.stringValue = mapNames[newIndex];

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(BoltsShaderPropertyAttribute))]
    public class BoltsShaderPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (BoltsShaderPropertyAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [ShaderProperty] on a string field.");
                EditorGUI.EndProperty();
                return;
            }

            var matProp = FindSiblingProperty(property, attr.materialField);

            if (matProp == null || matProp.objectReferenceValue == null)
            {
                EditorGUI.LabelField(position, label.text, "Assign a Material first.");
                EditorGUI.EndProperty();
                return;
            }

            var mat = matProp.objectReferenceValue as Material;

            if (!mat || !mat.shader)
            {
                EditorGUI.LabelField(position, label.text, "Invalid Material or Shader.");
                EditorGUI.EndProperty();
                return;
            }

            var shader = mat.shader;
            int count = shader.GetPropertyCount();

            if (count == 0)
            {
                EditorGUI.LabelField(position, label.text, "Shader has no properties.");
                EditorGUI.EndProperty();
                return;
            }

            List<string> propNames = new List<string>(count);

            for (int i = 0; i < count; i++)
                propNames.Add(shader.GetPropertyName(i));

            int index = Mathf.Max(0, propNames.IndexOf(property.stringValue));
            if (index >= propNames.Count) index = 0;

            int newIndex = EditorGUI.Popup(position, label.text, index, propNames.ToArray());
            property.stringValue = propNames[newIndex];

            EditorGUI.EndProperty();
        }

        private static SerializedProperty FindSiblingProperty(SerializedProperty property, string siblingName)
        {
            var direct = property.FindPropertyRelative(siblingName);

            if (direct != null)
                return direct;

            string path = property.propertyPath;
            int lastDot = path.LastIndexOf(".", StringComparison.Ordinal);

            if (lastDot < 0)
                return property.serializedObject.FindProperty(siblingName);

            string parentPath = path.Substring(0, lastDot);
            var parent = property.serializedObject.FindProperty(parentPath);

            if (parent == null)
                return null;

            return parent.FindPropertyRelative(siblingName);
        }
    }

    [CustomPropertyDrawer(typeof(BoltsSaveAttribute))]
    public class BoltsSaveAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[SavedVariable] Only Works On Sting Fields", MessageType.Error);
                return;
            }

            BoltsSaveAttribute bsa = (BoltsSaveAttribute)attribute;
            List<string> names = GetVariableNames(bsa.filterType, bsa.saveIndex);

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = new(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect buttonRect = new(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);

            string current = property.stringValue;
            string display = string.IsNullOrEmpty(current) ? "-- None --" : current;

            if (EditorGUI.DropdownButton(buttonRect, new(display), FocusType.Keyboard))
            {
                GenericMenu menu = new();

                if (names.Count == 0)
                    menu.AddDisabledItem(new("No Saved Variables Found"));
                else
                {
                    menu.AddItem(new("-- None --"), string.IsNullOrEmpty(current), () =>
                    {
                        property.stringValue = "";
                        property.serializedObject.ApplyModifiedProperties();
                    });

                    foreach (string name in names)
                    {
                        string captured = name;
                        menu.AddItem(new(captured), current == captured, () =>
                        {
                            property.stringValue = captured;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                menu.DropDown(buttonRect);
            }

            EditorGUI.EndProperty();
        }

        List<string> GetVariableNames(SavedVariableType filter, int saveFile)
        {
            List<string> names = new();

            SavingConfigAsset settings = BoltsSave._settings;

            if (settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:SavingConfigAsset");
                if (guids.Length == 0)
                    return names;

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<SavingConfigAsset>(path);
            }

            if (settings == null)
                return names;

            string fullPath = settings.GetFullPath(saveFile);

            if (!File.Exists(fullPath))
                return names;

            string json = File.ReadAllText(fullPath);
            SaveData sd = JsonUtility.FromJson<SaveData>(json);

            if (sd == null)
                return names;

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Float) && sd.floats != null)
                foreach (var item in sd.floats)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Int) && sd.ints != null)
                foreach (var item in sd.ints)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Vector3) && sd.Vector3s != null)
                foreach (var item in sd.Vector3s)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Vector2) && sd.Vector2s != null)
                foreach (var item in sd.Vector2s)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Bool) && sd.bools != null)
                foreach (var item in sd.bools)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.String) && sd.strings != null)
                foreach (var item in sd.strings)
                    names.Add(item.name);

            if ((filter == SavedVariableType.Any || filter == SavedVariableType.Class) && sd.classes != null)
                foreach (var item in sd.classes)
                    names.Add(item.name);

            return names;
        }
    }

    [CustomPropertyDrawer(typeof(BoltsAnimationClipAttribute))]
    public class BoltsAnimationClipDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (BoltsAnimationClipAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [BoltsAnimationClip] On A string Field");
                EditorGUI.EndProperty();

                return;
            }

            var assetProperty = property.serializedObject.FindProperty(attr.animator);

            if (assetProperty == null || assetProperty.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.HelpBox(position, $"BoltsAnimationClip {attr.animator} Not Found", MessageType.Error);

                return;
            }

            AnimationClip[] clips;
            
            var asAnimator = assetProperty.objectReferenceValue as Animator;
            var asController = assetProperty.objectReferenceValue as AnimatorController;

            if (asAnimator != null)
                clips = asAnimator.runtimeAnimatorController.animationClips;
            else if (asController != null)
                clips = asController.animationClips;
            else
            {
                EditorGUI.LabelField(position, label.text, "Field is not an Animator Or Controller");
                EditorGUI.EndProperty();

                return;
            }

            if (clips.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "No Clips Found In Animator.");
                EditorGUI.EndProperty();

                return;
            }

            string[] clipNames = clips.Select(m => m.name).ToArray();

            int index = Mathf.Max(0, Array.IndexOf(clipNames, property.stringValue));
            if (index >= clipNames.Length)
                index = 0;

            int newIndex = EditorGUI.Popup(position, label.text, index, clipNames);
            property.stringValue = clipNames[newIndex];

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(BoltsAnimationParamAttribute)) ]
    public class BoltsAnimatorParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (BoltsAnimationParamAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [BoltsAnimatorParameter] On A String Field.");
                EditorGUI.EndProperty();
                return;
            }

            var assetProperty = property.serializedObject.FindProperty(attr.animator);
            
            if(assetProperty == null || assetProperty.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.HelpBox(position, $"BoltsAnimatorParameter: '{attr.animator}' Not Found.", MessageType.Error);
                return;
            }

            AnimatorControllerParameter[] parameters;

            var asAnimator = assetProperty.objectReferenceValue as Animator;
            var asController = assetProperty.objectReferenceValue as AnimatorController;

            if (asAnimator != null)
                parameters = asAnimator.parameters;
            else if (asController != null)
                parameters = asController.parameters;
            else
            {
                EditorGUI.LabelField(position, label.text, "Field Is Not An Animator Or AnimatorController.");
                EditorGUI.EndProperty();
                return;
            }

            if (attr.filterType.HasValue)
                parameters = parameters.Where(p => p.type == attr.filterType.Value).ToArray();
            
            if (parameters.Length == 0)
            {
                string filter = attr.filterType.HasValue ? attr.filterType.Value.ToString() + " " : "";
                EditorGUI.LabelField(position, label.text, $"No {filter}Parameters Found");
                EditorGUI.EndProperty();
                return;
            }

            string[] displayNames = parameters
                .Select(p => $"{p.name} ({p.type})").ToArray();

            string[] paramNames = parameters
                .Select(p => p.name).ToArray();

            int index = Mathf.Max(0, Array.IndexOf(paramNames, property.stringValue));
            if (index >= paramNames.Length)
                index = 0;

            int newIndex = EditorGUI.Popup(position, label.text, index, displayNames);
            property.stringValue = paramNames[newIndex];
            
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(BoltsToolTipAttribute))]
    public class BoltsToolTipDrawer : PropertyDrawer
    {
        const float IconSize = 18;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BoltsToolTipAttribute bt = (BoltsToolTipAttribute)attribute;

            Rect fieldRect = new Rect(position.x + IconSize, position.y, position.width - IconSize, position.height);
            Rect iconRect = new Rect(position.x, position.y + (position.height - IconSize) / 2, IconSize,
                IconSize);

            GUIContent labelWithTooltio = new GUIContent(label.text, label.image, bt.msg);
            
            EditorGUI.PropertyField(fieldRect, property, labelWithTooltio);

            GUIContent icon = EditorGUIUtility.IconContent("console.infoicon.inactive.sml@2x");
            icon.tooltip = bt.msg;

            if (GUI.Button(iconRect, icon, GUIStyle.none))
                EditorUtility.DisplayDialog("Info", bt.msg, "OK");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    [CustomEditor(typeof(BoltsBoxCollider))]
    public class BoltsBoxColliderDrawer : Editor
    {
        Editor boxColliderEditor;

        public override void OnInspectorGUI()
        {
            BoltsBoxCollider customBC = (BoltsBoxCollider)target;

            DrawDefaultInspector();

            SerializedProperty px = serializedObject.FindProperty("px");
            SerializedProperty py = serializedObject.FindProperty("py");
            SerializedProperty pz = serializedObject.FindProperty("pz");
            SerializedProperty nx = serializedObject.FindProperty("nx");
            SerializedProperty ny = serializedObject.FindProperty("ny");
            SerializedProperty nz = serializedObject.FindProperty("nz");

            EditorGUILayout.BeginHorizontal();
            px.boolValue = EditorGUILayout.ToggleLeft("+X", px.boolValue, GUILayout.Width(40));
            py.boolValue = EditorGUILayout.ToggleLeft("+Y", py.boolValue, GUILayout.Width(40));
            pz.boolValue = EditorGUILayout.ToggleLeft("+Z", pz.boolValue, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            nx.boolValue = EditorGUILayout.ToggleLeft("-X", nx.boolValue, GUILayout.Width(40));
            ny.boolValue = EditorGUILayout.ToggleLeft("-Y", ny.boolValue, GUILayout.Width(40));
            nz.boolValue = EditorGUILayout.ToggleLeft("-Z", nz.boolValue, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Set Bounds"))
                customBC.SetBounds();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Box Collider Settings", EditorStyles.boldLabel);

            if (customBC.boxCollider != null)
            {
                customBC.boxCollider.hideFlags = HideFlags.HideInInspector;

                if (boxColliderEditor == null)
                    boxColliderEditor = Editor.CreateEditor(customBC.boxCollider);

                boxColliderEditor.DrawDefaultInspector();
            }
        }

        void OnDisable()
        {
            if (boxColliderEditor != null)
                DestroyImmediate(boxColliderEditor);
        }
    }
    [CustomEditor(typeof(BoltsMaterialInstance))]
    public class BoltsMaterialDrawer : Editor
    {
        MaterialEditor matEditor;
        Material trackedMat;

        Material[] originalMaterials;
        Renderer trackedRenderer;
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Don't Use For Finale Build!!!", MessageType.Warning);
            
            BoltsMaterialInstance bm = (BoltsMaterialInstance)this.target;
            
            EditorGUI.BeginChangeCheck();
            DrawInspector(bm);
            bool changed = EditorGUI.EndChangeCheck();
            
            if (bm.targetRenderer == null)
            {
                EditorGUILayout.HelpBox("Assign A Renderer.", MessageType.Info);
                CleanupEditor();
                return;
            }

            if (bm.targetRenderer != trackedRenderer)
            {
                originalMaterials = bm.targetRenderer.sharedMaterials.Clone()
                    as Material[];
                trackedRenderer = bm.targetRenderer;
            }
            
            if(changed && bm.mat != trackedMat)
                ApplyInstance(bm);
            
            if(bm.mat != null && bm.instancedMaterial == null)
                ApplyInstance(bm);

            if (bm.instancedMaterial == null)
            {
                EditorGUILayout.HelpBox("Assign A Material Above", MessageType.Info);
                CleanupEditor();
                return;
            }

            if (bm.instancedMaterial != trackedMat || matEditor == null)
            {
                CleanupEditor();
                matEditor = (MaterialEditor)CreateEditor(bm.instancedMaterial);
                trackedMat = bm.instancedMaterial;
            }
            
            EditorGUILayout.Space(8);
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.5f,0.5f,0.5f,0.3f));
            EditorGUILayout.Space(4);

            if (GUILayout.Button("Reset Mat"))
                PushToRenderer(bm, bm.mat, bm.materialIndex);
        }

        void ApplyInstance(BoltsMaterialInstance bm)
        {
            if (bm.mat == null)
            {
                bm.instancedMaterial = null;
                PushToRenderer(bm, null, bm.materialIndex);
                return;
            }

            bm.instancedMaterial = Instantiate(bm.mat);
            bm.instancedMaterial.name = bm.mat.name + " (Instance)";
            
            PushToRenderer(bm, bm.instancedMaterial, bm.materialIndex);

            trackedMat = bm.instancedMaterial;
            
            EditorUtility.SetDirty(bm);
        }

        static void PushToRenderer(BoltsMaterialInstance target, Material mat, int index)
        {
            SerializedObject so = new SerializedObject(target.targetRenderer);
            SerializedProperty mats = so.FindProperty("m_Materials");
            
            if(index >= mats.arraySize) return;

            mats.GetArrayElementAtIndex(index).objectReferenceValue = mat;

            so.ApplyModifiedProperties();
        }

        void CleanupEditor()
        {
            if (matEditor != null)
            {
                DestroyImmediate(matEditor);
                matEditor = null;
                trackedMat = null;
            }
        }
        
        void DrawInspector(BoltsMaterialInstance bm)
        {
            SerializedObject so = serializedObject;
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("targetRenderer"));

            SerializedProperty indexProp = so.FindProperty("materialIndex");
            SerializedProperty matProp = so.FindProperty("mat");
            
            if (bm.targetRenderer != null)
            {
                int count = bm.targetRenderer.sharedMaterials.Length;
                if (count <= 1)
                {
                    indexProp.intValue = 0;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.IntField("Material Index", 0);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("MaterialIndex");
                    
                    EditorGUI.BeginDisabledGroup(indexProp.intValue <= 0);
                    if (GUILayout.Button("◀", EditorStyles.miniButtonLeft, GUILayout.Width(28)))
                    {
                        PushToRenderer(bm, bm.mat, bm.materialIndex);
                        
                        indexProp.intValue--;
                        so.ApplyModifiedProperties();

                        matProp.objectReferenceValue = originalMaterials[indexProp.intValue];
                        so.ApplyModifiedProperties();

                        bm.materialIndex = indexProp.intValue;
                        ApplyInstance(bm);
                        EditorUtility.SetDirty(bm);
                    }
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.LabelField(
                        indexProp.intValue.ToString(),
                        EditorStyles.centeredGreyMiniLabel,
                        GUILayout.Width(28));
                    
                    EditorGUI.BeginDisabledGroup(indexProp.intValue >= count -1);
                    if (GUILayout.Button("▶", EditorStyles.miniButtonRight, GUILayout.Width(28)))
                    {
                        PushToRenderer(bm, bm.mat, bm.materialIndex);
                        
                        indexProp.intValue++;
                        so.ApplyModifiedProperties();

                        matProp.objectReferenceValue = originalMaterials[indexProp.intValue];
                        so.ApplyModifiedProperties();

                        bm.materialIndex = indexProp.intValue;
                        ApplyInstance(bm);
                        EditorUtility.SetDirty(bm);
                    }
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
                EditorGUILayout.PropertyField(indexProp);
            
            EditorGUILayout.PropertyField(matProp);

            so.ApplyModifiedProperties();
        }

        void OnDisable() => CleanupEditor();
        void OnDestroy() => CleanupEditor();
    }
    
    public abstract class NonWindowTools
    {
        [MenuItem("Tools/Bolts Tools/Documentation #&d")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/Bolt-Bug/Boolts-Tools");
        }

        [MenuItem("Tools/Bolts Tools/Open Save Folder #&o")]
        public static void OpenSaveFolder()
        {
            string path = Path.GetDirectoryName(BoltsSave._settings.GetFullPath());
            Application.OpenURL(path);
        }
    }
}
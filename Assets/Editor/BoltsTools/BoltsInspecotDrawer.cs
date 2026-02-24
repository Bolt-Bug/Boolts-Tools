using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
            EditorGUI.HelpBox(position, $"BoltsInputActionAttribute '{attr.actionAssetField}' not found", MessageType.Warning);

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

        int index = Mathf.Max(0, System.Array.IndexOf(mapNames, property.stringValue));
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

        if (mat == null || mat.shader == null)
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
        int lastDot = path.LastIndexOf(".");

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
        List<string> names = GetVariableNames(bsa.filterType);

        EditorGUI.BeginProperty(position, label, property);

        Rect labelRect = new(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect buttonRect = new(position.x + EditorGUIUtility.labelWidth, position.y,
            position.width - EditorGUIUtility.labelWidth, position.height);
        
        EditorGUI.LabelField(labelRect, label);

        string current = property.stringValue;
        string display = string.IsNullOrEmpty(current) ? "-- None --" : current;

        if (EditorGUI.DropdownButton(buttonRect, new (display), FocusType.Keyboard))
        {
            GenericMenu menu = new();
            
            if(names.Count == 0)
                menu.AddDisabledItem(new ("No Saved Variables Found"));
            else
            {
                menu.AddItem(new ("-- None --"), string.IsNullOrEmpty(current), () =>
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

    List<string> GetVariableNames(SavedVariableType filter)
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

        string fullPath = settings.GetFullPath();

        if (!File.Exists(fullPath))
            return names;

        string json = File.ReadAllText(fullPath);
        SaveData sd = JsonUtility.FromJson<SaveData>(json);

        if (sd == null)
            return names;
        
        if((filter == SavedVariableType.Any || filter == SavedVariableType.Float) && sd.floats != null)
            foreach(var item in sd.floats)
                names.Add(item.name);
        
        if((filter == SavedVariableType.Any || filter == SavedVariableType.Int) && sd.ints != null)
            foreach(var item in sd.ints)
                names.Add(item.name);
        
        if((filter == SavedVariableType.Any || filter == SavedVariableType.Bool) && sd.bools != null)
            foreach(var item in sd.bools)
                names.Add(item.name);
        
        if((filter == SavedVariableType.Any || filter == SavedVariableType.String) && sd.strings != null)
            foreach(var item in sd.strings)
                names.Add(item.name);
        
        if((filter == SavedVariableType.Any || filter == SavedVariableType.Class) && sd.classes != null)
            foreach (var item in sd.classes)
                names.Add(item.name);

        return names;
    }
}

[CustomEditor(typeof(BoltsBoxCollider))]
public class BoltsBoxColliderDrawer : Editor
{
    private Editor boxColliderEditor;

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
        
        if(GUILayout.Button("Set Bounds"))
            customBC.SetBounds();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Box Collider Settings", EditorStyles.boldLabel);

        if (customBC.boxCollider != null)
        {
            if (boxColliderEditor == null)
                boxColliderEditor = Editor.CreateEditor(customBC.boxCollider);

            boxColliderEditor.DrawDefaultInspector();
        }
    }

    private void OnDisable()
    {
        if(boxColliderEditor != null)
            DestroyImmediate(boxColliderEditor);
    }
}

public class OpenDocuments
{
    [MenuItem("Tools/Bolts Tools/Documentation")]
    public static void OpenURL()
    {
        Application.OpenURL("https://github.com/Bolt-Bug/Boolts-Tools");
    }
}
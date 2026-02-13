using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomPropertyDrawer(typeof(BoltsEvent))]
public class BoltsEventDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect backgroundRect = new Rect(position.x - 2, position.y, position.width * 4, GetPropertyHeight(property, label));
        EditorGUI.DrawRect(backgroundRect, new Color(0.2f, 0.2f, 0.2f, 0.9f));

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty listenersProp = property.FindPropertyRelative("persistentListeners");

            float yOffset = position.y + EditorGUIUtility.singleLineHeight + 2;

            //Draw the array size and + button
            Rect headerRect = new Rect(position.x, yOffset, position.width - 60, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, "Persistent Listeners");

            Rect addRect = new Rect(position.x + position.width - 55, yOffset, 25, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+"))
                listenersProp.arraySize++;

            Rect removeRect = new Rect(position.x + position.width - 25, yOffset, 25, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(removeRect, "-") && listenersProp.arraySize > 0)
                listenersProp.arraySize--;

            yOffset += EditorGUIUtility.singleLineHeight + 4;

            //Draw each listener
            for (int i = 0; i < listenersProp.arraySize; i++)
            {
                SerializedProperty listenerProp = listenersProp.GetArrayElementAtIndex(i);
                SerializedProperty gameObjectProp = listenerProp.FindPropertyRelative("targetGameObject");
                SerializedProperty componentProp = listenerProp.FindPropertyRelative("targetComponent");
                SerializedProperty methodProp = listenerProp.FindPropertyRelative("methodName");
                SerializedProperty paramsProp = listenerProp.FindPropertyRelative("parameters");

                float listenerStartY = yOffset;
                float listenerHeight = EditorGUIUtility.singleLineHeight + 2;

                if (gameObjectProp.objectReferenceValue != null)
                {
                    listenerHeight += EditorGUIUtility.singleLineHeight + 2;

                    var TargetObject = componentProp.objectReferenceValue != null ?
                                           componentProp.objectReferenceValue :
                                           gameObjectProp.objectReferenceValue;

                    if (TargetObject != null)
                    {
                        listenerHeight += EditorGUIUtility.singleLineHeight + 2;
                        listenerHeight += (EditorGUIUtility.singleLineHeight + 2) * paramsProp.arraySize;
                    }
                }

                Rect listenerBackgroundRect = new Rect(position.x + 5, listenerStartY - 2, position.width - 10, listenerHeight);
                EditorGUI.DrawRect(listenerBackgroundRect, new Color(0.15f, 0.15f, 0.15f, 1f));

                Rect gameObjectRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(gameObjectRect, gameObjectProp, new GUIContent($"GameObject {i}"));
                yOffset += EditorGUIUtility.singleLineHeight + 2;

                if (gameObjectProp.objectReferenceValue != null)
                {
                    GameObject targetGO = gameObjectProp.objectReferenceValue as GameObject;
                    var components = targetGO.GetComponents<Component>();
                    var componentsName = components.Select(c => c.GetType().Name).ToArray();

                    int selectedComponentIndex = -1;

                    if (componentProp.objectReferenceValue != null)
                    {
                        for (int c = 0; c < components.Length; c++)
                        {
                            if (components[c] == componentProp.objectReferenceValue)
                            {
                                selectedComponentIndex = c;
                                break;
                            }
                        }
                    }

                    Rect componentRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    int newComponentIndex = EditorGUI.Popup(componentRect, "Component", selectedComponentIndex, componentsName);

                    if (newComponentIndex >= 0 && newComponentIndex < components.Length)
                        componentProp.objectReferenceValue = components[newComponentIndex];

                    yOffset += EditorGUIUtility.singleLineHeight + 2;
                }

                var targetObject = componentProp.objectReferenceValue != null ?
                                       componentProp.objectReferenceValue :
                                        gameObjectProp.objectReferenceValue;

                //Draw method dropdown
                if(targetObject != null)
                {
                    var methodInfos = GetMethods(targetObject);
                    var methodNames = methodInfos.Select(m => GetMethodDisplayName(m)).ToArray();

                    int selectedIndex = -1;

                    for (int j = 0; j < methodInfos.Count; j++)
                    {
                        if (methodInfos[j].Name == methodProp.stringValue)
                        {
                            selectedIndex = j;
                            break;
                        }
                    }

                    Rect methodRect = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    int newIndex = EditorGUI.Popup(methodRect, "Method", selectedIndex, methodNames );

                    if (newIndex >= 0 && newIndex < methodInfos.Count)
                    {
                        var selectedMethod = methodInfos[newIndex];
                        methodProp.stringValue = selectedMethod.Name;

                        var methodParams = selectedMethod.GetParameters();
                        paramsProp.arraySize = methodParams.Length;

                        selectedIndex = newIndex;

                        for (int p = 0; p < methodParams.Length; p++)
                        {
                            SerializedProperty paramProp = paramsProp.GetArrayElementAtIndex(p);
                            SerializedProperty typeProp = paramProp.FindPropertyRelative("type");

                            Type paramType = methodParams[p].ParameterType;
                            if (paramType == typeof(int))
                                typeProp.enumValueIndex = (int)BoltsEventParameter.ParameterType.Int;
                            else if (paramType == typeof(float))
                                typeProp.enumValueIndex = (int)BoltsEventParameter.ParameterType.Float;
                            else if (paramType == typeof(string))
                                typeProp.enumValueIndex = (int)BoltsEventParameter.ParameterType.String;
                            else if (paramType == typeof(bool))
                                typeProp.enumValueIndex = (int)BoltsEventParameter.ParameterType.Bool;
                            else if (typeof(UnityEngine.Object).IsAssignableFrom(paramType))
                                typeProp.enumValueIndex = (int)BoltsEventParameter.ParameterType.Object;
                        }
                    }

                    yOffset += EditorGUIUtility.singleLineHeight + 2;

                    for (int p = 0; p < paramsProp.arraySize; p++)
                    {
                        SerializedProperty paramProp = paramsProp.GetArrayElementAtIndex(p);
                        SerializedProperty typeProp = paramProp.FindPropertyRelative("type");
                        BoltsEventParameter.ParameterType paramType = (BoltsEventParameter.ParameterType)typeProp.enumValueIndex;

                        string paramName =
                            selectedIndex >= 0 && selectedIndex < methodInfos.Count ?
                                methodInfos[selectedIndex].GetParameters()[p].Name : $"Parm {p}";

                        Rect paramRect = new Rect(position.x + 15, yOffset, position.width - 15, EditorGUIUtility.singleLineHeight);

                        switch (paramType)
                        {
                            case BoltsEventParameter.ParameterType.Int:
                                SerializedProperty intProp = paramProp.FindPropertyRelative("intValue");
                                EditorGUI.PropertyField(paramRect, intProp, new GUIContent(paramName));

                                break;
                            case BoltsEventParameter.ParameterType.Float:
                                SerializedProperty floatProp = paramProp.FindPropertyRelative("floatValue");
                                EditorGUI.PropertyField(paramRect, floatProp, new GUIContent(paramName));

                                break;
                            case BoltsEventParameter.ParameterType.String:
                                SerializedProperty stringProp = paramProp.FindPropertyRelative("stringValue");
                                EditorGUI.PropertyField(paramRect, stringProp, new GUIContent(paramName));

                                break;
                            case BoltsEventParameter.ParameterType.Bool:
                                SerializedProperty boolProp = paramProp.FindPropertyRelative("boolValue");
                                EditorGUI.PropertyField(paramRect, boolProp, new GUIContent(paramName));

                                break;
                            case BoltsEventParameter.ParameterType.Object:
                                SerializedProperty objProp = paramProp.FindPropertyRelative("objectValue");
                                EditorGUI.PropertyField(paramRect, objProp, new GUIContent(paramName));

                                break;
                        }

                        yOffset += EditorGUIUtility.singleLineHeight + 2;
                    }
                }

                yOffset += 14;
            }

            BoltsEvent boltsEvent = fieldInfo.GetValue(property.serializedObject.targetObject) as BoltsEvent;
            int runtimeCount = boltsEvent != null ? boltsEvent.GetRuntimeListenerCount() : 0;

            //Display listener count
            Rect countRect = new Rect(position.x, yOffset, position.width - 110, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(countRect, "Runtime Listeners", $"{runtimeCount} listener(s)");

            Rect clearRuntimeListenerButton = new Rect(position.x + position.width - 150, yOffset, 150, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(clearRuntimeListenerButton, "Reset Runtime Listeners"))
            {
                if (boltsEvent != null)
                {
                    boltsEvent.RemoveAllListeners();
                    listenersProp.ClearArray();
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        SerializedProperty listenersProp = property.FindPropertyRelative("persistentListeners");

        float height = EditorGUIUtility.singleLineHeight + 2;
        height += EditorGUIUtility.singleLineHeight + 4;

        for (int i = 0; i < listenersProp.arraySize; i++)
        {
            SerializedProperty listenerProp = listenersProp.GetArrayElementAtIndex(i);
            SerializedProperty gameObjectProp = listenerProp.FindPropertyRelative("targetGameObject");
            SerializedProperty componentProp = listenerProp.FindPropertyRelative("targetComponent");
            SerializedProperty paramsProp = listenerProp.FindPropertyRelative("parameters");

            height += EditorGUIUtility.singleLineHeight + 2;

            if (gameObjectProp.objectReferenceValue != null)
            {
                height += EditorGUIUtility.singleLineHeight + 2;
                height += (EditorGUIUtility.singleLineHeight + 2) * paramsProp.arraySize;

                var targetProp = componentProp.objectReferenceValue != null ?
                                     componentProp.objectReferenceValue :
                                     gameObjectProp.objectReferenceValue;

                if (targetProp != null)
                {
                    height += EditorGUIUtility.singleLineHeight + 2;
                    height += (EditorGUIUtility.singleLineHeight + 2) * paramsProp.arraySize;
                }
            }

            height += 4;
        }

        height += EditorGUIUtility.singleLineHeight;

        return height + 30;
    }

    private List<MethodInfo> GetMethods(UnityEngine.Object target)
    {
        if (target == null)
            return new List<MethodInfo>();

        var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        List<MethodInfo> validMethods = new List<MethodInfo>();

        foreach (var method in methods)
        {
            if (method.ReturnType == typeof(void) &&
                !method.Name.StartsWith("get_") &&
                !method.Name.StartsWith("set_") &&
                method.DeclaringType != typeof(UnityEngine.Object) &&
                method.DeclaringType != typeof(UnityEngine.Component) &&
                method.DeclaringType != typeof(UnityEngine.Behaviour) &&
                method.DeclaringType != typeof(UnityEngine.MonoBehaviour))
            {
                // Only allow supported parameter types
                bool validParams = true;

                foreach (var param in method.GetParameters())
                {
                    Type pType = param.ParameterType;

                    if (pType != typeof(int) &&
                        pType != typeof(float) &&
                        pType != typeof(string) &&
                        pType != typeof(bool) &&
                        !typeof(UnityEngine.Object).IsAssignableFrom(pType))
                    {
                        validParams = false;

                        break;
                    }
                }

                if (validParams)
                {
                    validMethods.Add(method);
                }
            }
        }

        return validMethods;
    }

    private string GetMethodDisplayName(MethodInfo method)
    {
        var parameters = method.GetParameters();

        if (parameters.Length == 0)
            return method.Name + " ()";

        string paramStr = String.Join(", ", parameters.Select(p => p.ParameterType.Name));

        return $"{method.Name} ({paramStr})";
    }
}

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

public class OpenDocuments
{
    [MenuItem("Tools/Bolts Tools/Documentation")]
    public static void OpenURL()
    {
        Application.OpenURL("https://docs.google.com/document/d/1xaQ9wJ4AUBwIX4THLpGPQA2GjIwiSZUjYudO5D1CTm8/edit?usp=sharing");
    }
}
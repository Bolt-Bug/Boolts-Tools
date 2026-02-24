using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class SpawnGrid : EditorWindow
{
    private Vector3Int gridSize = Vector3Int.one;
    private Vector3 startPos;
    private Vector3 posOffset;
    private Vector3 rotation;

    private UnityEngine.Object objectToSpawn;
    private UnityEngine.Object parent;
    private GameObject previewPos;
    
    private bool spawnList;
    private bool spawnRandom;
    private bool showPreviewPos = true;
    private int seed;
    
    private GameObjectList list;
    private SerializedObject serializedData;
    private SerializedProperty objectsProperty;
    
    // Not Seen In GUI
    private bool showedList;
    private string path = "Assets/Editor/BoltsTools/ObjectList.asset";
    private bool showPreview;
    private List<GameObject> previewList = new();

    [MenuItem("Tools/Bolts Tools/Small Tools/Grid Spawner")]
    static void SetWindow()
    {
        SpawnGrid window = GetWindow<SpawnGrid>(false, "Grid Spawner", true);
        
        window.minSize = new(400, 400);
        window.maxSize = new(400, 1000);
    }
    
    private void OnGUI()
    {
        spawnList = EditorGUILayout.Toggle("Spawn Multiple", spawnList);

        if (spawnList)
        {
            ShowList();
            
            spawnRandom = EditorGUILayout.Toggle("Spawn In Random Order", spawnRandom);
            if (spawnRandom)
            {
                seed = EditorGUILayout.IntField(new GUIContent("Seed", "If Left At 0, Will Be Random"), seed);
            }
        }
        else
            objectToSpawn = EditorGUILayout.ObjectField("Prefab", objectToSpawn, typeof(UnityEngine.Object), false);

        parent = EditorGUILayout.ObjectField(new GUIContent("Parent","Can Be Left Empty"), parent, typeof(UnityEngine.Object), true);
        
        gridSize = EditorGUILayout.Vector3IntField("Grid Size", gridSize);
        startPos = EditorGUILayout.Vector3Field("Start Position", startPos);
        rotation = EditorGUILayout.Vector3Field("Rotation", rotation);
        posOffset = EditorGUILayout.Vector3Field("Offset", posOffset);

        if (showPreviewPos)
        {
            if (GUILayout.Button("Hide Preview Position"))
                showPreviewPos = false;
        }
        else
        {
            if (GUILayout.Button("Show Preview Position"))
                showPreviewPos = true;
        }
        
        if(showPreview)
        {
            if (GUILayout.Button("Remove Preview"))
                RemovePreview();
        }
        else
        {
            if (GUILayout.Button("Preview"))
                ShowPreview();
        }

        if (GUILayout.Button("Spawn Grid"))
        {
            if(showPreview)
                RemovePreview();
            
            Spawn();
        }
    }

    void Spawn()
    {
        SpawnTheGrid();
    }

    void ShowPreview()
    {
        showPreview = true;
        SpawnTheGrid(true);
    }

    void RemovePreview()
    {
        foreach (var t in previewList)
        {
            DestroyImmediate(t);
        }
        
        previewList.Clear();

        showPreview = false;
    }

    void SpawnTheGrid(bool addToList = false)
    {
        Undo.SetCurrentGroupName("Grid Spawn");
        int undoGroup = Undo.GetCurrentGroup();
        
        int sizeX = Mathf.Max(1, gridSize.x);
        int sizeY = Mathf.Max(1, gridSize.y);
        int sizeZ = Mathf.Max(1, gridSize.z);

        int listIndex = 0;
        
        for(int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Vector3 posToSpawn = new Vector3(startPos.x + x * posOffset.x, startPos.y + y * posOffset.y,
                        startPos.z + z * posOffset.z);

                    UnityEngine.Object objToSpawn;

                    if (spawnList)
                    {
                        if (seed == 0)
                            seed = Random.Range(1, 9999);
                        Random.InitState(seed);

                        if (spawnRandom)
                            listIndex = Random.Range(0, list.gameObjects.Count);

                        objToSpawn = list.gameObjects[listIndex];

                        listIndex = (listIndex + 1) % list.gameObjects.Count;
                    }
                    else
                        objToSpawn = objectToSpawn;

                    GameObject spawnedObj = PrefabUtility.InstantiatePrefab(objToSpawn).GameObject();
                    
                    spawnedObj.transform.position = posToSpawn;
                    spawnedObj.transform.rotation = Quaternion.Euler(rotation);

                    if (parent != null)
                        spawnedObj.transform.parent = parent.GameObject().transform;

                    Undo.RegisterCreatedObjectUndo(spawnedObj, "Grid Placement");
                    
                    if (addToList)
                        previewList.Add(spawnedObj);
                }
            }
        }
        
        Undo.CollapseUndoOperations(undoGroup);
    }
    
    void ShowList()
    {
        if (!showedList)
        {
            GameObjectList makeList = CreateInstance<GameObjectList>();
            
            AssetDatabase.CreateAsset(makeList, path);

            list = AssetDatabase.LoadAssetAtPath<GameObjectList>(path);

            if (list != null)
            {
                serializedData = new SerializedObject(list);
                objectsProperty = serializedData.FindProperty("gameObjects");
            }

            showedList = true;
        }
        
        serializedData.Update();

        EditorGUILayout.PropertyField(objectsProperty, true);
        serializedData.ApplyModifiedProperties();
    }

    private void Update()
    {
        if (showPreviewPos)
        {
            if (previewPos == null)
                previewPos = new GameObject();
            else
            {
                previewPos.transform.position = startPos;
                previewPos.name = "Preview";
                
                var iconContent = EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo");
                EditorGUIUtility.SetIconForObject(previewPos, (Texture2D)iconContent.image);
            }
        }
        else
        {
            if (previewPos != null)
                DestroyImmediate(previewPos);
            
        }
    }

    private void OnDestroy()
    {
        if (AssetDatabase.AssetPathExists(path))
            AssetDatabase.DeleteAsset(path);
        
        if(previewPos != null)
            DestroyImmediate(previewPos);
        
        RemovePreview();
    }
}

public class GameObjectList : ScriptableObject
{
    public List<UnityEngine.Object> gameObjects;
}

public class SelectAllWithComponentOrName : EditorWindow
{
    List<Type> components = new List<Type>();
    string[] componentsName = new string[0];
    int selectedIndex;

    bool selectName;
    string objectName = "";

    [MenuItem("Tools/Bolts Tools/Small Tools/Select All Objects With Component")]
    public static void OpenWidow()
    {
        GetWindow(typeof(SelectAllWithComponentOrName));
    }

    private void OnEnable()
    {
        RefreshComponentList();
        EditorApplication.hierarchyChanged += RefreshComponentList;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= RefreshComponentList;
    }

    private void OnGUI()
    {
        selectName = EditorGUILayout.Toggle("Select With Name", selectName);

        EditorGUILayout.Space();
        if(components.Count == 0)
        {
            EditorGUILayout.HelpBox("No Components Found", MessageType.Error);
            if (GUILayout.Button("Re-scan Scene")) RefreshComponentList();
            return;
        }

        if (selectName)
        {
            objectName = EditorGUILayout.TextField("Name", objectName);

            if (GUILayout.Button("Select All Objects"))
                SelectAllWithName();
        }
        else
        {
            selectedIndex = EditorGUILayout.Popup("Component Type", selectedIndex, componentsName);

            if (GUILayout.Button("Select All Objects"))
                SelectAllWith(components[selectedIndex]);

            if (GUILayout.Button("Re-scan Scene"))
                RefreshComponentList();
        }
    }

    public void RefreshComponentList()
    {
        components.Clear();

        var roots = SceneManager.GetActiveScene().GetRootGameObjects();

        var allComps = roots
            .SelectMany(r => r.GetComponentsInChildren<Component>(true))
            .Where(c => c != null);

        components = allComps
            .Select(c => c.GetType())
            .Where(t => t != typeof(Transform))
            .Distinct()
            .OrderBy(t => t.Name)
            .ToList();

        componentsName = components
            .Select(t => t.Name)
            .ToArray();

        selectedIndex = 0;
    }

    public void SelectAllWith(Type type)
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var comps = roots
            .SelectMany(r => r.GetComponentsInChildren(type, true))
            .Cast<Component>();

        var gos = comps
            .Select(c => c.gameObject)
            .Distinct()
            .ToArray();

        Selection.objects = gos;
    }

    void SelectAllWithName()
    {
        var allGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        var getAllNames = allGameObjects
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
            .Select(t => t.gameObject);

        var matches = getAllNames
            .Where(go => go.name.ToLower().Contains(objectName.ToLower()))
            .Distinct()
            .ToArray();

        Selection.objects = matches;
    }
}

public class TakeScreenshot : EditorWindow
{
    private string path = "Assets";
    private string fileName = "image";

    [MenuItem("Tools/Bolts Tools/Small Tools/Take Screenshot")]
    static void OpenWindow()
    {
        GetWindow<TakeScreenshot>();
    }

    private void OnGUI()
    {
        fileName = EditorGUILayout.TextField("File Name", fileName);
        
        EditorGUILayout.LabelField(path);
        if (GUILayout.Button("Set Path"))
            path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");

        if (GUILayout.Button("Take Screenshot"))
        {
            string finalFileName = "";
            if (fileName.Contains("."))
            {
                string[] fileNameArray = fileName.Split(".");
                finalFileName = fileNameArray[0];
            }
            else
                finalFileName = fileName;

            string finalPath = Path.Combine(path, finalFileName + ".png");
            
            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            
            FocusWindowIfItsOpen(gameViewType);
            
            ScreenCapture.CaptureScreenshot(finalPath);
        }
    }
}

public class BatchRenamer : EditorWindow
{
    private string objNames = "Name";
    private bool addIndex;

    private List<Object> selectedObj = new();
    
    [MenuItem("Tools/Bolts Tools/Small Tools/BatchRename")]
    public static void OpenWindow()
    {
        GetWindow<BatchRenamer>();
    }

    private void OnGUI()
    {
        objNames = EditorGUILayout.TextField("Objects Names", objNames);

        string tooltip = "Will Add ObjName 1, ObjName 2, etc";
        addIndex = EditorGUILayout.Toggle(new GUIContent("Add Index To Name", tooltip), addIndex);

        if (GUILayout.Button("Rename All Objects"))
        {
            selectedObj = Selection.objects.ToList();

            Undo.SetCurrentGroupName("Renamed Objects");
            int undoGroup = Undo.GetCurrentGroup();
            
            for (int i = selectedObj.Count - 1; i > -1; i--)
            {
                string finalName = objNames + (addIndex && i > 0 ? $" ({i})" : "");
                string oldName = selectedObj[i].GameObject().name;
                selectedObj[i].GameObject().name = finalName;
                
                Undo.RegisterCompleteObjectUndo(selectedObj[i], "Renamed " + oldName);
            }
            
            Undo.CollapseUndoOperations(undoGroup);
        }
    }
}

public class MakeScriptableObject : EditorWindow
{
    private List<Type> soTypes = new();
    private string[] typesNames;
    private int selectedIndex;
    private string assetName;
    
    [MenuItem("Tools/Bolts Tools/Small Tools/ Make SO")]
    public static void OpenWindow()
    {
        GetWindow<MakeScriptableObject>();
    }

    private void OnGUI()
    {
        if (soTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("No ScriptableObject Types Found In Project", MessageType.Info);
            if(GUILayout.Button("Refresh")) RefreshTypes();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Type", GUILayout.Width(40));
        selectedIndex = EditorGUILayout.Popup(selectedIndex, typesNames);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();

        assetName = EditorGUILayout.TextField("Asset Name", assetName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Scriptable Object", GUILayout.Height(30)))
            CreateAsset(soTypes[selectedIndex]);
        
        EditorGUILayout.Space();
        
        if(GUILayout.Button("Refresh Types"))
            RefreshTypes();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox($"{soTypes.Count} Types Found.", MessageType.None);
    }

    void RefreshTypes()
    {
        soTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t =>
                t.IsSubclassOf(typeof(ScriptableObject))
                && !t.IsAbstract
                && !t.Namespace?.StartsWith("Unity") == true
                && !t.Namespace?.StartsWith("TMPro") == true
                && !t.Namespace?.StartsWith("SavingConfigAsset") == true)
            .OrderBy(t => t.Name)
            .ToList();

        typesNames = soTypes.Select(t => t.FullName.Replace(".", "/")).ToArray();

        if (selectedIndex >= soTypes.Count)
            selectedIndex = 0;
    }

    void CreateAsset(Type type)
    {
        var asset = CreateInstance(type);

        string path = EditorUtility.SaveFilePanelInProject("Save Scriptable Object",
            assetName,
            "asset",
            "Choose where to save the asset"
        );
        
        if(string.IsNullOrEmpty(path)) return;
        
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        Debug.Log($"Created {type.Name} At {path}");
    }
}
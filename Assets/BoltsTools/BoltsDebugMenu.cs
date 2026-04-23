using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BoltsTools
{
    public class BoltsDebugMenu : MonoBehaviour
    {
        public static BoltsDebugMenu Instance;
        
        float frames = 0, time = 0;

        bool showDebug;

        static readonly List<DebugText> textToShow = new();
        static readonly List<DebugButton> buttonsToShow = new();

        public KeyCode keyToOpenDebug = KeyCode.F3;
        
        public bool showFPS, showPlayerPos;
        
        public string playerTag = "Player";
        
        public Transform player;
        
        void OnGUI()
        {
            if(!showDebug) return;

            if (showPlayerPos && player != null)
            {
                if (player == null)
                {
                    Debug.LogError("Player Not Assigned!!!");
                    return;
                }
                
                Vector3 playerPos = player.position;
                
                GUIStyle style = new GUIStyle();
                
                style.font = Font.CreateDynamicFontFromOSFont("Courier New", 25);

                string text = string.Format("XYZ: X:{0,-8:F2}  Y:{1,-8:F2}  Z:{2,-8:F2}", playerPos.x, playerPos.y,
                    playerPos.z);
                
                float xPos = Screen.width - 600 - 100;
                style.alignment = TextAnchor.MiddleRight;
                
                Rect playerPosRect = new(xPos, 100, 600, 200);
                GUI.TextArea(playerPosRect, text, style);
            }
            
            if (showFPS)
            {
                Rect fpsRect = new(50, 50, 225,75);
                GUI.TextArea(fpsRect, $"<size=50>FPS: {frames}", new GUIStyle(){alignment = TextAnchor.MiddleLeft});
            }

            for (int i = 0; i < textToShow.Count; i++)
            {
                DebugText currentText = textToShow[i];
                if (currentText.size.y == 0)
                    currentText.size.y = 100 * (i + 1);
                
                GUI.Box(currentText.size, currentText.value);
            }

            for (int i = 0; i < buttonsToShow.Count; i++)
            {
                DebugButton currentButton = buttonsToShow[i];

                if (currentButton.size.y == 0)
                    currentButton.size.y = 100 * (i + 1);
                
                float newXPos = Screen.width - currentButton.size.width - currentButton.size.x;
                Rect newRect = currentButton.size;
                newRect.x = newXPos;
                
                GUILayout.BeginArea(newRect);
                if (GUILayout.Button(currentButton.value)) currentButton.onClick.Invoke();
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Add Text To The Debug Screen
        /// </summary>
        /// <param name="name">The Name Of The Text</param>
        /// <param name="value">What The Text Should Say</param>
        /// <param name="size">The Size And Position. Can Be Left Empty></param>
        public static void BoltsDebugAddText(string name, string value, Rect size = new Rect())
        {
            int index = -1;
            if (textToShow.Count > 0)
                index = textToShow.FindIndex(x => x.textName == name);

            Rect theSize = new Rect(0, 0, 100, 100);
            if (size.x > 0 || size.y > 0)
                theSize = size;
            
            if (index > -1)
                textToShow[index].value = value;
            else
                textToShow.Add(new(){textName = name, value = value, size = theSize});
        }

        
        /// <summary>
        /// Add A Button To The Debug Screen
        /// </summary>
        /// <param name="name">The Name Of The Button</param>
        /// <param name="value">What The Button Should Say</param>
        /// <param name="onClick">What Actions To Do When Clicked</param>
        /// <param name="size">The Size And Position. Can Be Left Empty></param>
        /// <example>BoltsDebugAddButton("Teleport", "Teleport The Player", TeleportPlayer(), new Rect(100, 100, 100, 100))</example>
        public static void BoltsDebugAddButton(string name, string value, Action onClick, Rect size = new Rect())
        {
            int index = -1;
            if (buttonsToShow.Count > 0)
                index = textToShow.FindIndex(x => x.textName == name);

            Rect theSize = new Rect(0, 0, 100, 100);
            if (size.x > 0 || size.y > 0)
                theSize = size;

            if (index > -1)
            {
                buttonsToShow[index].value = value;
                buttonsToShow[index].onClick = onClick;
            }
            else
                buttonsToShow.Add(new(){textName = name, value = value, onClick = onClick, size = theSize});
        }
        
        /// <summary>
        /// Add A Button To The Debug Screen
        /// </summary>
        /// <param name="name">The Name Of The Text To Remove</param>
        /// <example>BoltsDebugRemoveText("playerSpeed)</example>
        public static void BoltsDebugRemoveText(string name)
        {
            int index = -1;
            index = textToShow.FindIndex(x => x.textName == name);
            
            if(index > -1)
                textToShow.RemoveAt(index);
            else
                Debug.LogError($"Could Not Find Debug Text Named {name}");
        }

        /// <summary>
        /// Add A Button To The Debug Screen
        /// </summary>
        /// <param name="name">The Name Of The Button To Remove</param>
        /// <example>BoltsDebugRemoveButton("killPlayer")</example>
        public static void BoltsDebugRemoveButton(string name)
        {
            int index = -1;
            index = buttonsToShow.FindIndex(x => x.textName == name);
            
            if(index > -1)
                buttonsToShow.RemoveAt(index);
            else
                Debug.LogError($"Could Not Find Debug Button Named {name}");
        }
        
        void Update()
        {
            frames = (float)Decimal.Round((decimal)(1 / Time.unscaledDeltaTime));
            time += Time.unscaledDeltaTime;
            if (time >= 1)
            {
                frames = 0;
                time = 0;
            }

            if (Input.GetKeyDown(keyToOpenDebug))
                showDebug = !showDebug;

            if (player == null && LoadBoltsDebugMenu._settings.showPlayerPos)
                player = GameObject.FindGameObjectWithTag(playerTag).transform;
        }

        void Awake()
        {
            LoadBoltsDebugMenu.Initialize();
            
            if (Instance == null)
                Instance = this;
            else if(Instance != this)
                Destroy(gameObject);
        }

        void Reset()
        {
            LoadBoltsDebugMenu.Initialize();
            
            if (Instance == null)
                Instance = this;
            else if(Instance != this)
                Destroy(gameObject);
            
            if (LoadBoltsDebugMenu._settings != null)
            {
                showFPS = LoadBoltsDebugMenu._settings.showFPS;
                showPlayerPos = LoadBoltsDebugMenu._settings.showPlayerPos;
                keyToOpenDebug = LoadBoltsDebugMenu._settings.keyToOpenDebug;
                playerTag = LoadBoltsDebugMenu._settings.playerTag;
            }
        }
    }

    [Serializable]
    class DebugText
    {
        public string textName;
        public string value;

        public Rect size = new Rect(100, 100, 100, 100);
    }

    [Serializable]
    class DebugButton
    {
        public string textName;
        public string value;

        public Rect size = new Rect(100, 100, 100, 100);
        
        public Action onClick;
    }
    
    static class LoadBoltsDebugMenu
    {
        public static BoltsDebugMenuSettings _settings;
        static bool _isLoading;
        
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void InitializeInEditor()
        {
            Initialize();
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if(_settings != null || _isLoading)
                return;

            _isLoading = true;

            _settings = Resources.Load<BoltsDebugMenuSettings>("DebugSettings");
            Debug.Log("Debug Settings Loaded");

            _isLoading = false;
        }

        [MenuItem("GameObject/Bolts Debug Object #t", false, 5)]
        static void CreateOBJ(MenuCommand menuCommand)
        {
            GameObject obj = new GameObject("Bolts Debug");

            obj.AddComponent<BoltsDebugMenu>();
            obj.AddComponent<BoltsCommands>();
            
            GameObjectUtility.SetParentAndAlign(obj,menuCommand.context as GameObject);
            
            Undo.RegisterCreatedObjectUndo(obj, "Create Bots Debug Object");

            Selection.activeObject = obj;
        }
    }
}

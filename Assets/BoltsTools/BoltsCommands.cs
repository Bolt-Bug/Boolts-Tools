using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BoltsTools
{
    public class BoltsCommands : MonoBehaviour
    {
        public static BoltsCommands command;

        static readonly List<Command> commands = new();
        public List<Command> publicCommands = new();

        public KeyCode keyToOpenCommands = KeyCode.F2;
        
        [BoltsToolTip("Shows The Cursor When Typing A Command")]
        public bool unlockCursor = true;

        bool showCommands = true;

        public static bool isTyping;

        int selectedSuggestionIndex;

        string commandTyped = "";
        string lastCommand = "";
        
        List<Command> currentMatches = new();

        bool moveCursorToEnd;
        
        static bool addedCommand;

        void Update()
        {
            if(!showCommands) return;
            
            if (Input.GetKeyDown(keyToOpenCommands) && !isTyping)
            {
                isTyping = true;
                GUI.FocusControl("Command");
            }

            if (isTyping && unlockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (addedCommand)
            {
                publicCommands = commands;
                addedCommand = false;
            }
        }

        /// <summary>
        /// Adds A Command
        /// </summary>
        /// <param name="commandName">The Name For The Command</param>
        /// <param name="methodName">The Name Of The Method The Command Should Run (Don't Add Arguments)</param>
        /// <param name="target">What Script On What Game Object Should Run The Command</param>
        /// <param name="description">The Commands Description (Can Be Left Empty)</param>
        public static void AddCommand(string commandName, string methodName, MonoBehaviour target, string description = "")
        {
            string finalCommandName = commandName.Replace(" ", "");
            string finalName = methodName.Replace(" ", "");

            int index = -1;
            index = commands.FindIndex(x => x.name == finalCommandName);
            if (index > -1)
            {
                MethodInfo method = target.GetType().GetMethod(finalName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    Debug.Log($"No Method Named: {methodName} Found");
                    return;
                }

                commands[index].method = method;
                commands[index].action = null;
                commands[index].description = description;
            }
            else
            {
                MethodInfo method = target.GetType().GetMethod(finalName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    Debug.Log($"No Method Named: {methodName} Found");
                    return;
                }

                commands.Add(new()
                    { name = finalCommandName, method = method, target = target, description = description });
            }

            addedCommand = true;
        }
        /// <summary>
        /// Adds A Command
        /// </summary>
        /// <param name="commandName">The Name For The Command</param>
        /// <param name="action">The Action To Do</param>
        /// <param name="description">The Commands Description (Can Be Left Empty)</param>
        public static void AddCommand(string commandName, Action action, string description = "")
        {
            string finalCommandName = commandName.Replace(" ", "");

            int index = -1;
            index = commands.FindIndex(x => x.name == finalCommandName);
            if (index > -1)
            {
                commands[index].action = action;
                commands[index].method = null;
                commands[index].description = description;
            }
            else
                commands.Add(new() { name = finalCommandName, action = action, description = description });

            addedCommand = true;
        }

        void RunCommand()
        {
            string input = commandTyped.Trim();
            commandTyped = "";
            isTyping = false;

            if (string.IsNullOrWhiteSpace(input))
            {
                Debug.Log("No Command Typed");
                return;
            }

            List<string> tokens = ParseTokens(input);

            if (tokens.Count == 0)
                return;

            string commandName = tokens[0];
            lastCommand = commandName;

            int commandIndex = -1;
            commandIndex = commands.FindIndex(x => x.name == commandName);

            if (commandIndex == -1)
            {
                Debug.LogError($"{commandName} Is An Unknown Command");
                return;
            }

            Command cmd = commands[commandIndex];

            if (cmd.action != null)
            {
                cmd.action?.Invoke();
                return;
            }
            
            ParameterInfo[] parameters = cmd.method.GetParameters();
            List<object> arguments = new();

            for (int i = 0; i < parameters.Length; i++)
            {
                int tokenIndex = i + 1;

                if (tokenIndex >= tokens.Count)
                {
                    Debug.LogWarning(
                        $"Command {commandName} Expected {parameters.Length} Arguments(s) But Recived {i}");
                    return;
                }

                object converted;
                Type paramType = parameters[i].ParameterType;
                
                if (paramType == typeof(Vector3))
                {
                    string[] parts = tokens[tokenIndex].Split(",");
                    if (parts.Length != 3 ||
                        !float.TryParse(parts[0], out float x) ||
                        !float.TryParse(parts[1], out float y) ||
                        !float.TryParse(parts[2], out float z))
                    {
                        Debug.LogWarning($"Could Not Parse '{tokens[tokenIndex]}' As A Vector3. Use Format: 'X,Y,Z'");
                        return;
                    }

                    converted = new Vector3(x, y, z);
                }
                else if (paramType == typeof(Vector2))
                {
                    string[] parts = tokens[tokenIndex].Split(",");
                    if (parts.Length != 2 ||
                        !float.TryParse(parts[0], out float x) ||
                        !float.TryParse(parts[1], out float y))
                    {
                        Debug.LogWarning($"Could Not Parse '{tokens[tokenIndex]}' As A Vector2. Use Format: 'X,Y'");
                        return;
                    }

                    converted = new Vector2(x, y);
                }
                else if (!paramType.IsPrimitive && paramType != typeof(string) && paramType.GetFields().Any(f => f.GetCustomAttributes<CommandArgAttribute>() != null && !f.IsLiteral && !f.IsInitOnly))
                {
                    object instance = Activator.CreateInstance(paramType);

                    FieldInfo[] fields = paramType.GetFields()
                        .Where(f => f.GetCustomAttributes<CommandArgAttribute>().Any() && !f.IsInitOnly &&
                                    !f.IsLiteral)
                        .ToArray();

                    foreach (FieldInfo field in fields)
                    {
                        if (tokenIndex >= tokens.Count)
                        {
                            Debug.LogError($"Not Enough Arguments To Fill All Fields Of {paramType.Name}");
                            return;
                        }

                        try
                        {
                            object fieldValue = Convert.ChangeType(tokens[tokenIndex], field.FieldType);
                            
                            field.SetValue(instance, fieldValue);
                            tokenIndex++;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Could Not Set Field '{field.Name}' From '{tokens[tokenIndex]}': {e.Message}");
                            return;
                        }
                    }

                    converted = instance;
                } 
                else
                {
                    try
                    {
                        converted = Convert.ChangeType(tokens[tokenIndex], parameters[i].ParameterType);
                        tokenIndex++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(
                            $"Could Not Convert Argument '{tokens[tokenIndex]}' To {parameters[i].ParameterType.Name}: {e.Message}");
                        return;
                    }
                }

                arguments.Add(converted);
            }

            cmd.method.Invoke(cmd.target, arguments.ToArray());
        }

        string focusedArea = "Command";
        void OnGUI()
        {
            if (isTyping)
            {
                GUIStyle style = new GUIStyle(GUI.skin.textArea);
                style.fontSize = 50;

                Rect commandRect = new(0, Screen.height - 200, Screen.width, 150);

                GUI.SetNextControlName("Command");
                commandTyped = GUI.TextArea(commandRect, commandTyped, style);
                
                if (moveCursorToEnd)
                {
                    TextEditor editor =
                        (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (editor != null)
                        editor.MoveTextEnd();

                    moveCursorToEnd = false;
                }
                
                if (commandTyped.EndsWith("\n") && commandTyped.Length > 0)
                {
                    focusedArea = "Command";
                    RunCommand();
                }
                
                string currentInput = commandTyped.Trim();
                currentMatches = string.IsNullOrEmpty(currentInput)
                    ? new()
                    : commands.Where(c => c.name.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase)).ToList();
                
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
                {
                    if (currentMatches.Count > 0)
                    {
                        commandTyped = currentMatches[0].name;
                        selectedSuggestionIndex = 0;
                    }
                    else if (lastCommand.Length > 0)
                        focusedArea = focusedArea == "Command" ? "LastCommand" : "Command";
                    
                    Event.current.Use();
                }
                
                GUI.FocusControl(focusedArea);
                
                if (currentMatches.Count > 0)
                {
                    GUIStyle suggestionStyle = new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 40,
                        richText = true
                    };

                    float suggestionHeight = 60f;
                    float totalHeight = suggestionHeight * currentMatches.Count;
                    float yStart = Screen.height - 200 - totalHeight - 10;
                    
                    selectedSuggestionIndex = Mathf.Clamp(selectedSuggestionIndex, 0, currentMatches.Count - 1);

                    for (int i = 0; i < currentMatches.Count; i++)
                    {
                        Rect rect = new Rect(0, yStart + i * suggestionHeight, Screen.width, suggestionHeight);
                        bool isHighlighted = i == selectedSuggestionIndex;

                        string label = isHighlighted
                            ? $"<color=yellow>> {currentMatches[i].name}</color>"
                            : $"  {currentMatches[i].name}";

                        if (!string.IsNullOrEmpty(currentMatches[i].description))
                            label += $"  <color=grey>{currentMatches[i].description}</color>";

                        GUI.Box(rect, label, suggestionStyle);

                        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        {
                            commandTyped = currentMatches[i].name;
                            selectedSuggestionIndex = 0;

                            moveCursorToEnd = true;
                            
                            Event.current.Use();
                        }
                    }
                }
                else
                {
                    selectedSuggestionIndex = 0;

                    if (lastCommand.Length > 0)
                    {
                        float width = style.CalcSize(new GUIContent(lastCommand)).x;
                        float height = style.CalcHeight(new GUIContent(lastCommand), width);

                        GUIStyle lastCommandStyle = new GUIStyle(GUI.skin.box)
                            { alignment = TextAnchor.MiddleLeft, fontSize = 50, richText = true };

                        Rect lastCommandRect = new Rect(0, Screen.height - 210 - height, width, height);
                
                        GUI.SetNextControlName("LastCommand");
                        GUI.TextArea(lastCommandRect,
                            "<color=white>" + lastCommand,
                            lastCommandStyle);
                    }
                }
            }
        }
        
        List<string> ParseTokens(string input)
        {
            List<string> tokens = new();
            string[] words = input.Split(" ");

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].StartsWith("["))
                {
                    string grouped = "";
                    for (int j = i; j < words.Length; j++)
                    {
                        grouped += (j == i ? "" : " ") + words[j];
                        if (words[j].EndsWith("]"))
                        {
                            i = j;
                            break;
                        }
                    }

                    tokens.Add(grouped.Replace("[", "").Replace("]", ""));
                }
                else if (!string.IsNullOrEmpty(words[i]))
                    tokens.Add(words[i]);
            }

            return tokens;
        }

        public void ShowCommands()
        {
            if(commands.Count == 0) 
            {
                Debug.LogError("No Commands Register");
                return;
            }
            
            // Does "int = 1" So That There Is No Empty Spaces
            string allCommands = $"Command: {commands[0].name}    Description: {commands[0].description}";
            for (int i = 1; i < commands.Count; i++)
            {
                allCommands += $"\nCommand: {commands[i].name}";
                allCommands += string.IsNullOrEmpty(commands[i].description)
                    ? commands[i].action == null
                        ? $"    Method: {commands[i].method.Name}"
                        : "    Method: Does An Action"
                    : $"    Description: {commands[i].description}";
            }

            lastCommand = allCommands;

            isTyping = true;
        }

        void Awake()
        {
            command = this;
            
            AddCommand("help", "ShowCommands", this, "Shows All Commands");
        }

        void Reset()
        {
            if (LoadBoltsDebugMenu._settings != null)
            {
                unlockCursor = LoadBoltsDebugMenu._settings.unlockCursor;
                keyToOpenCommands = LoadBoltsDebugMenu._settings.keyToOpenCommands;
                showCommands = LoadBoltsDebugMenu._settings.showCommands;
            }
        }

        [Serializable]
        public class Command
        {
            public string name;
            public string description;
            public MethodInfo method;
            public Action action;
            public object target;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class CommandArgAttribute : Attribute{}
}
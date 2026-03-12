using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace  BoltsTools
{
    public class BoltsCommands : MonoBehaviour
    {
        public static BoltsCommands command;
        
        List<Command> commands = new();

        public bool isTyping;

        string commandTyped = "";
        
        void Update()
        {
            if (LoadBoltsDebugMenu._settings != null &&
                Input.GetKeyDown(LoadBoltsDebugMenu._settings.keyToOpenCommands) && !isTyping)
            {
                isTyping = true;
            }
        }

        /// <summary>
        /// Adds A Command
        /// </summary>
        /// <param name="commandName">The Name For The Command</param>
        /// <param name="methodName">The Name Of The Method The Command Should Run (Dont Add Arguments)</param>
        /// <param name="target">What Script On What Game Object Sould Run The Command</param>
        public void AddCommand(string commandName, string methodName, MonoBehaviour target)
        {
            string finalName = methodName;
            finalName.Replace(" ", "");
            
            int index = -1;
            index = commands.FindIndex(x => x.name == commandName);
             if (index > -1)
             {
                 MethodInfo method = target.GetType().GetMethod(methodName,
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                 if (method == null)
                 {
                     Debug.Log($"No Method Named: {methodName} Found");
                 }
                 
                 commands[index].method = method;
             }
             else
             {
                 MethodInfo method = target.GetType().GetMethod(methodName,
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                 if (method == null)
                 {
                     Debug.Log($"No Method Named: {methodName} Found");
                 }
                 
                 commands.Add(new() { name = commandName, method = method, target = target});
             }
        }

        void RunCommand()
        {
            string[] fullCommand = commandTyped.Split(" ");

            int commandIndex = -1;
            commandIndex = commands.FindIndex(x => x.name == fullCommand[0]);

            if (commandIndex > -1)
            {
                MethodInfo method = commands[commandIndex].method;
                List<object> arguments = new();
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    object convertedArgument =
                        Convert.ChangeType(fullCommand[i + 1], method.GetParameters()[i].ParameterType);
                            
                    arguments.Add(convertedArgument);
                }

                method.Invoke(commands[commandIndex].target, arguments.ToArray());
            }

            commandTyped = "";
            
            isTyping = false;
        }

        void OnGUI()
        {
            if (isTyping)
            {
                GUIStyle style = new GUIStyle(GUI.skin.textArea);
                style.fontSize = 50;
                
                GUI.SetNextControlName("Command");
                commandTyped = GUI.TextArea(new(0, Screen.height - 200, Screen.width, 150), commandTyped, style);

                GUI.FocusControl("Command");
                
                if(commandTyped.EndsWith("\n") && commandTyped.Length > 0)
                    RunCommand();
            }
        }

        void Awake()
        {
            command = this;
        }
    }
    
    public class Command
    {
        public string name;
        public MethodInfo method;
        public object target;
    }
}
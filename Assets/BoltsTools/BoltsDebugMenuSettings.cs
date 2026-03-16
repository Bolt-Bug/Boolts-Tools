using UnityEngine;

namespace BoltsTools
{
    [Icon("Assets/BoltsTools/Sprites/DebugLogo.png")]
    public class BoltsDebugMenuSettings : ScriptableObject
    {
        public KeyCode keyToOpenDebug = KeyCode.F3;

        public bool showFPS, showPlayerPos;

        [HideInInspector]
        public string playerTag = "Player";

        public KeyCode keyToOpenCommands = KeyCode.F2;
    }
}

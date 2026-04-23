using UnityEngine;

namespace BoltsTools
{
    [Icon("Assets/BoltsTools/Sprites/DebugLogo.png")]
    public class BoltsDebugMenuSettings : ScriptableObject
    {
        public KeyCode keyToOpenDebug = KeyCode.F3;

        public bool showFPS, showPlayerPos;
        
        [BoltsToolTip("Shows The Cursor When Typing A Command")]
        public bool unlockCursor = true;

        [HideInInspector]
        public string playerTag = "Player";

        public KeyCode keyToOpenCommands = KeyCode.F2;
    }
}

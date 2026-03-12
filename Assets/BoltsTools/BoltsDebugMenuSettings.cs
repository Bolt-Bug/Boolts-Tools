using UnityEngine;

public class BoltsDebugMenuSettings : ScriptableObject
{
    public KeyCode keyToOpenDebug = KeyCode.F3;

    public bool showFPS, showPlayerPos;

    public string playerTag = "Player";

    public KeyCode keyToOpenCommands = KeyCode.F2;
}

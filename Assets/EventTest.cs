using UnityEngine;
using BoltsTools;

public class EventTest : MonoBehaviour
{
    [BoltsSave(SavedVariableType.String)]
    public string saveString;
}
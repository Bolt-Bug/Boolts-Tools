using UnityEngine;

public class SavingConfigAsset : ScriptableObject
{
    public string fileName = "save.dat";
    public bool usePersistentDataPath = true;
    public bool useEncryption;

    public string path;
}
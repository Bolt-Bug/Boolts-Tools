using UnityEngine;

public class BoltsInputActionAttribute : PropertyAttribute
{
    public string actionAssetField;

    public BoltsInputActionAttribute(string actionAssetField)
    {
        this.actionAssetField = actionAssetField;
    }
}
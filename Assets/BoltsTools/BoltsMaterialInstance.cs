using UnityEngine;

[AddComponentMenu("Bolts Tools/BoltsMaterialInstance")]
public class BoltsMaterialInstance : MonoBehaviour
{
    public Renderer targetRenderer;
    public int materialIndex;
    
    public Material mat;

    // Read-only Reference Kept So The Editor Can
    // Destroy The Old Instance Before Creating A New One
    [HideInInspector]
    public Material instancedMaterial;
    
    void OnDestroy()
    {
        if(instancedMaterial != null)
            Destroy(instancedMaterial);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (targetRenderer == null)
        {
            Renderer checkIfHasRenderer = GetComponent<Renderer>();
            if (checkIfHasRenderer != null)
                targetRenderer = checkIfHasRenderer;
        }
        
        // Checks Again So That It Can Assign The Material
        // If The Renderer Was Null
        if (targetRenderer != null && mat == null)
            mat = targetRenderer.sharedMaterials[materialIndex];
    }
#endif
}
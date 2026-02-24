using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class BoltsBoxCollider : MonoBehaviour
{
    public BoxCollider boxCollider { get; private set; }

    [HideInInspector]
    public bool px, py, pz, nx, ny, nz;
    
    public void SetBounds()
    {
        var directions = new (bool active, Vector3 dir)[]
        {
            (px, transform.right),
            (nx, -transform.right),
            (py, transform.up),
            (ny, -transform.up),
            (pz, transform.forward),
            (nz, -transform.forward),
        };

        Vector3 min = boxCollider.center - boxCollider.size / 2f;
        Vector3 max = boxCollider.center + boxCollider.size / 2f;
        
        foreach (var (active, dir) in directions)
        {
            if(!active) continue;
            
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 100))
            {
                Vector3 localHit = transform.InverseTransformPoint(hit.point);
                
                if (dir == transform.right)  max.x = localHit.x;
                if (dir == -transform.right)   min.x = localHit.x;
                if (dir == transform.up)     max.y = localHit.y;
                if (dir == -transform.up)   min.y = localHit.y;
                if (dir == transform.forward) max.z = localHit.z;
                if (dir == -transform.forward)   min.z = localHit.z;
            }
        }

        #if UNITY_EDITOR
        Undo.RecordObject(boxCollider, "Set The Bounds On Box Collider");
        #endif
        
        boxCollider.center = (min + max) / 2;
        boxCollider.size = min - max;
    }
    
    private void OnValidate()
    {
        if (boxCollider == null)
        {
            boxCollider = gameObject.GetComponent<BoxCollider>() == null ? gameObject.AddComponent<BoxCollider>() : 
                gameObject.GetComponent<BoxCollider>();
        }
        
        boxCollider.hideFlags = HideFlags.HideInInspector;
    }

    private void Awake()
    {
        if (boxCollider == null)
        {
            boxCollider = gameObject.GetComponent<BoxCollider>() == null ? gameObject.AddComponent<BoxCollider>() : 
                gameObject.GetComponent<BoxCollider>();
        }
        
        boxCollider.hideFlags = HideFlags.HideInInspector;
    }

    private void OnDestroy()
    {
        foreach (BoxCollider bc in gameObject.GetComponents<BoxCollider>())
        {
            if(bc.hideFlags == HideFlags.HideInInspector)
                DestroyImmediate(bc);
        }
    }
}

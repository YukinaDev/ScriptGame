using UnityEngine;

[ExecuteInEditMode]
public class ScaleHelper : MonoBehaviour
{
    [Header("Đo lường kích thước thực")]
    public bool showDimensions = true;
    
    private Vector3 actualSize;

    void Update()
    {
        if (showDimensions)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                actualSize = renderer.bounds.size;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showDimensions)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Vector3 size = renderer.bounds.size;
                Vector3 center = renderer.bounds.center;
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(center, size);
                
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * size.y * 0.6f, 
                    $"Kích thước:\nX: {size.x:F2}m\nY: {size.y:F2}m\nZ: {size.z:F2}m");
#endif
            }
        }
    }
    
    [ContextMenu("Scale To Standard Door (2m x 0.8m)")]
    void ScaleToStandardDoor()
    {
        ScaleToSize(0.8f, 2.0f, 0.1f);
    }
    
    [ContextMenu("Scale To Standard Wall (Height 3m)")]
    void ScaleToStandardWall()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 currentSize = renderer.bounds.size;
            float scaleRatio = 3.0f / currentSize.y;
            transform.localScale *= scaleRatio;
        }
    }
    
    void ScaleToSize(float targetWidth, float targetHeight, float targetDepth)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 currentSize = renderer.bounds.size;
            Vector3 localScale = transform.localScale;
            
            transform.localScale = new Vector3(
                localScale.x * (targetWidth / currentSize.x),
                localScale.y * (targetHeight / currentSize.y),
                localScale.z * (targetDepth / currentSize.z)
            );
        }
    }
}

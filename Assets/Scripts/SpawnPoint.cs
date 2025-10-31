using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public bool isEnabled = true;
    public float heightOffset = 0.5f;
    public bool showGizmo = true;
    public Color gizmoColor = Color.cyan;
    public float gizmoRadius = 0.5f;
    
    public bool CanSpawn()
    {
        return isEnabled;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = isEnabled ? gizmoColor : Color.gray;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        // Vẽ mũi tên chỉ hướng lên
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoRadius * 2);
        
        // Vẽ vòng tròn ở trên
        Gizmos.DrawWireSphere(transform.position + Vector3.up * heightOffset, gizmoRadius * 0.8f);
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        
        // Vẽ sphere đặc khi được chọn
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position + Vector3.up * heightOffset, gizmoRadius);
    }
}

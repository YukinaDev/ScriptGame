using UnityEngine;

public class SnapToGround : MonoBehaviour
{
    [ContextMenu("Snap To Ground")]
    void SnapObjectToGround()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            float objectBottom = renderer.bounds.min.y;
            float groundY = 0f; // Hoặc lấy Y của Plane nếu khác 0
            
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + (groundY - objectBottom),
                transform.position.z
            );
        }
    }
}

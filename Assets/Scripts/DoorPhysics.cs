using UnityEngine;

public class DoorPhysics : MonoBehaviour
{
    [Header("References")]
    public BoxCollider physicalBlockCollider;
    public DoorInteraction doorInteraction;
    
    void Start()
    {
        if (doorInteraction == null)
            doorInteraction = GetComponent<DoorInteraction>();
        
        if (physicalBlockCollider == null)
        {
            // Tìm collider không phải trigger
            BoxCollider[] colliders = GetComponents<BoxCollider>();
            foreach (BoxCollider col in colliders)
            {
                if (!col.isTrigger)
                {
                    physicalBlockCollider = col;
                    break;
                }
            }
        }
        
        UpdatePhysics();
    }
    
    void Update()
    {
        // Cập nhật collider theo locked state
        if (doorInteraction != null && physicalBlockCollider != null)
        {
            physicalBlockCollider.enabled = doorInteraction.isLocked;
        }
    }
    
    public void UpdatePhysics()
    {
        if (doorInteraction != null && physicalBlockCollider != null)
        {
            // Nếu cửa locked → bật collider để block
            // Nếu unlocked → tắt collider để đi qua
            physicalBlockCollider.enabled = doorInteraction.isLocked;
            
            Debug.Log($"[DoorPhysics] {gameObject.name} - Locked: {doorInteraction.isLocked}, Collider enabled: {physicalBlockCollider.enabled}");
        }
    }
}

using UnityEngine;

public class DoorInteractionDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== DOOR INTERACTION DEBUG ===");
        
        DoorInteraction door = GetComponent<DoorInteraction>();
        if (door != null)
        {
            Debug.Log($"DoorInteraction found on {gameObject.name}");
            Debug.Log($"Is Locked: {door.isLocked}");
            Debug.Log($"Required Key: {door.requiredKeyItem}");
            Debug.Log($"Target Scene: {door.targetSceneName}");
        }
        else
        {
            Debug.LogError("DoorInteraction NOT FOUND!");
        }
        
        if (MessageDisplay.Instance != null)
        {
            Debug.Log("MessageDisplay.Instance EXISTS!");
        }
        else
        {
            Debug.LogError("MessageDisplay.Instance is NULL! Need to setup MessageDisplay in scene!");
        }
    }

    void Update()
    {
        // Temporarily disabled - need to configure Input System in Player Settings
        // Edit > Project Settings > Player > Active Input Handling > Change to "Both" or "Input Manager (Old)"
        /*
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Testing message display...");
            
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("TEST MESSAGE - I need Red Key");
            }
            else
            {
                Debug.LogError("MessageDisplay.Instance is NULL!");
            }
        }
        */
    }
}

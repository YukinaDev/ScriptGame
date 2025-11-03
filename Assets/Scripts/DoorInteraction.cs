using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public string doorID = "Door_HouseA";
    public string targetSceneName = "HouseA_Scene";
    public bool isLocked = false;
    public string requiredKeyItem = "";
    
    [Header("UI")]
    public string lockedMessage = "This door is locked";
    public string unlockedMessage = "Press E to Enter";
    
    [Header("Loading")]
    public GameObject loadingPanel;
    public float loadingDelay = 0.5f;
    
    [Header("Completion Status")]
    public bool showCompletionStatus = true;
    public string completedText = " [COMPLETED]";

    private bool playerNearby = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerData.IsDoorUnlocked(doorID))
            {
                isLocked = false;
            }
        }
    }

    public string GetInteractPrompt()
    {
        if (isLocked)
        {
            if (!string.IsNullOrEmpty(requiredKeyItem))
            {
                return $"I need {requiredKeyItem}";
            }
            return lockedMessage;
        }

        string message = unlockedMessage;

        if (showCompletionStatus && GameManager.Instance != null)
        {
            string houseName = ExtractHouseName(targetSceneName);
            if (GameManager.Instance.playerData.IsHouseCompleted(houseName))
            {
                message += completedText;
            }
        }

        return message;
    }

    public void Interact(GameObject player)
    {
        if (isLocked)
        {
            if (!string.IsNullOrEmpty(requiredKeyItem))
            {
                InventorySystem inventory = player.GetComponent<InventorySystem>();
                if (inventory != null && HasRequiredKey(inventory))
                {
                    UnlockDoor();
                    RemoveKey(inventory);
                    Debug.Log($"Door {doorID} unlocked with {requiredKeyItem}!");
                }
                else
                {
                    ShowMessage($"I need {requiredKeyItem}");
                    return;
                }
            }
            else
            {
                ShowMessage(lockedMessage);
                return;
            }
        }

        StartCoroutine(LoadSceneWithAnimation());
    }

    bool HasRequiredKey(InventorySystem inventory)
    {
        Debug.Log($"[DoorInteraction] Checking for required key: '{requiredKeyItem}'");
        Debug.Log($"[DoorInteraction] Inventory has {inventory.inventory.Length} slots");
        
        foreach (GameObject item in inventory.inventory)
        {
            if (item != null)
            {
                string itemName = item.name.Replace("(Clone)", "").Trim();
                Debug.Log($"[DoorInteraction] Found item in inventory: '{itemName}'");
                
                if (itemName.Equals(requiredKeyItem, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[DoorInteraction] KEY MATCH! '{itemName}' == '{requiredKeyItem}'");
                    return true;
                }
                else
                {
                    Debug.Log($"[DoorInteraction] No match: '{itemName}' != '{requiredKeyItem}'");
                }
            }
        }
        
        Debug.LogWarning($"[DoorInteraction] Required key '{requiredKeyItem}' NOT FOUND in inventory!");
        return false;
    }

    void RemoveKey(InventorySystem inventory)
    {
        for (int i = 0; i < inventory.inventory.Length; i++)
        {
            if (inventory.inventory[i] != null)
            {
                string itemName = inventory.inventory[i].name.Replace("(Clone)", "").Trim();
                if (itemName.Equals(requiredKeyItem, System.StringComparison.OrdinalIgnoreCase))
                {
                    Destroy(inventory.inventory[i]);
                    inventory.inventory[i] = null;
                    inventory.UpdateItemDisplay();
                    break;
                }
            }
        }
    }

    void UnlockDoor()
    {
        isLocked = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.UnlockDoor(doorID);
        }
    }

    IEnumerator LoadSceneWithAnimation()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return new WaitForSeconds(loadingDelay);

        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadHouseScene(targetSceneName);
        }
        else
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }

    void ShowMessage(string message)
    {
        Debug.Log($"[DoorInteraction] Showing message: {message}");
        
        if (MessageDisplay.Instance != null)
        {
            Debug.Log("[DoorInteraction] MessageDisplay.Instance EXISTS!");
            MessageDisplay.Instance.ShowMessage(message);
        }
        else
        {
            Debug.LogError("[DoorInteraction] MessageDisplay.Instance is NULL! Cannot show UI message!");
            Debug.LogError("Make sure you have MessageDisplay GameObject in the scene with MessageDisplay script attached!");
        }
    }

    string ExtractHouseName(string sceneName)
    {
        return sceneName.Replace("_Scene", "").Replace("Scene", "").Trim();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 3f, 0.2f));
    }
}

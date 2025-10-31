using UnityEngine;

public abstract class LockableObject : MonoBehaviour, IInteractable
{
    [Header("Lock Settings")]
    public string lockID = "Lock_01";
    public bool isLocked = true;
    public string requiredKeyID = "Key_Red";
    
    [Header("Messages")]
    public string lockedMessage = "Locked";
    public string unlockedMessage = "Unlocked";
    public string needKeyMessage = "You need {0} to unlock this";
    public string wrongKeyMessage = "This key doesn't fit";
    
    [Header("Auto Save Unlock State")]
    public bool saveUnlockState = true;

    protected virtual void Start()
    {
        if (saveUnlockState && GameManager.Instance != null)
        {
            if (GameManager.Instance.playerData.IsDoorUnlocked(lockID))
            {
                isLocked = false;
                OnUnlocked();
            }
        }
    }

    public virtual string GetInteractPrompt()
    {
        if (isLocked)
        {
            return GetLockedPrompt();
        }
        return GetUnlockedPrompt();
    }

    protected virtual string GetLockedPrompt()
    {
        KeyItem requiredKey = FindKeyByID(requiredKeyID);
        if (requiredKey != null)
        {
            return string.Format(needKeyMessage, requiredKey.keyDisplayName);
        }
        return lockedMessage;
    }

    protected virtual string GetUnlockedPrompt()
    {
        return unlockedMessage;
    }

    public virtual void Interact(GameObject player)
    {
        if (isLocked)
        {
            TryUnlock(player);
        }
        else
        {
            OnInteractUnlocked(player);
        }
    }

    protected virtual void TryUnlock(GameObject player)
    {
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            Debug.Log("No inventory found!");
            return;
        }

        KeyItem keyInInventory = FindKeyInInventory(inventory, requiredKeyID);

        if (keyInInventory != null)
        {
            Unlock();
            Debug.Log($"{lockID} unlocked with {keyInInventory.keyDisplayName}!");
        }
        else
        {
            KeyItem anyKey = FindAnyKeyInInventory(inventory);
            if (anyKey != null)
            {
                Debug.Log(wrongKeyMessage);
            }
            else
            {
                KeyItem requiredKey = FindKeyByID(requiredKeyID);
                string keyName = requiredKey != null ? requiredKey.keyDisplayName : requiredKeyID;
                Debug.Log(string.Format(needKeyMessage, keyName));
            }
        }
    }

    protected virtual void Unlock()
    {
        isLocked = false;

        if (saveUnlockState && GameManager.Instance != null)
        {
            GameManager.Instance.playerData.UnlockDoor(lockID);
        }

        OnUnlocked();
    }

    protected virtual void OnUnlocked()
    {
    }

    protected virtual void OnInteractUnlocked(GameObject player)
    {
        Debug.Log($"{lockID} is already unlocked!");
    }

    KeyItem FindKeyInInventory(InventorySystem inventory, string keyID)
    {
        foreach (GameObject item in inventory.inventory)
        {
            if (item != null)
            {
                KeyItem key = item.GetComponent<KeyItem>();
                if (key != null && key.CanUnlock(keyID))
                {
                    return key;
                }
            }
        }
        return null;
    }

    KeyItem FindAnyKeyInInventory(InventorySystem inventory)
    {
        foreach (GameObject item in inventory.inventory)
        {
            if (item != null)
            {
                KeyItem key = item.GetComponent<KeyItem>();
                if (key != null)
                {
                    return key;
                }
            }
        }
        return null;
    }

    KeyItem FindKeyByID(string keyID)
    {
        KeyItem[] allKeys = FindObjectsOfType<KeyItem>(true);
        foreach (KeyItem key in allKeys)
        {
            if (key.keyID.Equals(keyID, System.StringComparison.OrdinalIgnoreCase))
            {
                return key;
            }
        }
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}

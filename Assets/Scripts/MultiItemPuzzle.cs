using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Puzzle cần nhiều items để unlock
public class MultiItemPuzzle : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string objectName = "Complex Puzzle";
    public bool isLocked = true;
    
    [Header("Required Items")]
    [Tooltip("Danh sách tên items cần để unlock")]
    public List<string> requiredItemNames = new List<string>();
    [Tooltip("Nếu true, items sẽ bị xóa sau khi dùng")]
    public bool consumeItemsOnUse = false;
    
    [Header("Interact Prompts")]
    public string lockedPrompt = "Press E to unlock (need {0}/{1} items)";
    public string unlockedPrompt = "Press E to interact";
    public string missingItemsMessage = "You need: {0}";
    public string unlockSuccessMessage = "{0} unlocked!";
    
    [Header("Events")]
    public UnityEvent OnUnlocked;
    public UnityEvent OnInteractWhenUnlocked;
    
    [Header("Audio")]
    public AudioClip unlockSound;
    public AudioClip interactSound;
    private AudioSource audioSource;
    
    // Tracking items đã có
    private HashSet<string> collectedItems = new HashSet<string>();
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public string GetInteractPrompt()
    {
        if (isLocked)
        {
            int collected = collectedItems.Count;
            int total = requiredItemNames.Count;
            return string.Format(lockedPrompt, collected, total);
        }
        else
        {
            return unlockedPrompt;
        }
    }
    
    public void Interact(GameObject player)
    {
        if (isLocked)
        {
            TryUnlock(player);
        }
        else
        {
            InteractWhenUnlocked(player);
        }
    }
    
    void TryUnlock(GameObject player)
    {
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogWarning("[MultiItemPuzzle] Player has no InventorySystem!");
            return;
        }
        
        // Check items trong inventory
        List<int> itemIndicesToRemove = new List<int>();
        List<string> foundItems = new List<string>();
        
        for (int i = 0; i < inventory.inventory.Length; i++)
        {
            if (inventory.inventory[i] != null)
            {
                string itemName = inventory.inventory[i].name.Replace("(Clone)", "").Trim();
                
                foreach (string required in requiredItemNames)
                {
                    if (itemName.Equals(required, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (!collectedItems.Contains(required))
                        {
                            collectedItems.Add(required);
                            foundItems.Add(required);
                            
                            if (consumeItemsOnUse)
                            {
                                itemIndicesToRemove.Add(i);
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        // Check xem đã đủ items chưa
        if (collectedItems.Count >= requiredItemNames.Count)
        {
            // Unlock!
            isLocked = false;
            
            // Remove items nếu cần
            if (consumeItemsOnUse)
            {
                foreach (int index in itemIndicesToRemove)
                {
                    Destroy(inventory.inventory[index]);
                    inventory.inventory[index] = null;
                }
                inventory.UpdateInventoryUI();
                Debug.Log($"[MultiItemPuzzle] Consumed {itemIndicesToRemove.Count} items");
            }
            
            // Play sound
            if (unlockSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }
            
            // Show message
            MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
            if (messageDisplay != null)
            {
                messageDisplay.ShowMessage(string.Format(unlockSuccessMessage, objectName));
            }
            
            // Register với GameDataManager
            UniqueID uid = GetComponent<UniqueID>();
            if (uid != null && GameDataManager.Instance != null)
            {
                GameDataManager.Instance.RegisterPuzzleUnlock(uid.ID);
            }
            
            // Trigger event
            OnUnlocked?.Invoke();
            
            Debug.Log($"[MultiItemPuzzle] {objectName} unlocked with all required items!");
        }
        else
        {
            // Chưa đủ items
            List<string> missingItems = new List<string>();
            foreach (string required in requiredItemNames)
            {
                if (!collectedItems.Contains(required))
                {
                    missingItems.Add(required);
                }
            }
            
            string missingList = string.Join(", ", missingItems);
            
            MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
            if (messageDisplay != null)
            {
                messageDisplay.ShowMessage(string.Format(missingItemsMessage, missingList));
            }
            
            Debug.Log($"[MultiItemPuzzle] Still need: {missingList} ({collectedItems.Count}/{requiredItemNames.Count})");
        }
    }
    
    void InteractWhenUnlocked(GameObject player)
    {
        // Play sound
        if (interactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactSound);
        }
        
        // Trigger event
        OnInteractWhenUnlocked?.Invoke();
        
        Debug.Log($"[MultiItemPuzzle] Interacted with unlocked {objectName}");
    }
    
    // Hàm public để unlock từ code
    public void Unlock()
    {
        if (isLocked)
        {
            isLocked = false;
            OnUnlocked?.Invoke();
            Debug.Log($"[MultiItemPuzzle] {objectName} unlocked programmatically");
        }
    }
}

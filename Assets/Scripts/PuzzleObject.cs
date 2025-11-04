using UnityEngine;
using UnityEngine.Events;

public class PuzzleObject : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string objectName = "Locked Object";
    public bool isLocked = true;
    
    [Header("Required Item")]
    public string requiredItemName = "Key";
    [Tooltip("Nếu true, item sẽ bị xóa sau khi dùng")]
    public bool consumeItemOnUse = false;
    
    [Header("Interact Prompts")]
    public string lockedPrompt = "Press E to unlock (need {0})";
    public string unlockedPrompt = "Press E to interact";
    public string missingItemMessage = "You need {0} to unlock this";
    public string unlockSuccessMessage = "{0} unlocked!";
    
    [Header("Events")]
    public UnityEvent OnUnlocked;
    public UnityEvent OnInteractWhenUnlocked;
    
    [Header("Optional Components")]
    public DoorInteraction doorInteraction; // Nếu là cửa
    public Animator animator; // Nếu có animation
    public string unlockAnimationTrigger = "Unlock";
    
    [Header("Audio")]
    public AudioClip unlockSound;
    public AudioClip interactSound;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Auto-find DoorInteraction nếu là cửa
        if (doorInteraction == null)
        {
            doorInteraction = GetComponent<DoorInteraction>();
        }
        
        // Auto-find Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    public string GetInteractPrompt()
    {
        if (isLocked)
        {
            return string.Format(lockedPrompt, requiredItemName);
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
            Debug.LogWarning("[PuzzleObject] Player has no InventorySystem!");
            return;
        }
        
        // Check xem có item cần thiết không
        bool hasItem = false;
        int itemIndex = -1;
        
        for (int i = 0; i < inventory.inventory.Length; i++)
        {
            if (inventory.inventory[i] != null)
            {
                string itemName = inventory.inventory[i].name.Replace("(Clone)", "").Trim();
                if (itemName.Equals(requiredItemName, System.StringComparison.OrdinalIgnoreCase))
                {
                    hasItem = true;
                    itemIndex = i;
                    break;
                }
            }
        }
        
        if (hasItem)
        {
            // Unlock!
            isLocked = false;
            
            // Remove item nếu cần
            if (consumeItemOnUse && itemIndex >= 0)
            {
                Destroy(inventory.inventory[itemIndex]);
                inventory.inventory[itemIndex] = null;
                inventory.UpdateInventoryUI();
                Debug.Log($"[PuzzleObject] Consumed {requiredItemName}");
            }
            
            // Play sound
            if (unlockSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }
            
            // Play animation
            if (animator != null && !string.IsNullOrEmpty(unlockAnimationTrigger))
            {
                animator.SetTrigger(unlockAnimationTrigger);
            }
            
            // Update DoorInteraction nếu là cửa
            if (doorInteraction != null)
            {
                doorInteraction.isLocked = false;
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
            
            Debug.Log($"[PuzzleObject] {objectName} unlocked with {requiredItemName}!");
        }
        else
        {
            // Không có item
            MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();
            if (messageDisplay != null)
            {
                messageDisplay.ShowMessage(string.Format(missingItemMessage, requiredItemName));
            }
            
            Debug.Log($"[PuzzleObject] Need {requiredItemName} to unlock {objectName}");
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
        
        Debug.Log($"[PuzzleObject] Interacted with unlocked {objectName}");
    }
    
    // Hàm public để unlock từ code
    public void Unlock()
    {
        if (isLocked)
        {
            isLocked = false;
            
            if (doorInteraction != null)
            {
                doorInteraction.isLocked = false;
            }
            
            OnUnlocked?.Invoke();
            Debug.Log($"[PuzzleObject] {objectName} unlocked programmatically");
        }
    }
    
    // Hàm public để lock lại
    public void Lock()
    {
        if (!isLocked)
        {
            isLocked = true;
            
            if (doorInteraction != null)
            {
                doorInteraction.isLocked = true;
            }
            
            Debug.Log($"[PuzzleObject] {objectName} locked");
        }
    }
}

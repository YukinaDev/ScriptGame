using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public Sprite itemSprite;
    public GameObject itemObject; // Lưu GameObject gốc!
}

[System.Serializable]
public class GameData
{
    // Player stats
    public float currentBattery = 100f;
    public float currentStamina = 100f;
    public bool hasFlashlight = false;
    
    // Inventory - lưu data thay vì GameObject
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    
    // World state
    public List<string> pickedUpItemIDs = new List<string>();
    public List<string> openedDoorIDs = new List<string>();
    public List<string> unlockedPuzzleIDs = new List<string>(); // Puzzle đã unlock
    
    // Scene tracking
    public string lastSceneName = "";
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    
    [Header("Game Data")]
    public GameData currentData = new GameData();
    
    [Header("Auto Save")]
    public bool autoSaveOnSceneChange = true;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    


    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (showDebugLogs)
            Debug.Log("[GameDataManager] Initialized");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Scene loaded: {scene.name}");
        
        // Load data vào scene mới
        StartCoroutine(LoadDataToSceneDelayed());
    }

    void OnSceneUnloaded(Scene scene)
    {
        // KHÔNG auto-save ở đây nữa vì Player đã bị destroy
        // Save sẽ được gọi từ DoorInteraction TRƯỚC KHI load scene
        
        currentData.lastSceneName = scene.name;
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Scene unloaded: {scene.name}");
    }

    System.Collections.IEnumerator LoadDataToSceneDelayed()
    {
        // Đợi 1 frame để các objects spawn xong
        yield return new WaitForEndOfFrame();
        
        // Đợi thêm để Player chắc chắn spawn
        int maxAttempts = 10;
        int attempts = 0;
        
        while (GameObject.FindGameObjectWithTag("Player") == null && attempts < maxAttempts)
        {
            Debug.Log($"[GameDataManager] Waiting for Player... Attempt {attempts + 1}/{maxAttempts}");
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }
        
        LoadDataToScene();
    }

    // ==================== SAVE DATA ====================
    
    public void SaveCurrentSceneData()
    {
        if (showDebugLogs)
            Debug.Log("[GameDataManager] Saving current scene data...");
        
        // Save player stats
        SavePlayerStats();
        
        // Save inventory
        SaveInventory();
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Data saved! Battery={currentData.currentBattery}, Stamina={currentData.currentStamina}, Items={currentData.inventoryItems.Count}, PickedUp={currentData.pickedUpItemIDs.Count}, Doors={currentData.openedDoorIDs.Count}, Puzzles={currentData.unlockedPuzzleIDs.Count}");
    }

    void SavePlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GameDataManager] Player not found when saving!");
            return;
        }
        
        // Save flashlight battery
        Flashlight flashlight = player.GetComponentInChildren<Flashlight>();
        if (flashlight != null)
        {
            Debug.Log($"[GameDataManager] Saving battery: {flashlight.currentBattery}, useBattery={flashlight.useBattery}, hasFlashlight={flashlight.startWithFlashlight}");
            currentData.currentBattery = flashlight.currentBattery;
            currentData.hasFlashlight = flashlight.startWithFlashlight;
        }
        else
        {
            Debug.LogWarning("[GameDataManager] Flashlight not found when saving!");
        }
        
        // Save stamina
        StaminaSystem stamina = player.GetComponent<StaminaSystem>();
        if (stamina != null)
        {
            Debug.Log($"[GameDataManager] Saving stamina: {stamina.currentStamina}");
            currentData.currentStamina = stamina.currentStamina;
        }
        else
        {
            Debug.LogWarning("[GameDataManager] StaminaSystem not found when saving!");
        }
    }

    void SaveInventory()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory == null) return;
        
        currentData.inventoryItems.Clear();
        
        foreach (GameObject item in inventory.inventory)
        {
            if (item != null)
            {
                InventoryItemData itemData = new InventoryItemData();
                itemData.itemName = item.name.Replace("(Clone)", "").Trim();
                itemData.itemObject = item;
                
                // MOVE item vào GameDataManager để không bị destroy
                item.transform.SetParent(transform);
                DontDestroyOnLoad(item);
                
                // Lấy sprite để hiển thị
                PickupItem pickupItem = item.GetComponent<PickupItem>();
                if (pickupItem != null && pickupItem.itemIcon != null)
                {
                    itemData.itemSprite = pickupItem.itemIcon;
                }
                
                // Fallback: FlashlightItem
                if (itemData.itemSprite == null)
                {
                    FlashlightItem flashlightItem = item.GetComponent<FlashlightItem>();
                    if (flashlightItem != null && flashlightItem.itemIcon != null)
                    {
                        itemData.itemSprite = flashlightItem.itemIcon;
                    }
                }
                
                // Fallback: SpriteRenderer
                if (itemData.itemSprite == null)
                {
                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        itemData.itemSprite = sr.sprite;
                    }
                }
                
                currentData.inventoryItems.Add(itemData);
                Debug.Log($"[GameDataManager] Saved item: {itemData.itemName}, moved to GameDataManager");
            }
        }
        
        Debug.Log($"[GameDataManager] Saved {currentData.inventoryItems.Count} inventory items");
    }

    // ==================== LOAD DATA ====================
    
    public void LoadDataToScene()
    {
        if (showDebugLogs)
            Debug.Log("[GameDataManager] Loading data to scene...");
        
        // Restore player stats
        RestorePlayerStats();
        
        // Restore inventory
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Calling RestoreInventory() with {currentData.inventoryItems.Count} items...");
        RestoreInventory();
        
        // Remove picked up items
        RemovePickedUpItems();
        
        // Restore door states
        RestoreDoorStates();
        
        // Restore puzzle states
        RestorePuzzleStates();
        
        if (showDebugLogs)
            Debug.Log("[GameDataManager] Data loaded!");
    }

    void RestorePlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[GameDataManager] Player not found!");
            return;
        }
        
        // Restore flashlight
        Flashlight flashlight = player.GetComponentInChildren<Flashlight>();
        if (flashlight != null)
        {
            Debug.Log($"[GameDataManager] Restoring battery: {flashlight.currentBattery} → {currentData.currentBattery}");
            flashlight.currentBattery = currentData.currentBattery;
            
            if (currentData.hasFlashlight && !flashlight.startWithFlashlight)
            {
                Debug.Log("[GameDataManager] Enabling flashlight...");
                flashlight.EnableFlashlight();
                
                // Show battery bar
                UIManager uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.ShowBatteryBar();
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameDataManager] Flashlight not found on player!");
        }
        
        // Restore stamina
        StaminaSystem stamina = player.GetComponent<StaminaSystem>();
        if (stamina != null)
        {
            Debug.Log($"[GameDataManager] Restoring stamina: {stamina.currentStamina} → {currentData.currentStamina}");
            stamina.currentStamina = currentData.currentStamina;
        }
        else
        {
            Debug.LogWarning("[GameDataManager] StaminaSystem not found on player!");
        }
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Player stats restored: Battery={currentData.currentBattery}, Stamina={currentData.currentStamina}, HasFlashlight={currentData.hasFlashlight}");
    }

    void RestoreInventory()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory == null) return;
        
        // Restore inventory data
        for (int i = 0; i < currentData.inventoryItems.Count && i < inventory.maxSlots; i++)
        {
            InventoryItemData itemData = currentData.inventoryItems[i];
            
            if (itemData.itemObject != null)
            {
                // Dùng GameObject gốc (đang ở trong GameDataManager)
                inventory.inventory[i] = itemData.itemObject;
                itemData.itemObject.SetActive(false); // Vẫn hidden trong inventory
                Debug.Log($"[GameDataManager] Restored {itemData.itemName} to slot {i}");
            }
        }
        
        // Clear remaining slots
        for (int i = currentData.inventoryItems.Count; i < inventory.maxSlots; i++)
        {
            inventory.inventory[i] = null;
        }
        
        // Update UI sau khi restore
        inventory.UpdateInventoryUI();
        
        Debug.Log($"[GameDataManager] Restored {currentData.inventoryItems.Count} items to inventory");
    }

    void RemovePickedUpItems()
    {
        if (currentData.pickedUpItemIDs.Count == 0) return;
        
        // Tìm tất cả items có UniqueID
        UniqueID[] allItems = FindObjectsOfType<UniqueID>();
        int removedCount = 0;
        
        foreach (UniqueID item in allItems)
        {
            if (currentData.pickedUpItemIDs.Contains(item.ID))
            {
                if (showDebugLogs)
                    Debug.Log($"[GameDataManager] Removing already picked item: {item.gameObject.name} ({item.ID})");
                
                Destroy(item.gameObject);
                removedCount++;
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Removed {removedCount} already picked items");
    }

    void RestoreDoorStates()
    {
        if (currentData.openedDoorIDs.Count == 0) return;
        
        // Tìm tất cả doors có UniqueID
        DoorInteraction[] allDoors = FindObjectsOfType<DoorInteraction>();
        int restoredCount = 0;
        
        foreach (DoorInteraction door in allDoors)
        {
            UniqueID uid = door.GetComponent<UniqueID>();
            if (uid != null && currentData.openedDoorIDs.Contains(uid.ID))
            {
                // Mở cửa
                door.isLocked = false;
                
                if (showDebugLogs)
                    Debug.Log($"[GameDataManager] Restored door state: {door.gameObject.name} → Unlocked");
                
                restoredCount++;
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Restored {restoredCount} door states");
    }
    
    void RestorePuzzleStates()
    {
        if (currentData.unlockedPuzzleIDs.Count == 0) return;
        
        // Tìm tất cả puzzles có UniqueID
        PuzzleObject[] allPuzzles = FindObjectsOfType<PuzzleObject>();
        int restoredCount = 0;
        
        foreach (PuzzleObject puzzle in allPuzzles)
        {
            UniqueID uid = puzzle.GetComponent<UniqueID>();
            if (uid != null && currentData.unlockedPuzzleIDs.Contains(uid.ID))
            {
                // Unlock puzzle
                puzzle.isLocked = false;
                
                if (showDebugLogs)
                    Debug.Log($"[GameDataManager] Restored puzzle state: {puzzle.gameObject.name} → Unlocked");
                
                restoredCount++;
            }
        }
        
        // Tìm MultiItemPuzzle
        MultiItemPuzzle[] multiPuzzles = FindObjectsOfType<MultiItemPuzzle>();
        foreach (MultiItemPuzzle puzzle in multiPuzzles)
        {
            UniqueID uid = puzzle.GetComponent<UniqueID>();
            if (uid != null && currentData.unlockedPuzzleIDs.Contains(uid.ID))
            {
                // Unlock puzzle
                puzzle.isLocked = false;
                
                if (showDebugLogs)
                    Debug.Log($"[GameDataManager] Restored multi-item puzzle: {puzzle.gameObject.name} → Unlocked");
                
                restoredCount++;
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"[GameDataManager] Restored {restoredCount} puzzle states");
    }

    // ==================== PUBLIC API ====================
    
    public void RegisterItemPickup(string itemID)
    {
        if (!currentData.pickedUpItemIDs.Contains(itemID))
        {
            currentData.pickedUpItemIDs.Add(itemID);
            
            if (showDebugLogs)
                Debug.Log($"[GameDataManager] Registered item pickup: {itemID}");
        }
    }

    public void RegisterDoorOpened(string doorID)
    {
        if (!currentData.openedDoorIDs.Contains(doorID))
        {
            currentData.openedDoorIDs.Add(doorID);
            
            if (showDebugLogs)
                Debug.Log($"[GameDataManager] Registered door opened: {doorID}");
        }
    }
    
    public void RegisterPuzzleUnlock(string puzzleID)
    {
        if (!currentData.unlockedPuzzleIDs.Contains(puzzleID))
        {
            currentData.unlockedPuzzleIDs.Add(puzzleID);
            
            if (showDebugLogs)
                Debug.Log($"[GameDataManager] Registered puzzle unlocked: {puzzleID}");
        }
    }

    public bool IsItemPickedUp(string itemID)
    {
        return currentData.pickedUpItemIDs.Contains(itemID);
    }

    public bool IsDoorOpened(string doorID)
    {
        return currentData.openedDoorIDs.Contains(doorID);
    }
    
    public bool IsPuzzleUnlocked(string puzzleID)
    {
        return currentData.unlockedPuzzleIDs.Contains(puzzleID);
    }

    public void ResetGameData()
    {
        currentData = new GameData();
        
        if (showDebugLogs)
            Debug.Log("[GameDataManager] Game data reset!");
    }
}

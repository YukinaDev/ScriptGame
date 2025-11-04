using UnityEngine;

// ============================================================
// VÍ DỤ SỬ DỤNG PUZZLEOBJECT
// ============================================================

public class PuzzleExamples : MonoBehaviour
{
    // VÍ DỤ 1: Cửa cần key
    void Example_Door()
    {
        // Add PuzzleObject component vào cửa:
        // - objectName = "Main Door"
        // - isLocked = true
        // - requiredItemName = "Door Key"
        // - consumeItemOnUse = false (giữ key để dùng lại)
        // - Assign DoorInteraction component
        // - OnUnlocked event: Có thể gọi hàm mở cửa
    }
    
    // VÍ DỤ 2: Tủ cần key, key bị tiêu thụ
    void Example_Cabinet()
    {
        // Add PuzzleObject component vào tủ:
        // - objectName = "Cabinet"
        // - isLocked = true
        // - requiredItemName = "Cabinet Key"
        // - consumeItemOnUse = true (key bị xóa sau khi dùng)
        // - OnUnlocked event: Spawn items bên trong tủ
    }
    
    // VÍ DỤ 3: Két sắt cần mật khẩu (dùng item Password Note)
    void Example_Safe()
    {
        // Add PuzzleObject component vào két:
        // - objectName = "Safe"
        // - isLocked = true
        // - requiredItemName = "Password Note"
        // - consumeItemOnUse = false
        // - OnUnlocked event: Enable collider của treasure bên trong
    }
    
    // VÍ DỤ 4: Máy móc cần battery
    void Example_Machine()
    {
        // Add PuzzleObject component vào máy:
        // - objectName = "Generator"
        // - isLocked = true
        // - requiredItemName = "Battery"
        // - consumeItemOnUse = true (battery bị tiêu thụ)
        // - OnUnlocked event: Bật đèn trong phòng
    }
    
    // VÍ DỤ 5: Cổng cần 3 items khác nhau (chain multiple PuzzleObjects)
    void Example_ComplexPuzzle()
    {
        // Tạo 3 PuzzleObjects:
        // 1. Lever1 cần "Red Gem" → OnUnlocked: Unlock Lever2
        // 2. Lever2 cần "Blue Gem" → OnUnlocked: Unlock Lever3
        // 3. Lever3 cần "Green Gem" → OnUnlocked: Open gate
    }
    
    // CÁCH SỬ DỤNG VỚI CODE:
    void CodeExample()
    {
        PuzzleObject puzzle = GetComponent<PuzzleObject>();
        
        // Unlock từ code
        puzzle.Unlock();
        
        // Lock lại
        puzzle.Lock();
        
        // Check trạng thái
        if (puzzle.isLocked)
        {
            Debug.Log("Still locked");
        }
    }
    
    // CÁCH DÙNG EVENTS:
    void EventExample()
    {
        PuzzleObject puzzle = GetComponent<PuzzleObject>();
        
        // Add listener từ code
        puzzle.OnUnlocked.AddListener(() => {
            Debug.Log("Puzzle unlocked!");
            // Spawn reward, bật đèn, mở cửa, etc.
        });
        
        puzzle.OnInteractWhenUnlocked.AddListener(() => {
            Debug.Log("Interacted with unlocked object!");
            // Load scene, hiện dialogue, etc.
        });
    }
}

// ============================================================
// VÍ DỤ SCRIPT SPAWN ITEMS KHI UNLOCK TỦ
// ============================================================
public class CabinetReward : MonoBehaviour
{
    public GameObject[] itemsToSpawn;
    public Transform spawnPoint;
    
    void Start()
    {
        PuzzleObject puzzle = GetComponent<PuzzleObject>();
        if (puzzle != null)
        {
            puzzle.OnUnlocked.AddListener(SpawnRewards);
        }
    }
    
    void SpawnRewards()
    {
        foreach (GameObject item in itemsToSpawn)
        {
            if (item != null)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                Instantiate(item, spawnPos, Quaternion.identity);
            }
        }
        
        Debug.Log("[CabinetReward] Spawned rewards!");
    }
}

// ============================================================
// VÍ DỤ SCRIPT BẬT ĐÈN KHI UNLOCK GENERATOR
// ============================================================
public class GeneratorPower : MonoBehaviour
{
    public Light[] lightsToEnable;
    
    void Start()
    {
        PuzzleObject puzzle = GetComponent<PuzzleObject>();
        if (puzzle != null)
        {
            puzzle.OnUnlocked.AddListener(EnableLights);
        }
    }
    
    void EnableLights()
    {
        foreach (Light light in lightsToEnable)
        {
            if (light != null)
            {
                light.enabled = true;
            }
        }
        
        Debug.Log("[GeneratorPower] Lights enabled!");
    }
}

// ============================================================
// VÍ DỤ SCRIPT CHUYỂN SCENE KHI UNLOCK GATE
// ============================================================
public class GateToNextLevel : MonoBehaviour
{
    public string nextSceneName = "Level2";
    
    void Start()
    {
        PuzzleObject puzzle = GetComponent<PuzzleObject>();
        if (puzzle != null)
        {
            puzzle.OnInteractWhenUnlocked.AddListener(LoadNextLevel);
        }
    }
    
    void LoadNextLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}

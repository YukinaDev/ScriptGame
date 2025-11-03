using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        public GameObject itemPrefab;
        [Tooltip("Cho phép spawn nhiều lần")]
        public bool allowDuplicate = false;
        [Tooltip("Số lần spawn (chỉ dùng khi Duplicate Mode = Fixed)")]
        public int duplicateCount = 2;
    }
    
    public enum DuplicateMode
    {
        Fill,   // Tự động fill hết spawn points
        Fixed   // Spawn số lần cố định theo duplicateCount
    }


    [Header("Spawn Settings")]
    [SerializeField] private List<SpawnableItem> spawnableItems = new List<SpawnableItem>();
    
    [Header("Duplicate Mode")]
    [Tooltip("Fill: Tự động fill hết spawn points | Fixed: Spawn số lần cố định")]
    [SerializeField] private DuplicateMode duplicateMode = DuplicateMode.Fill;
    
    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Random;
    [SerializeField] [Range(0f, 1f)] private float designatedPointChance = 0.5f; // Tỷ lệ spawn ở điểm chỉ định
    
    [Header("Spawn Area (Box Bounds) - For Random Mode")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(50f, 0f, 50f);
    
    [Header("Designated Spawn Points - For Point Mode")]
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    
    public enum SpawnMode
    {
        Random,         // Spawn ngẫu nhiên trong area
        Designated,     // Chỉ spawn ở điểm chỉ định
        Mixed           // Phối hợp cả hai
    }
    
    [Header("Spawn Options")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private float groundCheckDistance = 100f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float heightOffset = 0.5f;  // Độ cao từ mặt đất
    [SerializeField] private float minDistanceBetweenItems = 2f;
    
    [Header("Item Behavior")]
    [Tooltip("Tắt animation bob (lên xuống) của items khi spawn")]
    [SerializeField] private bool disableBobAnimation = true;
    [Tooltip("Tắt animation xoay của items khi spawn")]
    [SerializeField] private bool disableRotation = false;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private List<SpawnPoint> usedSpawnPoints = new List<SpawnPoint>();

    void Start()
    {
        // Tự động tìm các SpawnPoint trong scene nếu chưa có
        if (spawnPoints.Count == 0)
        {
            FindSpawnPointsInScene();
        }
        
        if (spawnOnStart)
        {
            SpawnAllItems();
        }
    }
    
    void FindSpawnPointsInScene()
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        spawnPoints.AddRange(points);
        Debug.Log($"Found {points.Length} spawn points in scene");
    }

    public void SpawnAllItems()
    {
        spawnedPositions.Clear();
        usedSpawnPoints.Clear();
        
        if (spawnableItems.Count == 0)
        {
            Debug.LogWarning("ItemSpawner: No items in spawnableItems list!");
            return;
        }
        
        // Tạo list các prefabs để spawn (mỗi item trong list spawn 1 lần)
        List<GameObject> itemsToSpawn = new List<GameObject>();
        List<GameObject> duplicableItems = new List<GameObject>(); // Items có thể spawn thêm
        
        foreach (var item in spawnableItems)
        {
            if (item != null && item.itemPrefab != null)
            {
                itemsToSpawn.Add(item.itemPrefab);
                
                // Lưu items có allowDuplicate
                if (item.allowDuplicate)
                {
                    duplicableItems.Add(item.itemPrefab);
                }
            }
        }
        
        // Tính số spawn points có sẵn
        int availableSpawnPoints = GetAvailableSpawnPointCount();
        
        // Shuffle để spawn random
        ShuffleList(itemsToSpawn);
        
        int itemsSpawned = 0;
        int maxAttempts = availableSpawnPoints * 10; // Tránh vòng lặp vô hạn
        int attempts = 0;
        
        // Phase 1: Spawn từng item trong list
        for (int i = 0; i < itemsToSpawn.Count && attempts < maxAttempts; i++)
        {
            bool spawned = false;
            int attemptsForThisItem = 0;
            
            while (!spawned && attemptsForThisItem < 10)
            {
                attempts++;
                attemptsForThisItem++;
                
                Vector3 spawnPos = GetSpawnPosition();
                
                if (spawnPos != Vector3.zero && IsPositionValid(spawnPos))
                {
                    SpawnItem(itemsToSpawn[i], spawnPos);
                    spawnedPositions.Add(spawnPos);
                    itemsSpawned++;
                    spawned = true;
                }
            }
            
            if (!spawned)
            {
                Debug.LogWarning($"ItemSpawner: Không tìm được vị trí spawn cho {itemsToSpawn[i].name}");
            }
        }
        
        // Phase 2: Spawn duplicate items theo mode
        if (duplicateMode == DuplicateMode.Fill)
        {
            // Fill mode: Tự động fill hết spawn points
            if (duplicableItems.Count > 0 && itemsSpawned < availableSpawnPoints)
            {
                int remainingSlots = availableSpawnPoints - itemsSpawned;
                Debug.Log($"ItemSpawner: [Fill Mode] Filling {remainingSlots} remaining slots with duplicable items");
                
                for (int i = 0; i < remainingSlots && attempts < maxAttempts; i++)
                {
                    bool spawned = false;
                    int attemptsForThisItem = 0;
                    
                    // Random pick từ duplicable items
                    GameObject prefabToSpawn = duplicableItems[Random.Range(0, duplicableItems.Count)];
                    
                    while (!spawned && attemptsForThisItem < 10)
                    {
                        attempts++;
                        attemptsForThisItem++;
                        
                        Vector3 spawnPos = GetSpawnPosition();
                        
                        if (spawnPos != Vector3.zero && IsPositionValid(spawnPos))
                        {
                            SpawnItem(prefabToSpawn, spawnPos);
                            spawnedPositions.Add(spawnPos);
                            itemsSpawned++;
                            spawned = true;
                        }
                    }
                    
                    if (!spawned)
                    {
                        Debug.LogWarning($"ItemSpawner: Không tìm được vị trí spawn thêm cho {prefabToSpawn.name}");
                        break; // Dừng nếu không còn chỗ spawn
                    }
                }
            }
        }
        else // Fixed mode
        {
            // Fixed mode: Spawn theo duplicateCount cố định
            foreach (var item in spawnableItems)
            {
                if (item != null && item.itemPrefab != null && item.allowDuplicate)
                {
                    // Spawn thêm (duplicateCount - 1) lần (vì đã spawn 1 lần ở Phase 1)
                    int additionalSpawns = item.duplicateCount - 1;
                    
                    Debug.Log($"ItemSpawner: [Fixed Mode] Spawning {additionalSpawns} additional {item.itemPrefab.name}");
                    
                    for (int i = 0; i < additionalSpawns && attempts < maxAttempts; i++)
                    {
                        bool spawned = false;
                        int attemptsForThisItem = 0;
                        
                        while (!spawned && attemptsForThisItem < 10)
                        {
                            attempts++;
                            attemptsForThisItem++;
                            
                            Vector3 spawnPos = GetSpawnPosition();
                            
                            if (spawnPos != Vector3.zero && IsPositionValid(spawnPos))
                            {
                                SpawnItem(item.itemPrefab, spawnPos);
                                spawnedPositions.Add(spawnPos);
                                itemsSpawned++;
                                spawned = true;
                            }
                        }
                        
                        if (!spawned)
                        {
                            Debug.LogWarning($"ItemSpawner: Không tìm được vị trí spawn thêm cho {item.itemPrefab.name}");
                            break;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"ItemSpawner: Spawned {itemsSpawned} items in {attempts} attempts");
    }
    
    int GetAvailableSpawnPointCount()
    {
        // Nếu dùng designated mode, đếm spawn points
        if (spawnMode == SpawnMode.Designated)
        {
            return spawnPoints.Count;
        }
        else if (spawnMode == SpawnMode.Mixed)
        {
            // Mixed mode: ước lượng dựa trên spawn area và min distance
            float areaVolume = spawnAreaSize.x * spawnAreaSize.z;
            float itemCircleArea = Mathf.PI * (minDistanceBetweenItems * minDistanceBetweenItems);
            int estimatedCapacity = Mathf.Max(10, Mathf.FloorToInt(areaVolume / itemCircleArea));
            return Mathf.Min(estimatedCapacity, spawnPoints.Count + 20);
        }
        else // Random mode
        {
            // Ước lượng số items có thể spawn dựa trên area và min distance
            float areaVolume = spawnAreaSize.x * spawnAreaSize.z;
            float itemCircleArea = Mathf.PI * (minDistanceBetweenItems * minDistanceBetweenItems);
            return Mathf.Max(10, Mathf.FloorToInt(areaVolume / itemCircleArea));
        }
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }



    Vector3 GetSpawnPosition()
    {
        switch (spawnMode)
        {
            case SpawnMode.Random:
                return GetRandomSpawnPosition();
                
            case SpawnMode.Designated:
                return GetDesignatedSpawnPosition();
                
            case SpawnMode.Mixed:
                if (Random.value < designatedPointChance)
                    return GetDesignatedSpawnPosition();
                else
                    return GetRandomSpawnPosition();
                
            default:
                return GetRandomSpawnPosition();
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Random position trong box bounds
        Vector3 worldCenter = transform.position + spawnAreaCenter;
        
        float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float randomZ = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);
        
        Vector3 randomPos = worldCenter + new Vector3(randomX, 0f, randomZ);
        
        // Snap to ground nếu cần
        if (snapToGround)
        {
            RaycastHit hit;
            Vector3 rayStart = randomPos + Vector3.up * 50f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
            {
                randomPos = hit.point + Vector3.up * heightOffset;  // Thêm độ cao
            }
        }
        else
        {
            randomPos.y += heightOffset;  // Thêm độ cao nếu không snap to ground
        }
        
        return randomPos;
    }

    Vector3 GetDesignatedSpawnPosition()
    {
        List<SpawnPoint> availablePoints = new List<SpawnPoint>();
        
        foreach (SpawnPoint point in spawnPoints)
        {
            if (!usedSpawnPoints.Contains(point) && point.CanSpawn())
            {
                availablePoints.Add(point);
            }
        }
        
        if (availablePoints.Count == 0)
        {
            // Nếu hết điểm spawn, dùng random mode hoặc trả về zero
            return GetRandomSpawnPosition();
        }
        
        SpawnPoint selectedPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        usedSpawnPoints.Add(selectedPoint);
        
        // Đặt offset theo spawn point
        Vector3 position = selectedPoint.transform.position;
        position += Vector3.up * selectedPoint.heightOffset;
        
        return position;
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 existingPos in spawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenItems)
            {
                return false;
            }
        }
        return true;
    }

    void SpawnItem(GameObject prefab, Vector3 position)
    {
        GameObject spawnedItem = Instantiate(prefab, position, Quaternion.identity);
        spawnedItem.transform.SetParent(transform); // Organize dưới ItemSpawner
        
        // Freeze Rigidbody để item không trôi nổi
        Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Không bị ảnh hưởng bởi physics
            rb.useGravity = false;
        }
        
        // Tắt animations nếu được yêu cầu
        PickupItem pickupItem = spawnedItem.GetComponent<PickupItem>();
        if (pickupItem != null)
        {
            if (disableBobAnimation)
            {
                pickupItem.bobUpDown = false;
            }
            
            if (disableRotation)
            {
                pickupItem.rotateItem = false;
            }
        }
        
        // Tắt animations cho BatteryItem
        BatteryItem batteryItem = spawnedItem.GetComponent<BatteryItem>();
        if (batteryItem != null)
        {
            if (disableBobAnimation)
            {
                batteryItem.bobUpDown = false;
            }
            
            if (disableRotation)
            {
                batteryItem.rotateItem = false;
            }
        }
        
        // Tắt animations cho FlashlightItem
        FlashlightItem flashlightItem = spawnedItem.GetComponent<FlashlightItem>();
        if (flashlightItem != null)
        {
            if (disableBobAnimation)
            {
                flashlightItem.bobUpDown = false;
            }
            
            if (disableRotation)
            {
                flashlightItem.rotateItem = false;
            }
        }
    }

    public void ClearAllSpawnedItems()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();
        usedSpawnPoints.Clear();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Vector3 worldCenter = transform.position + spawnAreaCenter;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldCenter, spawnAreaSize);
        
        // Vẽ grid để dễ visualize
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        
        float gridSize = 5f;
        int gridCountX = Mathf.CeilToInt(spawnAreaSize.x / gridSize);
        int gridCountZ = Mathf.CeilToInt(spawnAreaSize.z / gridSize);
        
        Vector3 startPos = worldCenter - spawnAreaSize / 2f;
        
        for (int x = 0; x <= gridCountX; x++)
        {
            Vector3 start = startPos + new Vector3(x * gridSize, 0f, 0f);
            Vector3 end = start + new Vector3(0f, 0f, spawnAreaSize.z);
            Gizmos.DrawLine(start, end);
        }
        
        for (int z = 0; z <= gridCountZ; z++)
        {
            Vector3 start = startPos + new Vector3(0f, 0f, z * gridSize);
            Vector3 end = start + new Vector3(spawnAreaSize.x, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }
        
        // Vẽ spawned positions nếu đang chạy
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 pos in spawnedPositions)
            {
                Gizmos.DrawSphere(pos, 0.3f);
            }
        }
    }
}

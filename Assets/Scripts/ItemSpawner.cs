using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableItem
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float spawnChance = 50f;  // Tỉ lệ spawn (%)
        public int minAmount = 1;
        public int maxAmount = 3;
    }

    [Header("Spawn Settings")]
    [SerializeField] private List<SpawnableItem> spawnableItems = new List<SpawnableItem>();
    [SerializeField] private int totalItemsToSpawn = 10;
    
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
        
        int itemsSpawned = 0;
        int maxAttempts = totalItemsToSpawn * 10; // Tránh vòng lặp vô hạn
        int attempts = 0;
        
        while (itemsSpawned < totalItemsToSpawn && attempts < maxAttempts)
        {
            attempts++;
            
            // Chọn random item để spawn
            SpawnableItem itemToSpawn = GetRandomItem();
            
            if (itemToSpawn != null && itemToSpawn.itemPrefab != null)
            {
                Vector3 spawnPos = GetSpawnPosition();
                
                if (spawnPos != Vector3.zero && IsPositionValid(spawnPos))
                {
                    SpawnItem(itemToSpawn.itemPrefab, spawnPos);
                    spawnedPositions.Add(spawnPos);
                    itemsSpawned++;
                }
            }
        }
        
        Debug.Log($"ItemSpawner: Spawned {itemsSpawned} items in {attempts} attempts");
    }

    SpawnableItem GetRandomItem()
    {
        if (spawnableItems.Count == 0) return null;
        
        // Tính tổng spawn chance
        float totalChance = 0f;
        foreach (var item in spawnableItems)
        {
            totalChance += item.spawnChance;
        }
        
        // Random theo tỉ lệ
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;
        
        foreach (var item in spawnableItems)
        {
            currentChance += item.spawnChance;
            if (randomValue <= currentChance)
            {
                return item;
            }
        }
        
        return spawnableItems[0];
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

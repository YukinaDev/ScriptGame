using UnityEngine;
using System.Collections.Generic;

public class WarehouseMapGenerator : MonoBehaviour
{
    private static WarehouseMapGenerator instance;
    public static WarehouseMapGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WarehouseMapGenerator>();
                if (instance == null)
                {
                    GameObject go = new GameObject("WarehouseMapGenerator");
                    instance = go.AddComponent<WarehouseMapGenerator>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Generation Control")]
    public bool regenerateOnNewGame = true;
    private bool mapGenerated = false;
    [Header("Map Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public float tileSize = 1f;

    [Header("Colors")]
    public Color floorColor = new Color(0.4f, 0.4f, 0.4f);
    public Color wallColor = new Color(0.3f, 0.2f, 0.1f);
    public Color crateColor = new Color(0.6f, 0.4f, 0.2f);
    public Color boxColor = new Color(0.7f, 0.5f, 0.3f);
    public Color cabinetColor = new Color(0.5f, 0.5f, 0.5f);
    public Color tableColor = new Color(0.4f, 0.3f, 0.2f);
    public Color shelfColor = new Color(0.55f, 0.45f, 0.35f);

    [Header("Generation Settings")]
    [Range(0f, 1f)] public float obstacleChance = 0.15f;
    [Range(0f, 1f)] public float crateChance = 0.35f;
    [Range(0f, 1f)] public float boxChance = 0.30f;
    [Range(0f, 1f)] public float cabinetChance = 0.15f;
    [Range(0f, 1f)] public float tableChance = 0.10f;
    [Range(0f, 1f)] public float shelfChance = 0.10f;

    [Header("Room Settings")]
    public int minRoomSize = 5;
    public int maxRoomSize = 12;
    public int numberOfRooms = 8;
    public float roomHeight = 4f;
    public bool createCeiling = true;
    public bool createLights = true;
    public Color lightColor = Color.white;
    public float lightIntensity = 2f;

    [Header("Player Settings")]
    public bool centerMapOnPlayer = true;
    public bool movePlayerToSpawn = true;
    
    [Header("Item Spawning Integration")]
    public bool createItemSpawnPoints = true;
    public int spawnPointsPerRoom = 3;
    public float minDistanceFromWalls = 1.5f;

    private int[,] mapGrid;
    private List<Room> rooms = new List<Room>();
    private Transform mapParent;
    private Vector3 playerPosition;
    private GameObject player;
    private List<Vector3> validSpawnPositions = new List<Vector3>();

    private enum TileType
    {
        Empty = 0,
        Floor = 1,
        Wall = 2,
        Obstacle = 3
    }

    private class Room
    {
        public int x, y, width, height;

        public Room(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2Int Center()
        {
            return new Vector2Int(x + width / 2, y + height / 2);
        }

        public bool Intersects(Room other)
        {
            return x < other.x + other.width + 2 &&
                   x + width + 2 > other.x &&
                   y < other.y + other.height + 2 &&
                   y + height + 2 > other.y;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (!mapGenerated || regenerateOnNewGame)
        {
            GenerateMap();
            mapGenerated = true;
        }
        else
        {
            FindPlayerPosition();
            RepositionMapForPlayer();
            CreateSpawnMarker();
        }
    }

    public void GenerateMap()
    {
        if (mapParent != null)
            DestroyImmediate(mapParent.gameObject);

        FindPlayerPosition();

        mapParent = new GameObject("WarehouseMap").transform;
        mapGrid = new int[mapWidth, mapHeight];
        rooms.Clear();

        InitializeMap();
        GenerateRooms();
        ConnectRooms();
        BuildWalls();
        PlaceObjects();
        InstantiateMap();
        CreateCeilings();
        CreateRoomLights();
        if (createItemSpawnPoints)
            GenerateItemSpawnPoints();
        CreateSpawnMarker();
    }

    void RepositionMapForPlayer()
    {
        if (mapParent == null) return;

        FindPlayerPosition();

        Vector3 spawnPointLocal = Vector3.zero;
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center();
            spawnPointLocal = new Vector3(center.x * tileSize, 0, center.y * tileSize);
        }

        Vector3 offset = playerPosition - spawnPointLocal;
        mapParent.position = offset;
    }

    public void RegenerateNewMap()
    {
        mapGenerated = false;
        GenerateMap();
        mapGenerated = true;
    }

    void GenerateItemSpawnPoints()
    {
        validSpawnPositions.Clear();
        
        Vector3 spawnPointLocal = Vector3.zero;
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center();
            spawnPointLocal = new Vector3(center.x * tileSize, 0, center.y * tileSize);
        }

        Vector3 offset = playerPosition - spawnPointLocal;
        GameObject spawnPointsParent = new GameObject("ItemSpawnPoints");
        spawnPointsParent.transform.SetParent(mapParent);

        foreach (Room room in rooms)
        {
            GenerateSpawnPointsInRoom(room, offset, spawnPointsParent.transform);
        }
        
        Debug.Log($"Generated {validSpawnPositions.Count} valid spawn points across {rooms.Count} rooms");
    }

    void GenerateSpawnPointsInRoom(Room room, Vector3 offset, Transform parent)
    {
        int pointsGenerated = 0;
        int maxAttempts = spawnPointsPerRoom * 10;
        int attempts = 0;

        while (pointsGenerated < spawnPointsPerRoom && attempts < maxAttempts)
        {
            attempts++;

            // Random vị trí trong phòng
            int x = Random.Range(room.x + Mathf.CeilToInt(minDistanceFromWalls), 
                                 room.x + room.width - Mathf.CeilToInt(minDistanceFromWalls));
            int y = Random.Range(room.y + Mathf.CeilToInt(minDistanceFromWalls), 
                                 room.y + room.height - Mathf.CeilToInt(minDistanceFromWalls));

            // Kiểm tra có phải là floor không và không có obstacle
            if (mapGrid[x, y] == (int)TileType.Floor)
            {
                Vector3 worldPos = new Vector3(x * tileSize, 0, y * tileSize) + offset;
                worldPos.y += 0.5f; // Độ cao từ sàn

                // Kiểm tra khoảng cách với các điểm đã có
                bool farEnough = true;
                foreach (Vector3 existingPos in validSpawnPositions)
                {
                    if (Vector3.Distance(worldPos, existingPos) < minDistanceFromWalls * tileSize)
                    {
                        farEnough = false;
                        break;
                    }
                }

                if (farEnough)
                {
                    // Tạo SpawnPoint object
                    GameObject spawnPointObj = new GameObject($"SpawnPoint_R{rooms.IndexOf(room)}_{pointsGenerated}");
                    spawnPointObj.transform.position = worldPos;
                    spawnPointObj.transform.SetParent(parent);
                    
                    SpawnPoint spawnPoint = spawnPointObj.AddComponent<SpawnPoint>();
                    spawnPoint.heightOffset = 0f; // Đã đặt đúng vị trí
                    spawnPoint.gizmoColor = Color.magenta;
                    spawnPoint.gizmoRadius = 0.3f;
                    
                    validSpawnPositions.Add(worldPos);
                    pointsGenerated++;
                }
            }
        }
    }

    public List<Vector3> GetValidSpawnPositions()
    {
        return new List<Vector3>(validSpawnPositions);
    }

    void InitializeMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapGrid[x, y] = (int)TileType.Empty;
            }
        }
    }

    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize);
            int roomX = Random.Range(1, mapWidth - roomWidth - 1);
            int roomY = Random.Range(1, mapHeight - roomHeight - 1);

            Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);

            bool intersects = false;
            foreach (Room room in rooms)
            {
                if (newRoom.Intersects(room))
                {
                    intersects = true;
                    break;
                }
            }

            if (!intersects)
            {
                CreateRoom(newRoom);
                rooms.Add(newRoom);
            }
        }
    }

    void CreateRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                mapGrid[x, y] = (int)TileType.Floor;
            }
        }
    }

    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int currentCenter = rooms[i].Center();
            Vector2Int nextCenter = rooms[i + 1].Center();

            if (Random.value > 0.5f)
            {
                CreateHorizontalCorridor(currentCenter.x, nextCenter.x, currentCenter.y);
                CreateVerticalCorridor(currentCenter.y, nextCenter.y, nextCenter.x);
            }
            else
            {
                CreateVerticalCorridor(currentCenter.y, nextCenter.y, currentCenter.x);
                CreateHorizontalCorridor(currentCenter.x, nextCenter.x, nextCenter.y);
            }
        }
    }

    void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            if (x > 0 && x < mapWidth && y > 0 && y < mapHeight)
            {
                mapGrid[x, y] = (int)TileType.Floor;
                if (y - 1 > 0) mapGrid[x, y - 1] = (int)TileType.Floor;
                if (y + 1 < mapHeight) mapGrid[x, y + 1] = (int)TileType.Floor;
            }
        }
    }

    void CreateVerticalCorridor(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            if (x > 0 && x < mapWidth && y > 0 && y < mapHeight)
            {
                mapGrid[x, y] = (int)TileType.Floor;
                if (x - 1 > 0) mapGrid[x - 1, y] = (int)TileType.Floor;
                if (x + 1 < mapWidth) mapGrid[x + 1, y] = (int)TileType.Floor;
            }
        }
    }

    void BuildWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapGrid[x, y] == (int)TileType.Floor)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                            {
                                if (mapGrid[nx, ny] == (int)TileType.Empty)
                                {
                                    mapGrid[nx, ny] = (int)TileType.Wall;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void PlaceObjects()
    {
        foreach (Room room in rooms)
        {
            for (int x = room.x + 1; x < room.x + room.width - 1; x++)
            {
                for (int y = room.y + 1; y < room.y + room.height - 1; y++)
                {
                    if (mapGrid[x, y] == (int)TileType.Floor && Random.value < obstacleChance)
                    {
                        mapGrid[x, y] = (int)TileType.Obstacle;
                    }
                }
            }
        }
    }

    void InstantiateMap()
    {
        Transform floorParent = new GameObject("Floor").transform;
        floorParent.SetParent(mapParent);

        Transform wallParent = new GameObject("Walls").transform;
        wallParent.SetParent(mapParent);

        Transform obstacleParent = new GameObject("Obstacles").transform;
        obstacleParent.SetParent(mapParent);

        Vector3 spawnPointLocal = Vector3.zero;
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center();
            spawnPointLocal = new Vector3(center.x * tileSize, 0, center.y * tileSize);
        }

        Vector3 offset = playerPosition - spawnPointLocal;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize) + offset;

                if (mapGrid[x, y] == (int)TileType.Floor || mapGrid[x, y] == (int)TileType.Obstacle)
                {
                    CreateFloorTile(position, floorParent);
                    
                    if (mapGrid[x, y] == (int)TileType.Obstacle)
                    {
                        CreateRandomObstacle(position, obstacleParent);
                    }
                }
                else if (mapGrid[x, y] == (int)TileType.Wall)
                {
                    CreateWall(position, wallParent);
                }
            }
        }
    }

    void CreateFloorTile(Vector3 position, Transform parent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = position + Vector3.down * 0.5f;
        floor.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
        floor.transform.SetParent(parent);
        floor.GetComponent<Renderer>().material.color = floorColor;
        floor.layer = LayerMask.NameToLayer("Ground");
    }

    void CreateWall(Vector3 position, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.position = position + Vector3.up * (roomHeight / 2f - 0.5f);
        wall.transform.localScale = new Vector3(tileSize, roomHeight, tileSize);
        wall.transform.SetParent(parent);
        wall.GetComponent<Renderer>().material.color = wallColor;
        wall.layer = LayerMask.NameToLayer("Default");
    }

    void CreateRandomObstacle(Vector3 position, Transform parent)
    {
        float random = Random.value;
        float total = crateChance + boxChance + cabinetChance + tableChance + shelfChance;

        if (random < crateChance / total)
        {
            CreateCrate(position, parent);
        }
        else if (random < (crateChance + boxChance) / total)
        {
            CreateBox(position, parent);
        }
        else if (random < (crateChance + boxChance + cabinetChance) / total)
        {
            CreateCabinet(position, parent);
        }
        else if (random < (crateChance + boxChance + cabinetChance + tableChance) / total)
        {
            CreateTable(position, parent);
        }
        else
        {
            CreateShelf(position, parent);
        }
    }

    void CreateCrate(Vector3 position, Transform parent)
    {
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        crate.transform.position = position + Vector3.up * (-0.5f + 0.4f);
        crate.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        crate.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        crate.transform.SetParent(parent);
        crate.GetComponent<Renderer>().material.color = crateColor;
    }

    void CreateBox(Vector3 position, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Box";
        box.transform.position = position + Vector3.up * (-0.5f + 0.3f);
        box.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        box.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), Random.Range(-5, 5));
        box.transform.SetParent(parent);
        box.GetComponent<Renderer>().material.color = boxColor;
    }

    void CreateCabinet(Vector3 position, Transform parent)
    {
        GameObject cabinet = new GameObject("Cabinet");
        cabinet.transform.position = position + Vector3.up * (-0.5f);
        cabinet.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
        cabinet.transform.SetParent(parent);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(cabinet.transform);
        body.transform.localPosition = Vector3.up * 0.8f;
        body.transform.localScale = new Vector3(0.8f, 1.6f, 0.4f);
        body.GetComponent<Renderer>().material.color = cabinetColor;

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(cabinet.transform);
        door.transform.localPosition = new Vector3(0, 0.8f, -0.22f);
        door.transform.localScale = new Vector3(0.75f, 1.5f, 0.05f);
        door.GetComponent<Renderer>().material.color = cabinetColor * 0.8f;
    }

    void CreateTable(Vector3 position, Transform parent)
    {
        GameObject table = new GameObject("Table");
        table.transform.position = position + Vector3.up * (-0.5f);
        table.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
        table.transform.SetParent(parent);

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "TableTop";
        top.transform.SetParent(table.transform);
        top.transform.localPosition = Vector3.up * 0.7f;
        top.transform.localScale = new Vector3(0.9f, 0.1f, 0.6f);
        top.GetComponent<Renderer>().material.color = tableColor;

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg" + i;
            leg.transform.SetParent(table.transform);
            float x = (i % 2 == 0) ? -0.35f : 0.35f;
            float z = (i < 2) ? -0.2f : 0.2f;
            leg.transform.localPosition = new Vector3(x, 0.35f, z);
            leg.transform.localScale = new Vector3(0.08f, 0.7f, 0.08f);
            leg.GetComponent<Renderer>().material.color = tableColor * 0.7f;
        }
    }

    void CreateShelf(Vector3 position, Transform parent)
    {
        GameObject shelf = new GameObject("Shelf");
        shelf.transform.position = position + Vector3.up * (-0.5f);
        shelf.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
        shelf.transform.SetParent(parent);

        for (int i = 0; i < 3; i++)
        {
            GameObject level = GameObject.CreatePrimitive(PrimitiveType.Cube);
            level.name = "Level" + i;
            level.transform.SetParent(shelf.transform);
            level.transform.localPosition = Vector3.up * (0.5f + i * 0.6f);
            level.transform.localScale = new Vector3(0.9f, 0.05f, 0.4f);
            level.GetComponent<Renderer>().material.color = shelfColor;
        }

        GameObject leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPost.name = "LeftPost";
        leftPost.transform.SetParent(shelf.transform);
        leftPost.transform.localPosition = new Vector3(-0.4f, 1.0f, 0);
        leftPost.transform.localScale = new Vector3(0.08f, 2.0f, 0.4f);
        leftPost.GetComponent<Renderer>().material.color = shelfColor * 0.8f;

        GameObject rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPost.name = "RightPost";
        rightPost.transform.SetParent(shelf.transform);
        rightPost.transform.localPosition = new Vector3(0.4f, 1.0f, 0);
        rightPost.transform.localScale = new Vector3(0.08f, 2.0f, 0.4f);
        rightPost.GetComponent<Renderer>().material.color = shelfColor * 0.8f;
    }

    void FindPlayerPosition()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null && centerMapOnPlayer)
        {
            playerPosition = player.transform.position;
            Debug.Log("Found player at: " + playerPosition);
        }
        else
        {
            playerPosition = Vector3.zero;
            if (player == null)
                Debug.Log("No player found with 'Player' tag, generating map at origin");
        }
    }

    void CreateCeilings()
    {
        if (!createCeiling) return;

        Transform ceilingParent = new GameObject("Ceilings").transform;
        ceilingParent.SetParent(mapParent);

        Vector3 spawnPointLocal = Vector3.zero;
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center();
            spawnPointLocal = new Vector3(center.x * tileSize, 0, center.y * tileSize);
        }

        Vector3 offset = playerPosition - spawnPointLocal;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapGrid[x, y] == (int)TileType.Floor || mapGrid[x, y] == (int)TileType.Obstacle)
                {
                    Vector3 position = new Vector3(x * tileSize, roomHeight - 0.55f, y * tileSize) + offset;
                    
                    GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ceiling.name = "Ceiling";
                    ceiling.transform.position = position;
                    ceiling.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);
                    ceiling.transform.SetParent(ceilingParent);
                    ceiling.GetComponent<Renderer>().material.color = wallColor * 0.8f;
                }
            }
        }
    }

    void CreateRoomLights()
    {
        if (!createLights) return;

        Transform lightsParent = new GameObject("Lights").transform;
        lightsParent.SetParent(mapParent);

        Vector3 spawnPointLocal = Vector3.zero;
        if (rooms.Count > 0)
        {
            Vector2Int center = rooms[0].Center();
            spawnPointLocal = new Vector3(center.x * tileSize, 0, center.y * tileSize);
        }

        Vector3 offset = playerPosition - spawnPointLocal;

        foreach (Room room in rooms)
        {
            Vector3 lightPosition = new Vector3(
                (room.x + room.width / 2f) * tileSize,
                roomHeight - 0.5f,
                (room.y + room.height / 2f) * tileSize
            ) + offset;

            GameObject lightObj = new GameObject("RoomLight");
            lightObj.transform.position = lightPosition;
            lightObj.transform.SetParent(lightsParent);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.intensity = lightIntensity;
            light.range = Mathf.Max(room.width, room.height) * tileSize * 0.8f;
            light.shadows = LightShadows.Soft;
            light.renderMode = LightRenderMode.ForcePixel;
        }
    }

    void CreateSpawnMarker()
    {
        if (rooms.Count == 0) return;

        Vector3 spawnPosition = playerPosition;
        spawnPosition.y = 0.05f;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "SpawnPoint";
        marker.transform.position = spawnPosition;
        marker.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        marker.GetComponent<Renderer>().material.color = Color.red;
        
        DestroyImmediate(marker.GetComponent<Collider>());

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.position = spawnPosition + Vector3.up * 1f;
        pole.transform.localScale = new Vector3(0.05f, 1f, 0.05f);
        pole.GetComponent<Renderer>().material.color = Color.yellow;
        pole.transform.SetParent(marker.transform);
        
        DestroyImmediate(pole.GetComponent<Collider>());

        marker.transform.SetParent(mapParent);

        if (player != null && movePlayerToSpawn)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = new Vector3(playerPosition.x, playerPosition.y + 0.5f, playerPosition.z);
                cc.enabled = true;
            }
            else
            {
                player.transform.position = new Vector3(playerPosition.x, playerPosition.y + 0.5f, playerPosition.z);
            }
            Debug.Log("Player moved to spawn point: " + player.transform.position);
        }
    }
}

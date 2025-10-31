using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
            }
            return instance;
        }
    }

    [Header("Player Data")]
    public PlayerData playerData;

    [Header("Scene Management")]
    public string hubSceneName = "HubScene";
    public bool isInHub = true;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (playerData == null)
        {
            playerData = new PlayerData();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isInHub = (scene.name == hubSceneName);
        
        if (!isInHub)
        {
            RestorePlayerState();
        }
    }

    public void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        StaminaSystem stamina = player.GetComponent<StaminaSystem>();
        if (stamina != null)
        {
            playerData.currentStamina = stamina.currentStamina;
            playerData.maxStamina = stamina.maxStamina;
        }

        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory != null)
        {
            playerData.inventoryItems.Clear();
            playerData.currentSlot = inventory.currentSlot;
            
            for (int i = 0; i < inventory.inventory.Length; i++)
            {
                if (inventory.inventory[i] != null)
                {
                    playerData.inventoryItems.Add(inventory.inventory[i].name.Replace("(Clone)", "").Trim());
                }
                else
                {
                    playerData.inventoryItems.Add("");
                }
            }
        }

        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        if (controller != null)
        {
            playerData.mouseSensitivity = controller.mouseSensitivity;
        }

        playerData.lastScene = SceneManager.GetActiveScene().name;
        playerData.lastPosition = player.transform.position;
    }

    public void RestorePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene!");
            return;
        }

        StaminaSystem stamina = player.GetComponent<StaminaSystem>();
        if (stamina != null)
        {
            stamina.currentStamina = playerData.currentStamina;
            stamina.maxStamina = playerData.maxStamina;
        }

        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.mouseSensitivity = playerData.mouseSensitivity;
        }
    }

    public void LoadHouseScene(string sceneName)
    {
        SavePlayerState();
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToHub()
    {
        SavePlayerState();
        SceneManager.LoadScene(hubSceneName);
    }

    public void NewGame()
    {
        playerData = new PlayerData();
        SceneManager.LoadScene(hubSceneName);
    }

    public void SaveGame()
    {
        SavePlayerState();
        
        string json = JsonUtility.ToJson(playerData, true);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved!");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            playerData = JsonUtility.FromJson<PlayerData>(json);
            
            SceneManager.LoadScene(playerData.lastScene);
            Debug.Log("Game loaded!");
        }
        else
        {
            Debug.LogWarning("No save data found!");
        }
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey("SaveData");
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey("SaveData");
        Debug.Log("Save data deleted!");
    }
}

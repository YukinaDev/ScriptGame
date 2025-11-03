using UnityEngine;

public class FlashlightItem : MonoBehaviour, IInteractable
{
    [Header("Flashlight Item Info")]
    public string itemName = "Flashlight";
    public Sprite itemIcon; // Icon hiện trong inventory
    [Tooltip("Pin khi nhặt (%)")]
    public float initialBattery = 100f;
    
    [Header("Visual Effects")]
    public bool rotateItem = true;
    public float rotationSpeed = 50f;
    public bool bobUpDown = true;
    public float bobSpeed = 1f;
    public float bobHeight = 0.3f;
    
    [Header("Audio (Optional)")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    
    [Header("Particle Effect (Optional)")]
    public GameObject pickupParticle;
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Xoay item
        if (rotateItem)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        // Bob lên xuống
        if (bobUpDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public string GetInteractPrompt()
    {
        return $"Pick up {itemName}";
    }

    public void Interact(GameObject player)
    {
        Debug.Log("[FlashlightItem] Interact called!");
        
        // Tìm Flashlight component
        Flashlight flashlight = player.GetComponentInChildren<Flashlight>();
        
        if (flashlight == null)
        {
            Debug.LogError("[FlashlightItem] Player doesn't have Flashlight component!");
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("Cannot equip flashlight!");
            }
            return;
        }

        Debug.Log($"[FlashlightItem] Found Flashlight. startWithFlashlight={flashlight.startWithFlashlight}");

        // Kiểm tra đã có đèn pin chưa
        if (flashlight.startWithFlashlight)
        {
            Debug.LogWarning("[FlashlightItem] Already have flashlight!");
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("Already have a flashlight!");
            }
            return;
        }

        // Enable flashlight
        Debug.Log("[FlashlightItem] Enabling flashlight...");
        flashlight.EnableFlashlight();
        
        // Set pin ban đầu
        if (flashlight.useBattery)
        {
            flashlight.currentBattery = initialBattery;
            Debug.Log($"[FlashlightItem] Set battery to {initialBattery}%");
        }
        
        // Hiện battery bar
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("[FlashlightItem] Calling ShowBatteryBar()");
            uiManager.ShowBatteryBar();
        }
        else
        {
            Debug.LogError("[FlashlightItem] UIManager not found!");
        }
        
        // Thêm vào inventory
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory != null)
        {
            bool added = inventory.AddItem(gameObject);
            Debug.Log($"[FlashlightItem] Added to inventory: {added}");
            
            if (!added)
            {
                if (MessageDisplay.Instance != null)
                {
                    MessageDisplay.Instance.ShowMessage("Inventory full!");
                }
                return; // Không destroy nếu inventory đầy
            }
            
            // Register với GameDataManager
            UniqueID uid = GetComponent<UniqueID>();
            if (uid != null && GameDataManager.Instance != null)
            {
                GameDataManager.Instance.RegisterItemPickup(uid.ID);
            }
        }
        
        // Hiện message
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowMessage($"Picked up {itemName}! Press F to toggle");
        }
        
        Debug.Log("[FlashlightItem] Pickup complete!");
        
        // Item sẽ được ẩn bởi InventorySystem.AddItem(), không destroy

        // Phát âm thanh
        if (pickupSound != null && audioSource != null)
        {
            // Tạo temporary audio source
            GameObject tempAudio = new GameObject("TempAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = pickupSound;
            tempSource.volume = audioSource.volume;
            tempSource.Play();
            Destroy(tempAudio, pickupSound.length);
        }

        // Spawn particle effect
        if (pickupParticle != null)
        {
            GameObject particle = Instantiate(pickupParticle, transform.position, Quaternion.identity);
            Destroy(particle, 2f);
        }

        // KHÔNG destroy - inventory sẽ quản lý
    }

    void OnDrawGizmos()
    {
        // Vẽ icon trong Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Vẽ ray chỉ hướng lên (giống đèn pin)
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);
    }
}

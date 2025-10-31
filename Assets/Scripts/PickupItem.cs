using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    public string itemName = "Item";
    public Sprite itemIcon;
    
    [Header("Visual Effects")]
    public bool rotateItem = true;
    public float rotationSpeed = 50f;
    public bool bobUpDown = true;
    public float bobSpeed = 1f;
    public float bobHeight = 0.3f;
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
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

    // Implement IInteractable
    public string GetInteractPrompt()
    {
        return $"Pick up {itemName}";
    }

    public void Interact(GameObject player)
    {
        // Kiểm tra inventory
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        
        if (inventory == null)
        {
            Debug.LogError("Player doesn't have InventorySystem!");
            return;
        }
        
        // Kiểm tra inventory đã đầy chưa
        if (inventory.IsFull())
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("Inventory is full!");
            }
            Debug.Log("Cannot pick up: Inventory is full!");
            return;
        }
        
        // Thêm item vào inventory
        bool success = inventory.AddItem(gameObject);
        
        if (success)
        {
            Debug.Log($"Picked up: {itemName}");
            
            // Hiện message
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage($"Picked up {itemName}");
            }
            
            // Item sẽ được ẩn bởi InventorySystem.AddItem()
            // KHÔNG destroy ngay, vì cần giữ reference trong inventory
        }
        else
        {
            Debug.LogWarning($"Failed to add {itemName} to inventory");
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 4;
    public GameObject[] inventory;
    public int currentSlot = 0;

    [Header("Item Hold Position")]
    public Transform itemHoldPosition;

    [Header("UI Reference")]
    public UIManager uiManager;
    
    [Header("Drop Settings")]
    public float dropDistance = 2f;  // Khoảng cách drop phía trước
    public float dropForce = 3f;     // Lực ném item ra

    void Start()
    {
        inventory = new GameObject[maxSlots];
        
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    void Update()
    {
        HandleSlotSelection();
        HandleDropItem();
    }

    void HandleSlotSelection()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            
            if (scroll > 0f)
            {
                SelectSlot((currentSlot + 1) % maxSlots);
            }
            else if (scroll < 0f)
            {
                SelectSlot((currentSlot - 1 + maxSlots) % maxSlots);
            }
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;

        currentSlot = slotIndex;
        UpdateItemDisplay();
        
        if (uiManager != null)
        {
            uiManager.HighlightInventorySlot(currentSlot);
        }
    }

    public bool AddItem(GameObject item)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = item;
                item.SetActive(false);
                SelectSlot(i);
                UpdateInventoryUI();  // Cập nhật UI
                return true;
            }
        }
        Debug.Log("Inventory is full!");
        return false;
    }

    public GameObject GetCurrentItem()
    {
        return inventory[currentSlot];
    }

    public void RemoveCurrentItem()
    {
        inventory[currentSlot] = null;
        UpdateItemDisplay();
        UpdateInventoryUI();  // Cập nhật UI
    }
    
    void HandleDropItem()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            DropCurrentItem();
        }
    }
    
    public void DropCurrentItem()
    {
        if (inventory[currentSlot] == null)
        {
            Debug.Log("[InventorySystem] No item to drop in current slot");
            return;
        }
        
        GameObject itemToDrop = inventory[currentSlot];
        
        // Tính vị trí drop (phía trước player)
        Vector3 dropPosition = transform.position + transform.forward * dropDistance;
        dropPosition.y += 1f;  // Nâng cao một chút để không spawn trong đất
        
        // Active item và đặt vào world
        itemToDrop.SetActive(true);
        itemToDrop.transform.SetParent(null);  // Bỏ parent khỏi player
        itemToDrop.transform.position = dropPosition;
        itemToDrop.transform.rotation = Quaternion.identity;
        
        // Thêm lực ném ra phía trước (nếu có Rigidbody)
        Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;  // Reset velocity cũ
            rb.angularVelocity = Vector3.zero; // Reset rotation cũ
            
            // Tăng drag để item dừng lại nhanh
            rb.linearDamping = 2f;   // Drag cho velocity
            rb.angularDamping = 5f;  // Drag cho rotation
            
            rb.AddForce(transform.forward * dropForce + Vector3.up * 2f, ForceMode.VelocityChange);
        }
        
        // Reset PickupItem để có thể nhặt lại
        PickupItem pickupItem = itemToDrop.GetComponent<PickupItem>();
        if (pickupItem != null)
        {
            // Re-enable các animation effects
            pickupItem.enabled = true;
        }
        
        // Bật lại Collider và tắt Trigger để va chạm với đất
        Collider col = itemToDrop.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            // Tạm thời tắt trigger để item va chạm với đất và dừng lại
            col.isTrigger = false;
            
            // Sau 1 giây, bật lại trigger để có thể nhặt
            StartCoroutine(EnableTriggerAfterDelay(col, 1f));
        }
        
        // Xóa khỏi inventory
        inventory[currentSlot] = null;
        UpdateItemDisplay();
        UpdateInventoryUI();
        
        Debug.Log($"[InventorySystem] Dropped item: {itemToDrop.name}");
        
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowMessage($"Dropped {pickupItem?.itemName ?? itemToDrop.name}");
        }
    }

    public void UpdateItemDisplay()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (inventory[i] != null)
            {
                if (i == currentSlot)
                {
                    inventory[i].SetActive(true);
                    inventory[i].transform.SetParent(itemHoldPosition);
                    inventory[i].transform.localPosition = Vector3.zero;
                    inventory[i].transform.localRotation = Quaternion.identity;
                }
                else
                {
                    inventory[i].SetActive(false);
                }
            }
        }
    }

    public bool IsFull()
    {
        foreach (GameObject item in inventory)
        {
            if (item == null) return false;
        }
        return true;
    }

    void UpdateInventoryUI()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[InventorySystem] UIManager is NULL! Cannot update inventory UI.");
            return;
        }

        Debug.Log("[InventorySystem] Updating Inventory UI...");

        for (int i = 0; i < maxSlots; i++)
        {
            if (i < inventory.Length && inventory[i] != null)
            {
                // Lấy sprite từ PickupItem component
                PickupItem pickupItem = inventory[i].GetComponent<PickupItem>();
                if (pickupItem != null && pickupItem.itemIcon != null)
                {
                    Debug.Log($"[InventorySystem] Slot {i}: Showing icon for '{pickupItem.itemName}'");
                    uiManager.UpdateInventorySlot(i, pickupItem.itemIcon);
                }
                else
                {
                    Debug.LogWarning($"[InventorySystem] Slot {i}: Item has NO ICON! Item: {inventory[i].name}");
                    uiManager.UpdateInventorySlot(i, null);
                }
            }
            else
            {
                Debug.Log($"[InventorySystem] Slot {i}: Empty");
                uiManager.UpdateInventorySlot(i, null);
            }
        }
    }
    
    IEnumerator EnableTriggerAfterDelay(Collider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"[InventorySystem] Trigger re-enabled for {col.gameObject.name}");
        }
    }
}

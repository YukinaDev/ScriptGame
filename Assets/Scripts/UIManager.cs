using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stamina UI")]
    public Image staminaBarFill;
    public GameObject staminaBarPanel;

    [Header("Inventory UI")]
    public Image[] inventorySlotImages;
    public GameObject inventoryPanel;

    [Header("References")]
    public StaminaSystem staminaSystem;
    public InventorySystem inventorySystem;

    void Start()
    {
        if (staminaSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                staminaSystem = player.GetComponent<StaminaSystem>();
            }
        }

        if (inventorySystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                inventorySystem = player.GetComponent<InventorySystem>();
            }
        }
    }

    void Update()
    {
        UpdateStaminaBar();
    }

    void UpdateStaminaBar()
    {
        if (staminaSystem != null && staminaBarFill != null)
        {
            staminaBarFill.fillAmount = staminaSystem.GetStaminaPercentage();
        }
    }

    public void UpdateInventorySlot(int slotIndex, Sprite itemSprite)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlotImages.Length)
        {
            Debug.LogWarning($"[UIManager] Invalid slot index: {slotIndex}");
            return;
        }

        if (inventorySlotImages[slotIndex] == null)
        {
            Debug.LogError($"[UIManager] Slot {slotIndex} Image is NULL! Not assigned in Inspector!");
            return;
        }

        if (itemSprite != null)
        {
            inventorySlotImages[slotIndex].sprite = itemSprite;
            inventorySlotImages[slotIndex].enabled = true;
            inventorySlotImages[slotIndex].gameObject.SetActive(true);  // Đảm bảo GameObject active
            Debug.Log($"[UIManager] Slot {slotIndex}: Icon ENABLED with sprite '{itemSprite.name}'");
        }
        else
        {
            inventorySlotImages[slotIndex].sprite = null;
            inventorySlotImages[slotIndex].enabled = false;
            Debug.Log($"[UIManager] Slot {slotIndex}: Icon DISABLED (empty slot)");
        }
    }

    public void HighlightInventorySlot(int slotIndex)
    {
        for (int i = 0; i < inventorySlotImages.Length; i++)
        {
            Transform slotTransform = inventorySlotImages[i].transform.parent;
            Image slotBackground = slotTransform.GetComponent<Image>();
            
            if (slotBackground != null)
            {
                if (i == slotIndex)
                {
                    slotBackground.color = new Color(1f, 1f, 0f, 0.5f);
                }
                else
                {
                    slotBackground.color = new Color(1f, 1f, 1f, 0.3f);
                }
            }
        }
    }
}

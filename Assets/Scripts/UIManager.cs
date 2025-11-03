using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stamina UI")]
    public Image staminaBarFill;
    public GameObject staminaBarPanel;
    [Tooltip("Dùng scale thay vì fillAmount (cho Image không phải Filled type)")]
    public bool useScaleForStamina = false;
    [Tooltip("Có thể dùng UI.Text hoặc TextMeshProUGUI")]
    public UnityEngine.UI.Text staminaPercentageText;
    public TMPro.TextMeshProUGUI staminaPercentageTMP;

    [Header("Battery UI")]
    public Image batteryBarFill;
    public GameObject batteryBarPanel;
    [Tooltip("Có thể dùng UI.Text hoặc TextMeshProUGUI")]
    public UnityEngine.UI.Text batteryPercentageText;
    public TMPro.TextMeshProUGUI batteryPercentageTMP;
    [Tooltip("Dùng scale thay vì fillAmount (cho Image không phải Filled type)")]
    public bool useScaleInsteadOfFill = false;

    [Header("Inventory UI")]
    public Image[] inventorySlotImages;
    public GameObject inventoryPanel;

    [Header("References")]
    public StaminaSystem staminaSystem;
    public InventorySystem inventorySystem;
    public Flashlight flashlight;

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

        if (flashlight == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                flashlight = player.GetComponentInChildren<Flashlight>();
            }
        }

        // Ẩn battery panel nếu không dùng pin HOẶC chưa nhặt flashlight
        if (batteryBarPanel != null)
        {
            if (flashlight != null)
            {
                bool shouldShow = flashlight.useBattery && flashlight.startWithFlashlight;
                batteryBarPanel.SetActive(shouldShow);
                Debug.Log($"[UIManager] BatteryPanel: {(shouldShow ? "ACTIVE" : "HIDDEN")} (useBattery={flashlight.useBattery}, hasFlashlight={flashlight.startWithFlashlight})");
            }
            else
            {
                Debug.LogWarning("[UIManager] Flashlight NOT FOUND! Battery panel hidden.");
                batteryBarPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("[UIManager] BatteryBarPanel is NULL! Please assign it in Inspector.");
        }
    }

    void Update()
    {
        UpdateStaminaBar();
        UpdateBatteryBar();
    }

    void UpdateStaminaBar()
    {
        if (staminaSystem != null && staminaBarFill != null)
        {
            float staminaPercent = staminaSystem.GetStaminaPercentage();
            
            // Dùng scale hoặc fillAmount
            if (useScaleForStamina)
            {
                // Scale theo trục X để tạo hiệu ứng giảm dần
                Vector3 scale = staminaBarFill.transform.localScale;
                scale.x = staminaPercent;
                staminaBarFill.transform.localScale = scale;
                
                // Adjust pivot để scale từ trái sang phải
                RectTransform rect = staminaBarFill.GetComponent<RectTransform>();
                if (rect != null && rect.pivot.x != 0)
                {
                    rect.pivot = new Vector2(0, rect.pivot.y); // Pivot bên trái
                }
            }
            else
            {
                // Dùng fillAmount (cần Image Type = Filled)
                staminaBarFill.fillAmount = staminaPercent;
            }
            
            // Đổi màu dựa vào % stamina
            if (staminaPercent > 0.5f)
                staminaBarFill.color = Color.cyan;      // > 50%: Cyan (xanh dương nhạt)
            else if (staminaPercent > 0.2f)
                staminaBarFill.color = Color.yellow;    // 20-50%: Vàng
            else
                staminaBarFill.color = Color.red;       // < 20%: Đỏ
            
            // Update text hiển thị số %
            float percent = staminaPercent * 100f;
            string percentText = $"{Mathf.CeilToInt(percent)}%";
            
            if (staminaPercentageText != null)
            {
                staminaPercentageText.text = percentText;
            }
            
            if (staminaPercentageTMP != null)
            {
                staminaPercentageTMP.text = percentText;
            }
        }
    }

    void UpdateBatteryBar()
    {
        if (flashlight == null)
        {
            Debug.LogWarning("[UIManager] Flashlight is NULL in UpdateBatteryBar!");
            return;
        }
        
        if (!flashlight.useBattery)
        {
            return;
        }

        if (batteryBarFill != null)
        {
            float batteryPercent = flashlight.GetBatteryPercentage();
            
            // Dùng scale hoặc fillAmount
            if (useScaleInsteadOfFill)
            {
                // Scale theo trục X để tạo hiệu ứng giảm dần
                Vector3 scale = batteryBarFill.transform.localScale;
                scale.x = batteryPercent;
                batteryBarFill.transform.localScale = scale;
                
                // Adjust pivot để scale từ trái sang phải
                RectTransform rect = batteryBarFill.GetComponent<RectTransform>();
                if (rect != null && rect.pivot.x != 0)
                {
                    rect.pivot = new Vector2(0, rect.pivot.y); // Pivot bên trái
                }
            }
            else
            {
                // Dùng fillAmount (cần Image Type = Filled)
                batteryBarFill.fillAmount = batteryPercent;
            }
            
            // Debug để check giá trị
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[UIManager] Battery: {flashlight.currentBattery}/{flashlight.maxBattery} = {batteryPercent:F2}");
            }
            
            // Đổi màu dựa vào % pin
            if (batteryPercent > 0.5f)
                batteryBarFill.color = Color.green;
            else if (batteryPercent > 0.2f)
                batteryBarFill.color = Color.yellow;
            else
                batteryBarFill.color = Color.red;
        }
        else
        {
            Debug.LogWarning("[UIManager] BatteryBarFill is NULL! Please assign it in Inspector.");
        }

        // Update text hiển thị số %
        float percent = flashlight.GetBatteryPercentage() * 100f;
        string percentText = $"{Mathf.CeilToInt(percent)}%";
        
        if (batteryPercentageText != null)
        {
            batteryPercentageText.text = percentText;
        }
        
        if (batteryPercentageTMP != null)
        {
            batteryPercentageTMP.text = percentText;
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
    
    public void ShowBatteryBar()
    {
        if (batteryBarPanel != null && flashlight != null && flashlight.useBattery)
        {
            batteryBarPanel.SetActive(true);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public bool isOn = true;
    
    [Header("Pickup Settings")]
    [Tooltip("Player có đèn pin từ đầu? Nếu false, phải nhặt FlashlightItem mới dùng được")]
    public bool startWithFlashlight = true;
    
    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.F;
    
    [Header("Battery Settings (Optional)")]
    public bool useBattery = false;
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float batteryDrainRate = 5f;
    
    [Header("Recharge Settings")]
    [Tooltip("Chỉ sạc pin bằng Battery Items, không tự động sạc")]
    public bool manualRechargeOnly = true;
    
    [Header("Flicker Effect (Optional)")]
    public bool enableFlicker = false;
    public float flickerIntensity = 0.2f;
    public float flickerSpeed = 10f;
    
    private float originalIntensity;
    private float flickerTimer;

    void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if (flashlight != null)
        {
            originalIntensity = flashlight.intensity;
            
            // Nếu không có đèn pin từ đầu → disable
            if (!startWithFlashlight)
            {
                flashlight.enabled = false;
                this.enabled = false; // Disable script
                return;
            }
            
            flashlight.enabled = isOn;
        }

        currentBattery = maxBattery;
    }
    
    // Được gọi khi nhặt FlashlightItem
    public void EnableFlashlight()
    {
        startWithFlashlight = true;
        this.enabled = true;
        
        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
        
        Debug.Log("Flashlight enabled!");
    }

    void Update()
    {
        HandleToggle();
        HandleBattery();
        HandleFlicker();
    }

    void HandleToggle()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        if (useBattery && currentBattery <= 0f)
        {
            isOn = false;
            if (flashlight != null)
                flashlight.enabled = false;
            return;
        }

        isOn = !isOn;
        
        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
    }

    void HandleBattery()
    {
        if (!useBattery) return;

        if (isOn)
        {
            // Drain battery khi đèn bật
            currentBattery -= batteryDrainRate * Time.deltaTime;
            currentBattery = Mathf.Max(currentBattery, 0f);

            if (currentBattery <= 0f)
            {
                isOn = false;
                if (flashlight != null)
                    flashlight.enabled = false;
            }
        }
    }

    void HandleFlicker()
    {
        if (!enableFlicker || !isOn || flashlight == null) return;

        flickerTimer += Time.deltaTime * flickerSpeed;
        float flicker = Mathf.PerlinNoise(flickerTimer, 0f) * flickerIntensity;
        flashlight.intensity = originalIntensity + flicker;

        if (useBattery && currentBattery < 20f)
        {
            float batteryFlicker = (20f - currentBattery) / 20f;
            flashlight.intensity *= (1f - batteryFlicker * 0.5f);
        }
    }

    public float GetBatteryPercentage()
    {
        return currentBattery / maxBattery;
    }

    // Public methods để sạc pin từ items hoặc script khác
    public void RechargeBattery(float amount)
    {
        if (!useBattery) return;
        
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        Debug.Log($"Battery recharged: {currentBattery}/{maxBattery}");
    }

    public void SetBatteryToFull()
    {
        currentBattery = maxBattery;
        Debug.Log("Battery fully charged!");
    }

    public bool IsBatteryLow()
    {
        return currentBattery < maxBattery * 0.2f; // < 20%
    }

    public bool IsBatteryDead()
    {
        return currentBattery <= 0f;
    }
}

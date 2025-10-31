using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public bool isOn = true;
    
    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.F;
    
    [Header("Battery Settings (Optional)")]
    public bool useBattery = false;
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float batteryDrainRate = 5f;
    
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
            flashlight.enabled = isOn;
        }

        currentBattery = maxBattery;
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
        if (!useBattery || !isOn) return;

        currentBattery -= batteryDrainRate * Time.deltaTime;
        currentBattery = Mathf.Max(0f, currentBattery);

        if (currentBattery <= 0f)
        {
            isOn = false;
            if (flashlight != null)
                flashlight.enabled = false;
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
}

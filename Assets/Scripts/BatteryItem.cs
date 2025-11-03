using UnityEngine;

public class BatteryItem : MonoBehaviour, IInteractable
{
    [Header("Battery Info")]
    public string batteryName = "Battery Pack";
    public float chargeAmount = 50f;
    [Tooltip("Sạc đầy pin thay vì thêm số lượng cố định")]
    public bool fullRecharge = false;
    
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
        if (fullRecharge)
        {
            return $"Pick up {batteryName} (Full Charge)";
        }
        else
        {
            return $"Pick up {batteryName} (+{chargeAmount}%)";
        }
    }

    public void Interact(GameObject player)
    {
        // Tìm Flashlight component
        Flashlight flashlight = player.GetComponentInChildren<Flashlight>();
        
        if (flashlight == null)
        {
            Debug.LogWarning("Player doesn't have Flashlight!");
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("No flashlight equipped!");
            }
            return;
        }

        // Kiểm tra đã bật useBattery chưa
        if (!flashlight.useBattery)
        {
            Debug.LogWarning("Flashlight doesn't use battery!");
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("Flashlight doesn't need batteries!");
            }
            return;
        }

        // Kiểm tra pin đã đầy chưa
        if (flashlight.currentBattery >= flashlight.maxBattery)
        {
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage("Battery already full!");
            }
            return;
        }

        // Sạc pin
        if (fullRecharge)
        {
            flashlight.SetBatteryToFull();
            
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage($"{batteryName} used - Battery fully charged!");
            }
        }
        else
        {
            flashlight.RechargeBattery(chargeAmount);
            
            if (MessageDisplay.Instance != null)
            {
                MessageDisplay.Instance.ShowMessage($"{batteryName} used - Battery +{chargeAmount}%");
            }
        }

        // Phát âm thanh
        if (pickupSound != null && audioSource != null)
        {
            // Tạo temporary audio source để phát sound sau khi destroy
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

        // Destroy item
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Vẽ icon trong Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

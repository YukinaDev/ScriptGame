using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light lightSource;
    [SerializeField] private bool enableFlicker = true;
    
    [Header("Flicker Timing")]
    [Tooltip("Thời gian tối thiểu giữa các lần flicker (giây)")]
    [SerializeField] private float minFlickerInterval = 2f;
    [Tooltip("Thời gian tối đa giữa các lần flicker (giây)")]
    [SerializeField] private float maxFlickerInterval = 10f;
    
    [Header("Flicker Duration")]
    [Tooltip("Thời gian tối thiểu của mỗi lần flicker (giây)")]
    [SerializeField] private float minFlickerDuration = 0.05f;
    [Tooltip("Thời gian tối đa của mỗi lần flicker (giây)")]
    [SerializeField] private float maxFlickerDuration = 0.3f;
    
    [Header("Flicker Pattern")]
    [Tooltip("Số lần chớp trong 1 pattern")]
    [SerializeField] private int minFlickerCount = 1;
    [SerializeField] private int maxFlickerCount = 5;
    [Tooltip("Khoảng cách giữa các lần chớp trong pattern")]
    [SerializeField] private float flickerGap = 0.1f;
    
    [Header("Intensity")]
    [SerializeField] private float normalIntensity = 2f;
    [SerializeField] private float flickerIntensity = 0f;
    [Tooltip("Random intensity thay vì tắt hoàn toàn")]
    [SerializeField] private bool randomFlickerIntensity = false;
    [SerializeField] private float minRandomIntensity = 0.2f;
    [SerializeField] private float maxRandomIntensity = 1.5f;
    
    private float nextFlickerTime;
    private bool isFlickering = false;
    private int currentFlickerCount = 0;
    private int targetFlickerCount = 0;

    void Start()
    {
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
            if (lightSource == null)
            {
                Debug.LogError("FlickeringLight: Không tìm thấy Light component!");
                enabled = false;
                return;
            }
        }
        
        normalIntensity = lightSource.intensity;
        ScheduleNextFlicker();
    }

    void Update()
    {
        if (!enableFlicker || lightSource == null)
            return;
        
        if (!isFlickering && Time.time >= nextFlickerTime)
        {
            StartFlicker();
        }
    }

    void ScheduleNextFlicker()
    {
        nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
    }

    void StartFlicker()
    {
        isFlickering = true;
        currentFlickerCount = 0;
        targetFlickerCount = Random.Range(minFlickerCount, maxFlickerCount + 1);
        FlickerOnce();
    }

    void FlickerOnce()
    {
        if (currentFlickerCount >= targetFlickerCount)
        {
            // Kết thúc pattern flicker
            lightSource.intensity = normalIntensity;
            isFlickering = false;
            ScheduleNextFlicker();
            return;
        }

        currentFlickerCount++;
        
        // Tắt đèn hoặc giảm intensity
        float offIntensity = randomFlickerIntensity 
            ? Random.Range(minRandomIntensity, maxRandomIntensity) 
            : flickerIntensity;
        
        StartCoroutine(FlickerCoroutine(offIntensity));
    }

    System.Collections.IEnumerator FlickerCoroutine(float offIntensity)
    {
        // Tắt
        lightSource.intensity = offIntensity;
        
        float duration = Random.Range(minFlickerDuration, maxFlickerDuration);
        yield return new WaitForSeconds(duration);
        
        // Bật lại
        lightSource.intensity = normalIntensity;
        
        yield return new WaitForSeconds(flickerGap);
        
        // Tiếp tục flicker hoặc kết thúc
        FlickerOnce();
    }

    // Public methods để control từ bên ngoài
    public void EnableFlicker(bool enable)
    {
        enableFlicker = enable;
        if (!enable)
        {
            StopAllCoroutines();
            isFlickering = false;
            if (lightSource != null)
                lightSource.intensity = normalIntensity;
        }
    }

    public void SetNormalIntensity(float intensity)
    {
        normalIntensity = intensity;
        if (!isFlickering && lightSource != null)
            lightSource.intensity = normalIntensity;
    }

    public void TriggerFlicker()
    {
        if (enableFlicker && !isFlickering)
        {
            StartFlicker();
        }
    }

    void OnDisable()
    {
        if (lightSource != null)
            lightSource.intensity = normalIntensity;
    }

    void OnValidate()
    {
        if (minFlickerInterval < 0) minFlickerInterval = 0;
        if (maxFlickerInterval < minFlickerInterval) maxFlickerInterval = minFlickerInterval;
        if (minFlickerDuration < 0.01f) minFlickerDuration = 0.01f;
        if (maxFlickerDuration < minFlickerDuration) maxFlickerDuration = minFlickerDuration;
        if (minFlickerCount < 1) minFlickerCount = 1;
        if (maxFlickerCount < minFlickerCount) maxFlickerCount = minFlickerCount;
    }
}

using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    
    [Header("Stamina Drain")]
    public float sprintStaminaDrain = 20f;
    
    [Header("Stamina Regeneration")]
    public float staminaRegenRate = 10f;
    public float regenDelay = 1f;
    
    private float lastDrainTime;
    private bool isDraining = false;

    void Start()
    {
        currentStamina = maxStamina;
        lastDrainTime = Time.time;
    }

    void Update()
    {
        HandleStaminaRegeneration();
    }

    public bool CanSprint()
    {
        return currentStamina > 0f;
    }

    public void DrainStamina()
    {
        if (currentStamina > 0f)
        {
            currentStamina -= sprintStaminaDrain * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            lastDrainTime = Time.time;
            isDraining = true;
        }
    }

    public void StopDraining()
    {
        isDraining = false;
    }

    void HandleStaminaRegeneration()
    {
        if (!isDraining && currentStamina < maxStamina)
        {
            if (Time.time - lastDrainTime >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
}
    
using UnityEngine;
using UnityEngine.InputSystem;

public class FootstepSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip[] runSounds;
    
    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;  // Thời gian giữa các bước đi
    [SerializeField] private float runStepInterval = 0.3f;   // Thời gian giữa các bước chạy
    
    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float walkVolume = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float runVolume = 0.5f;
    
    [Header("References")]
    [SerializeField] private CharacterController controller;
    
    private float stepTimer = 0f;
    private bool isMoving = false;
    private bool isSprinting = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;
        }

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        CheckMovement();
        
        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            
            float currentInterval = isSprinting ? runStepInterval : walkStepInterval;
            
            if (stepTimer >= currentInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void CheckMovement()
    {
        // Kiểm tra player có đang di chuyển không
        if (controller != null)
        {
            isMoving = controller.velocity.magnitude > 0.1f;
        }
        
        // Kiểm tra có đang sprint không
        isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && isMoving;
    }

    void PlayFootstep()
    {
        if (audioSource == null) return;
        
        AudioClip[] sounds = isSprinting ? runSounds : walkSounds;
        
        if (sounds == null || sounds.Length == 0)
        {
            return;
        }
        
        // Random âm thanh từ danh sách
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        
        if (clip != null)
        {
            audioSource.volume = isSprinting ? runVolume : walkVolume;
            audioSource.PlayOneShot(clip);
        }
    }
}

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
    [SerializeField] private FirstPersonController firstPersonController;
    
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

        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }
    }

    void Update()
    {
        CheckMovement();
        
        // Không phát âm thanh khi đang sneak (Alt)
        bool isSneaking = firstPersonController != null && firstPersonController.IsSneaking;
        
        if (isMoving && !isSneaking)
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
            
            // Ngừng phát âm thanh ngay khi buông phím
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void CheckMovement()
    {
        // Kiểm tra WASD có đang được nhấn không
        if (Keyboard.current != null)
        {
            bool pressingMovementKey = 
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.dKey.isPressed;
            
            isMoving = pressingMovementKey;
            
            // Kiểm tra có đang sprint không (Shift + đang di chuyển)
            isSprinting = Keyboard.current.leftShiftKey.isPressed && isMoving;
        }
        else
        {
            isMoving = false;
            isSprinting = false;
        }
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

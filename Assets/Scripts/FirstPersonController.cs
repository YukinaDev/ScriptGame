using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float sneakSpeed = 2.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 89f;  // Gần 90 độ để nhìn thẳng xuống chân

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    [Header("Sound Detection")]
    public float walkSoundIntensity = 8f;
    public float runSoundIntensity = 15f;
    public float sneakSoundIntensity = 3f;
    public float soundEmitInterval = 0.5f;
    private float soundTimer = 0f;

    private CharacterController controller;
    private StaminaSystem staminaSystem;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraPitch = 0f;
    private bool isSneaking = false;

    public bool IsSneaking => isSneaking;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        staminaSystem = GetComponent<StaminaSystem>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleGravity();
        HandleSoundEmission();
    }

    void HandleMovement()
    {
        // Raycast thay vì CheckSphere để tránh detect chính Player
        isGrounded = Physics.Raycast(transform.position, Vector3.down, controller.height / 2f + 0.2f, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        if (move.magnitude > 1f) move.Normalize();

        // Kiểm tra sneak mode (Ctrl key)
        isSneaking = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;

        bool wantsToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool isMoving = move.magnitude > 0.1f;
        bool canSprint = staminaSystem != null && staminaSystem.CanSprint();
        
        // Không thể sprint khi đang sneak
        bool isSprinting = wantsToSprint && isMoving && canSprint && isGrounded && !isSneaking;
        
        if (isSprinting && staminaSystem != null)
        {
            staminaSystem.DrainStamina();
        }
        else if (staminaSystem != null)
        {
            staminaSystem.StopDraining();
        }
        

        // Chọn tốc độ dựa trên trạng thái
        float currentSpeed = walkSpeed;
        if (isSneaking)
            currentSpeed = sneakSpeed;
        else if (isSprinting)
            currentSpeed = runSpeed;
        
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void HandleMouseLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.02f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.02f;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        // Giới hạn chỉ khi nhìn lên, không giới hạn khi cúi xuống
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, 120f);  // -89 lên, 120 xuống
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool IsMoving()
    {
        return controller.velocity.magnitude > 0.1f;
    }
    
    void HandleSoundEmission()
    {
        if (!isGrounded || !IsMoving()) 
        {
            soundTimer = 0f;
            return;
        }
        
        soundTimer += Time.deltaTime;
        
        if (soundTimer >= soundEmitInterval)
        {
            soundTimer = 0f;
            
            // Determine sound intensity based on movement type
            float intensity = walkSoundIntensity;
            
            if (isSneaking)
            {
                intensity = sneakSoundIntensity;
            }
            else if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                intensity = runSoundIntensity;
            }
            
            // Emit sound to SoundManager
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.EmitSound(transform.position, intensity, "Footstep");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}

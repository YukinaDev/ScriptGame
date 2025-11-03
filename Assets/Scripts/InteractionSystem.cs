using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    
    [Header("References")]
    public Camera playerCamera;
    public InventorySystem inventorySystem;

    private IInteractable currentInteractable;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }
        
        // Debug layer mask
        Debug.Log($"[InteractionSystem] Interactable Layer Mask VALUE: {interactableLayer.value}");
        if (interactableLayer.value == 0)
        {
            Debug.LogError("[InteractionSystem] Layer Mask is NOTHING (0)! Please set Interactable Layer in Inspector!");
        }
    }

    void Update()
    {
        CheckForInteractable();
        HandleInteraction();
    }

    void HandleInteraction()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Debug raycast
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.cyan);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Debug.Log($"[InteractionSystem] Raycast HIT: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                // Debug.Log($"[InteractionSystem] IInteractable found: {interactable.GetType().Name}");
                SetCurrentInteractable(interactable);
                return;
            }
            else
            {
                // Debug.LogWarning($"[InteractionSystem] Object hit but NO IInteractable component!");
            }
        }

        SetCurrentInteractable(null);
    }

    void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;

        if (currentInteractable != null)
        {
            ShowInteractPrompt(currentInteractable.GetInteractPrompt());
        }
        else
        {
            HideInteractPrompt();
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    void ShowInteractPrompt(string message)
    {
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowPrompt(message);
        }
        else
        {
            Debug.Log($"[E] {message}");
        }
    }

    void HideInteractPrompt()
    {
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.Hide();
        }
    }
}

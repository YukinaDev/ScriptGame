using UnityEngine;
using UnityEngine.InputSystem;

public class TestInteraction : MonoBehaviour
{
    [Header("Test Settings")]
    public float testDistance = 5f;
    public LayerMask testLayer;

    void Update()
    {
        // Test E key
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("========== E KEY PRESSED ==========");
            TestPickup();
        }

        // Test Q key
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("========== Q KEY PRESSED ==========");
            TestDrop();
        }
    }

    void TestPickup()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * testDistance, Color.green, 2f);
        
        Debug.Log($"Raycast from: {ray.origin}");
        Debug.Log($"Raycast direction: {ray.direction}");
        Debug.Log($"Raycast distance: {testDistance}");

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, testDistance, testLayer))
        {
            Debug.Log($"HIT OBJECT: {hit.collider.gameObject.name}");
            Debug.Log($"Hit position: {hit.point}");
            Debug.Log($"Hit distance: {hit.distance}");
            Debug.Log($"Object layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"INTERACTABLE FOUND! Type: {interactable.GetType().Name}");
                Debug.Log($"Prompt: {interactable.GetInteractPrompt()}");
            }
            else
            {
                Debug.LogWarning("Object has NO IInteractable component!");
            }

            PickupItem pickupItem = hit.collider.GetComponent<PickupItem>();
            if (pickupItem != null)
            {
                Debug.Log($"PICKUP ITEM FOUND: {pickupItem.itemName}");
            }
        }
        else
        {
            Debug.LogWarning("Raycast HIT NOTHING!");
            Debug.Log("Possible issues:");
            Debug.Log("- No objects in range");
            Debug.Log("- Wrong layer mask");
            Debug.Log("- Items don't have colliders");
        }
    }

    void TestDrop()
    {
        InventorySystem inventory = GetComponent<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogError("No InventorySystem on Player!");
            return;
        }

        Debug.Log($"Current slot: {inventory.currentSlot}");
        GameObject currentItem = inventory.GetCurrentItem();
        
        if (currentItem != null)
        {
            Debug.Log($"Item in current slot: {currentItem.name}");
            Debug.Log("Attempting to drop...");
        }
        else
        {
            Debug.LogWarning("Current slot is EMPTY - nothing to drop!");
        }
    }

    void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * testDistance);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowSystem : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpwardForce = 2f;
    
    [Header("References")]
    public Camera playerCamera;
    public Transform throwPoint;
    public InventorySystem inventorySystem;

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
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ThrowCurrentItem();
        }
    }

    void ThrowCurrentItem()
    {
        GameObject currentItem = inventorySystem.GetCurrentItem();

        if (currentItem == null)
        {
            Debug.Log("No item to throw!");
            return;
        }

        currentItem.SetActive(true);
        currentItem.transform.SetParent(null);
        
        Vector3 throwPosition = throwPoint != null ? throwPoint.position : playerCamera.transform.position;
        currentItem.transform.position = throwPosition;

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = currentItem.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 forceDirection = playerCamera.transform.forward * throwForce + Vector3.up * throwUpwardForce;
        rb.AddForce(forceDirection, ForceMode.Impulse);

        inventorySystem.RemoveCurrentItem();
    }
}

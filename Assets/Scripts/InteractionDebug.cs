using UnityEngine;

public class InteractionDebug : MonoBehaviour
{
    public float rayDistance = 3f;
    public Camera playerCamera;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"IInteractable found! Prompt: {interactable.GetInteractPrompt()}");
            }
            else
            {
                Debug.LogWarning($"Hit object but NO IInteractable component!");
            }
        }
    }
}

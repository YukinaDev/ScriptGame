using UnityEngine;

public interface IInteractable
{
    string GetInteractPrompt();
    void Interact(GameObject player);
}

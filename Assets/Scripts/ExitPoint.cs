using UnityEngine;
using UnityEngine.InputSystem;

public class ExitPoint : MonoBehaviour, IInteractable
{
    [Header("Exit Settings")]
    public string exitMessage = "Press [E] to exit";
    public bool requireCompletion = false;
    public string notCompletedMessage = "Complete all puzzles before exiting";
    
    [Header("Completion Check")]
    public string[] requiredPuzzleIDs;
    
    [Header("House Info")]
    public string houseName = "HouseA";

    public string GetInteractPrompt()
    {
        if (requireCompletion && !CheckCompletion())
        {
            return notCompletedMessage;
        }
        
        return exitMessage;
    }

    public void Interact(GameObject player)
    {
        if (requireCompletion && !CheckCompletion())
        {
            Debug.Log(notCompletedMessage);
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.MarkHouseCompleted(houseName);
        }

        ExitToHub();
    }

    bool CheckCompletion()
    {
        if (GameManager.Instance == null) return true;
        
        if (requiredPuzzleIDs == null || requiredPuzzleIDs.Length == 0)
            return true;

        foreach (string puzzleID in requiredPuzzleIDs)
        {
            if (!GameManager.Instance.playerData.IsPuzzleSolved(puzzleID))
            {
                return false;
            }
        }

        return true;
    }

    void ExitToHub()
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.ReturnToHub();
        }
        else
        {
            Debug.LogError("SceneTransition not found!");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 3f, 0.2f));
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}

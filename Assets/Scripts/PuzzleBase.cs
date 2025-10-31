using UnityEngine;

public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Info")]
    public string puzzleID;
    public string puzzleName = "Puzzle";
    
    [Header("Puzzle State")]
    public bool isSolved = false;
    
    [Header("Rewards (Optional)")]
    public GameObject[] rewardItems;
    public GameObject[] unlockDoors;

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(puzzleID))
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            puzzleID = $"{sceneName}_{gameObject.name}";
        }

        if (GameManager.Instance != null)
        {
            isSolved = GameManager.Instance.playerData.IsPuzzleSolved(puzzleID);
            
            if (isSolved)
            {
                OnPuzzleAlreadySolved();
            }
        }
    }

    protected void SolvePuzzle()
    {
        if (isSolved) return;

        isSolved = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.SetPuzzleSolved(puzzleID, true);
        }

        GiveRewards();
        UnlockDoors();
        OnPuzzleSolved();

        Debug.Log($"Puzzle '{puzzleName}' solved!");
    }

    protected virtual void OnPuzzleSolved()
    {
    }

    protected virtual void OnPuzzleAlreadySolved()
    {
    }

    void GiveRewards()
    {
        if (rewardItems == null || rewardItems.Length == 0) return;

        foreach (GameObject rewardPrefab in rewardItems)
        {
            if (rewardPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up;
                Instantiate(rewardPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    void UnlockDoors()
    {
        if (unlockDoors == null || unlockDoors.Length == 0) return;

        foreach (GameObject doorObj in unlockDoors)
        {
            if (doorObj != null)
            {
                DoorInteraction door = doorObj.GetComponent<DoorInteraction>();
                if (door != null)
                {
                    door.isLocked = false;
                }
            }
        }
    }
}

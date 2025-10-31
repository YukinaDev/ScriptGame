using UnityEngine;

public class LockedDoor : LockableObject
{
    [Header("Door Specific")]
    public string targetSceneName = "";
    public bool isSceneTransition = false;
    
    [Header("Door Animation (Optional)")]
    public Animator doorAnimator;
    public string openAnimationTrigger = "Open";
    
    [Header("Door Movement (Optional)")]
    public Transform doorTransform;
    public Vector3 openPosition;
    public Vector3 closedPosition;
    public float openSpeed = 2f;
    
    private bool isOpening = false;

    protected override void Start()
    {
        base.Start();
        
        if (doorTransform == null)
        {
            doorTransform = transform;
        }
        
        closedPosition = doorTransform.localPosition;
    }

    void Update()
    {
        if (isOpening && !isLocked)
        {
            doorTransform.localPosition = Vector3.Lerp(
                doorTransform.localPosition,
                openPosition,
                Time.deltaTime * openSpeed
            );

            if (Vector3.Distance(doorTransform.localPosition, openPosition) < 0.01f)
            {
                isOpening = false;
            }
        }
    }

    protected override string GetUnlockedPrompt()
    {
        if (isSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            return $"Press [E] to enter";
        }
        return "Press [E] to open door";
    }

    protected override void OnUnlocked()
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(openAnimationTrigger))
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }
        
        if (doorTransform != null)
        {
            isOpening = true;
        }

        Debug.Log("Door unlocked!");
    }

    protected override void OnInteractUnlocked(GameObject player)
    {
        if (isSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            EnterScene();
        }
        else
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        if (!isOpening && doorTransform != null)
        {
            isOpening = true;
        }
    }

    void EnterScene()
    {
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadHouseScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneTransition not found!");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 3f, 0.2f));
        
        if (isSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
    }
}

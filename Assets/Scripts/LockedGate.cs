using UnityEngine;

public class LockedGate : LockableObject
{
    [Header("Gate Movement")]
    public Transform gateTransform;
    public Vector3 openPosition;
    public Vector3 closedPosition;
    public float openSpeed = 1f;
    
    [Header("Gate Animation")]
    public Animator gateAnimator;
    public string openAnimationTrigger = "Open";
    
    [Header("Auto Open When Unlocked")]
    public bool autoOpenWhenUnlocked = true;
    
    private bool isOpening = false;
    private bool isFullyOpen = false;

    protected override void Start()
    {
        base.Start();
        
        if (gateTransform == null)
        {
            gateTransform = transform;
        }
        
        closedPosition = gateTransform.localPosition;
        
        if (!isLocked)
        {
            if (autoOpenWhenUnlocked)
            {
                gateTransform.localPosition = openPosition;
                isFullyOpen = true;
            }
        }
    }

    void Update()
    {
        if (isOpening && !isFullyOpen)
        {
            gateTransform.localPosition = Vector3.Lerp(
                gateTransform.localPosition,
                openPosition,
                Time.deltaTime * openSpeed
            );

            if (Vector3.Distance(gateTransform.localPosition, openPosition) < 0.01f)
            {
                gateTransform.localPosition = openPosition;
                isOpening = false;
                isFullyOpen = true;
            }
        }
    }

    protected override string GetUnlockedPrompt()
    {
        if (isFullyOpen)
        {
            return "Gate is open";
        }
        return "Press [E] to open gate";
    }

    protected override void OnUnlocked()
    {
        if (gateAnimator != null && !string.IsNullOrEmpty(openAnimationTrigger))
        {
            gateAnimator.SetTrigger(openAnimationTrigger);
        }

        if (autoOpenWhenUnlocked)
        {
            OpenGate();
        }

        Debug.Log("Gate unlocked!");
    }

    protected override void OnInteractUnlocked(GameObject player)
    {
        if (!isFullyOpen && !isOpening)
        {
            OpenGate();
        }
    }

    void OpenGate()
    {
        if (gateTransform != null)
        {
            isOpening = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 3f, 0.5f));
        
        if (!Application.isPlaying && gateTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + openPosition, new Vector3(3f, 3f, 0.5f));
            Gizmos.DrawLine(transform.position, transform.position + openPosition);
        }
    }
}

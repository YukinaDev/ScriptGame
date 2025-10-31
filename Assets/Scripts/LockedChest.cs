using UnityEngine;

public class LockedChest : LockableObject
{
    [Header("Chest Contents")]
    public GameObject[] itemsInside;
    public Vector3 spawnOffset = Vector3.up;
    
    [Header("Chest Animation")]
    public Animator chestAnimator;
    public string openAnimationTrigger = "Open";
    
    [Header("Chest Lid (Optional)")]
    public Transform chestLid;
    public Vector3 openRotation = new Vector3(-90, 0, 0);
    public float openSpeed = 2f;
    
    private bool isOpened = false;
    private bool isOpening = false;

    protected override void Start()
    {
        base.Start();
        
        if (!isLocked && saveUnlockState)
        {
            isOpened = true;
            if (chestLid != null)
            {
                chestLid.localEulerAngles = openRotation;
            }
        }
    }

    void Update()
    {
        if (isOpening && chestLid != null)
        {
            chestLid.localEulerAngles = Vector3.Lerp(
                chestLid.localEulerAngles,
                openRotation,
                Time.deltaTime * openSpeed
            );

            if (Vector3.Distance(chestLid.localEulerAngles, openRotation) < 1f)
            {
                isOpening = false;
            }
        }
    }

    protected override string GetUnlockedPrompt()
    {
        if (isOpened)
        {
            return "Empty chest";
        }
        return "Press [E] to open chest";
    }

    protected override void OnUnlocked()
    {
        if (chestAnimator != null && !string.IsNullOrEmpty(openAnimationTrigger))
        {
            chestAnimator.SetTrigger(openAnimationTrigger);
        }

        if (chestLid != null)
        {
            isOpening = true;
        }

        Debug.Log("Chest unlocked!");
    }

    protected override void OnInteractUnlocked(GameObject player)
    {
        if (isOpened)
        {
            Debug.Log("Chest is empty");
            return;
        }

        OpenChest();
    }

    void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;
        SpawnItems();

        if (chestLid != null && !isOpening)
        {
            isOpening = true;
        }

        Debug.Log("Chest opened!");
    }

    void SpawnItems()
    {
        if (itemsInside == null || itemsInside.Length == 0)
        {
            Debug.Log("Chest is empty!");
            return;
        }

        foreach (GameObject itemPrefab in itemsInside)
        {
            if (itemPrefab != null)
            {
                Vector3 spawnPos = transform.position + spawnOffset;
                GameObject spawnedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                
                Rigidbody rb = spawnedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * 2f + Random.insideUnitSphere, ForceMode.Impulse);
                }
            }
        }

        Debug.Log($"Spawned {itemsInside.Length} items from chest!");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isLocked ? Color.red : (isOpened ? Color.gray : Color.green);
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 1f));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + spawnOffset, 0.2f);
    }
}

using UnityEngine;

public class PersistentItem : MonoBehaviour
{
    [Header("Persistent Settings")]
    public string itemID;
    public bool autoGenerateID = true;
    
    [Header("Scene Info")]
    public string sceneName;

    void Start()
    {
        if (autoGenerateID && string.IsNullOrEmpty(itemID))
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            itemID = $"{sceneName}_{gameObject.name}_{transform.position.GetHashCode()}";
        }

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerData.HasPickedUpItem(itemID))
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    public void OnPickedUp()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerData.MarkItemPickedUp(itemID);
        }
    }
}

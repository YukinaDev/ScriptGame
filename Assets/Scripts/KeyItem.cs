using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Info")]
    public string keyID = "Key_Red";
    public string keyDisplayName = "Red Key";
    
    [Header("Visual (Optional)")]
    public Sprite keyIcon;
    public Color keyColor = Color.red;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description = "A red colored key";

    public bool CanUnlock(string lockID)
    {
        return keyID.Equals(lockID, System.StringComparison.OrdinalIgnoreCase);
    }

    void OnValidate()
    {
        if (string.IsNullOrEmpty(keyDisplayName))
        {
            keyDisplayName = keyID.Replace("_", " ");
        }
    }
}

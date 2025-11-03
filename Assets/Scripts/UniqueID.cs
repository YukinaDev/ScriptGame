using UnityEngine;

public class UniqueID : MonoBehaviour
{
    [Header("Unique Identifier")]
    [Tooltip("ID duy nhất cho object này. Dùng để track đã nhặt/mở chưa")]
    public string ID;
    
    [Header("Auto Generate")]
    [Tooltip("Tự động tạo ID khi chưa có")]
    public bool autoGenerateID = true;

    void Awake()
    {
        // Tự động tạo ID nếu chưa có
        if (autoGenerateID && string.IsNullOrEmpty(ID))
        {
            GenerateID();
        }
    }

    [ContextMenu("Generate New ID")]
    public void GenerateID()
    {
        // Format: SceneName_ObjectName_RandomGUID
        string sceneName = gameObject.scene.name;
        string objectName = gameObject.name.Replace("(Clone)", "").Replace(" ", "_");
        string guid = System.Guid.NewGuid().ToString().Substring(0, 8);
        
        ID = $"{sceneName}_{objectName}_{guid}";
        
        Debug.Log($"[UniqueID] Generated ID: {ID}");
    }

    [ContextMenu("Copy ID to Clipboard")]
    public void CopyIDToClipboard()
    {
        if (!string.IsNullOrEmpty(ID))
        {
            GUIUtility.systemCopyBuffer = ID;
            Debug.Log($"[UniqueID] Copied to clipboard: {ID}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ label với ID trong Scene view
        if (!string.IsNullOrEmpty(ID))
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"ID: {ID}");
#endif
        }
    }
}

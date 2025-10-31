using UnityEngine;
using TMPro;
using System.Collections;

public class MessageDisplay : MonoBehaviour
{
    public static MessageDisplay Instance;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messagePanel;
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    
    private Coroutine hideCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    // Hiện message tạm thời (tự động ẩn sau 3 giây)
    public void ShowMessage(string message)
    {
        Debug.Log($"[MessageDisplay] ShowMessage called with: {message}");
        
        if (messageText != null)
        {
            messageText.text = message;
        }
        else
        {
            Debug.LogError("[MessageDisplay] messageText is NULL! Assign TextMeshProUGUI in Inspector!");
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[MessageDisplay] messagePanel is NULL! Assign Panel GameObject in Inspector!");
        }
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // Hiện prompt liên tục (không tự động ẩn, dùng cho interaction prompt)
    public void ShowPrompt(string message)
    {
        if (messageText != null)
        {
            messageText.text = $"[E] {message}";
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
        
        // Hủy auto-hide nếu đang chạy
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    // Ẩn prompt/message thủ công
    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
}

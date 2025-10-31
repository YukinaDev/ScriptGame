using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject promptPanel;

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
        
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    public void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = $"[E] {message}";
        }
        
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
}

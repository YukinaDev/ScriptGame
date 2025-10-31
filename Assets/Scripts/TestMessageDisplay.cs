using UnityEngine;

public class TestMessageDisplay : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== TESTING MESSAGE DISPLAY ===");
        
        // Test sau 2 giây
        Invoke("TestMessage", 2f);
    }

    void TestMessage()
    {
        Debug.Log("Attempting to show test message...");
        
        if (MessageDisplay.Instance != null)
        {
            Debug.Log("MessageDisplay.Instance EXISTS - Showing message now!");
            MessageDisplay.Instance.ShowMessage("TEST: I need Red Key");
        }
        else
        {
            Debug.LogError("MessageDisplay.Instance is NULL!");
            
            // Tìm MessageDisplay trong scene
            MessageDisplay[] displays = FindObjectsOfType<MessageDisplay>();
            Debug.Log($"Found {displays.Length} MessageDisplay objects in scene");
            
            foreach (MessageDisplay display in displays)
            {
                Debug.Log($"Found MessageDisplay on: {display.gameObject.name}");
            }
        }
    }
}

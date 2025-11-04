using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Gọi từ Button "Play"
    public void PlayGame()
    {
        // Đổi tên scene theo scene đầu tiên của game
        SceneManager.LoadScene("HouseC_Scene"); // Hoặc "MainGame"
    }
    
    // Gọi từ Button "Quit"
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // Gọi từ Button "Settings"
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }
    
    // Load scene bất kỳ (dùng chung cho nhiều buttons)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RestartLevel()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void LoadMainMenu()
    {
        // Load main menu scene (adjust name if needed)
        SceneManager.LoadScene("MainMenu");
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void QuitGame()
    {
        Debug.Log("[GameOverManager] Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[GameOverManager] Initialized in scene: " + SceneManager.GetActiveScene().name);
    }
    
    public void RestartLevel()
    {
        Debug.Log("[GameOverManager] RestartLevel() called!");
        
        // Unpause game
        Time.timeScale = 1f;
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void LoadMainMenu()
    {
        Debug.Log("[GameOverManager] LoadMainMenu() called!");
        
        // Unpause game
        Time.timeScale = 1f;
        
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

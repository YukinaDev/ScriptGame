using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button closeButton;
    
    [Header("Settings Controls")]
    public Slider mouseSensitivitySlider;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    
    [Header("Player Reference")]
    public FirstPersonController playerController;

    private bool isSettingsOpen = false;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        LoadSettings();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        
        if (settingsPanel != null)
            settingsPanel.SetActive(isSettingsOpen);

        if (isSettingsOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void LoadSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = savedSensitivity;

        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = savedFullscreen;

        if (playerController != null)
            playerController.mouseSensitivity = savedSensitivity;

        AudioListener.volume = savedVolume;
        Screen.fullScreen = savedFullscreen;
    }

    void OnDestroy()
    {
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(ToggleSettings);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseSettings);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }
}

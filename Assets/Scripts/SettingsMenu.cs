using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Button settingsButton;

    [Header("Settings Options")]
    public Slider mouseSensitivitySlider;
    public Slider masterVolumeSlider;
    public Toggle fullscreenToggle;

    [Header("References")]
    public FirstPersonController playerController;

    private bool isMenuOpen = false;

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettingsMenu);
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<FirstPersonController>();
        }

        LoadSettings();
        SetupListeners();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettingsMenu();
        }
    }

    void SetupListeners()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    public void ToggleSettingsMenu()
    {
        isMenuOpen = !isMenuOpen;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isMenuOpen);
        }

        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void LoadSettings()
    {
        float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = mouseSensitivity;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
        }

        if (playerController != null)
        {
            playerController.mouseSensitivity = mouseSensitivity;
        }

        AudioListener.volume = masterVolume;
        Screen.fullScreen = fullscreen;
    }

    public void ResetToDefaults()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = 2f;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 1f;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = true;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "SampleScene";
    [SerializeField] private float loadingDelay = 0.5f;
    [SerializeField] private GameObject loadingPanel;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioSource bgMusicSource;
    private AudioSource sfxSource;

    void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        if (bgMusicSource != null)
        {
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }
    }

    public void PlayClickSound()
    {
        if (buttonClickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayGame()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneWithAnimation());
    }

    private IEnumerator LoadSceneWithAnimation()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return new WaitForSeconds(loadingDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            yield return null;
        }
    }

    public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

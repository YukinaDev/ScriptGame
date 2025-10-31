using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.loop = loop;
            audioSource.volume = volume;
            
            if (backgroundMusic != null)
            {
                audioSource.clip = backgroundMusic;
            }
            
            if (playOnStart && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("BackgroundMusic: No AudioSource found!");
        }
    }

    public void Play()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void Pause()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}

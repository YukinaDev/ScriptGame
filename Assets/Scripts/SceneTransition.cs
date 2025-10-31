using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;
    public static SceneTransition Instance
    {
        get { return instance; }
    }

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    private bool isTransitioning = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup == null)
        {
            CreateFadeCanvas();
        }
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform);
        
        UnityEngine.UI.Image image = panelObj.AddComponent<UnityEngine.UI.Image>();
        image.color = fadeColor;
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
    }

    public void LoadHouseScene(string sceneName)
    {
        if (!isTransitioning)
        {
            GameManager.Instance.SavePlayerState();
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
    }

    public void ReturnToHub()
    {
        if (!isTransitioning)
        {
            GameManager.Instance.SavePlayerState();
            StartCoroutine(FadeOutAndLoad(GameManager.Instance.hubSceneName));
        }
    }

    IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        isTransitioning = true;
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (fadeCanvasGroup == null) yield break;

        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = elapsed / fadeDuration;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene(sceneName);

        yield return null;

        StartCoroutine(FadeIn());
    }
}


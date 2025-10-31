using UnityEngine;
using TMPro;

public class LoadingText : MonoBehaviour
{
    [SerializeField] private float dotInterval = 0.5f;
    private TextMeshProUGUI textComponent;
    private string baseText = "Loading";
    private int dotCount = 0;
    private float timer = 0f;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= dotInterval)
        {
            timer = 0f;
            dotCount = (dotCount + 1) % 4;
            textComponent.text = baseText + new string('.', dotCount);
        }
    }
}

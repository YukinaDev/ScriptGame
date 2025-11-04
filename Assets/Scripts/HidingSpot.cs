using UnityEngine;

public class HidingSpot : MonoBehaviour, IInteractable
{
    [Header("Hiding Settings")]
    public string hidingSpotName = "Cabinet";
    public Transform hidePosition;
    [Tooltip("(Optional) Empty GameObject để chỉ định hướng camera nhìn. Bỏ trống = dùng rotation của HidePosition")]
    public Transform hideViewDirection;

    [Header("Player Settings")]
    [Tooltip("Ẩn hoàn toàn player model")]
    public bool hidePlayerModel = true;
    [Tooltip("Làm tối màn hình khi trốn")]
    public bool darkenScreen = false;
    [Range(0f, 1f)]
    public float darkenAmount = 0.5f;
    
    [Header("Hiding View UI")]
    [Tooltip("Hiển thị UI overlay khi trốn (khung tủ, khe hở...)")]
    public bool showHidingOverlay = true;
    public Sprite cabinetFrameSprite;
    public Color overlayTintColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("UI Messages")]
    public string enterMessage = "Press E to Hide";
    public string exitMessage = "Press E to Exit";
    public string occupiedMessage = "Someone is already hiding here";
    
    [Header("Audio (Optional)")]
    public AudioClip enterSound;
    public AudioClip exitSound;
    public AudioSource audioSource;
    
    private bool isOccupied = false;
    private GameObject hiddenPlayer;
    private FirstPersonController playerController;
    private InteractionSystem interactionSystem;
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;
    private Camera playerCamera;
    private Quaternion originalCameraRotation;
    private GameObject playerModel;
    private GameObject hidingOverlayUI;
    private Collider cabinetCollider;
    private int originalPlayerLayer;

    void Start()
    {
        // Tạo hide position mặc định nếu chưa có
        if (hidePosition == null)
        {
            GameObject hidePos = new GameObject("HidePosition");
            hidePos.transform.SetParent(transform);
            hidePos.transform.localPosition = Vector3.zero;
            hidePosition = hidePos.transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Lấy collider của tủ
        cabinetCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // Check input để exit khi đang trốn
        if (isOccupied && hiddenPlayer != null)
        {
            // Không cần ShowPrompt vì overlay UI đã có text rồi
            
            // Check E key để exit
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                ExitHiding();
            }
        }
    }

    public string GetInteractPrompt()
    {
        if (isOccupied && hiddenPlayer != null)
        {
            // Đang trốn trong tủ này - không hiện prompt (overlay UI đã có)
            return "";
        }
        else if (isOccupied)
        {
            // Người khác đang trốn (multiplayer)
            return occupiedMessage;
        }
        else
        {
            return $"{enterMessage} in {hidingSpotName}";
        }
    }

    public void Interact(GameObject player)
    {
        if (isOccupied && hiddenPlayer == player)
        {
            // Exit hiding
            ExitHiding();
        }
        else if (!isOccupied)
        {
            // Enter hiding
            EnterHiding(player);
        }
    }

    void EnterHiding(GameObject player)
    {
        hiddenPlayer = player;
        isOccupied = true;

        // Lưu vị trí ban đầu
        originalPlayerPosition = player.transform.position;
        originalPlayerRotation = player.transform.rotation;

        // Get components
        playerController = player.GetComponent<FirstPersonController>();
        interactionSystem = player.GetComponent<InteractionSystem>();
        playerCamera = player.GetComponentInChildren<Camera>();
        
        if (playerCamera != null)
        {
            originalCameraRotation = playerCamera.transform.rotation;
        }

        // Disable movement and interaction
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable interaction để không raycast hit tủ khác
        if (interactionSystem != null)
        {
            interactionSystem.enabled = false;
        }

        // Di chuyển player vào vị trí trốn
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = hidePosition.position;
            player.transform.rotation = hidePosition.rotation;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = hidePosition.position;
            player.transform.rotation = hidePosition.rotation;
        }

        // Xoay camera về hướng nhìn (nếu có)
        if (playerCamera != null && hideViewDirection != null)
        {
            playerCamera.transform.rotation = hideViewDirection.rotation;
        }

        // Ẩn player model
        if (hidePlayerModel)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.gameObject != playerCamera?.gameObject) // Không ẩn camera
                {
                    r.enabled = false;
                }
            }
        }
        
        // ĐỔI LAYER ĐỂ ENEMY KHÔNG NHÌN THẤY
        originalPlayerLayer = player.layer;
        player.layer = LayerMask.NameToLayer("Ignore Raycast");
        Debug.Log("[HidingSpot] Player hidden, changed to Ignore Raycast layer");

        // Phát âm thanh
        if (enterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enterSound);
        }

        // Ẩn interaction prompt
        if (InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.HidePrompt();
        }
        
        // Force ẩn MessageDisplay nếu có
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.Hide();
        }

        // Disable collider để raycast không hit tủ này từ trong
        if (cabinetCollider != null)
        {
            cabinetCollider.enabled = false;
        }

        // Tạo hiding overlay UI (đã có text exit ở đây rồi)
        if (showHidingOverlay)
        {
            CreateHidingOverlay();
        }

    }

    void ExitHiding()
    {
        if (hiddenPlayer == null) return;

        GameObject player = hiddenPlayer;

        // Enable movement and interaction
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Enable lại interaction system
        if (interactionSystem != null)
        {
            interactionSystem.enabled = true;
        }

        // Di chuyển player ra khỏi hiding spot
        CharacterController cc = player.GetComponent<CharacterController>();
        Vector3 exitPosition = originalPlayerPosition + transform.forward * 1.5f; // Ra phía trước tủ

        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = exitPosition;
            player.transform.rotation = originalPlayerRotation;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = exitPosition;
            player.transform.rotation = originalPlayerRotation;
        }

        // Restore camera rotation
        if (playerCamera != null)
        {
            playerCamera.transform.rotation = originalCameraRotation;
        }

        // Hiện lại player model
        if (hidePlayerModel)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
        }
        
        // RESTORE LAYER
        player.layer = originalPlayerLayer;
        Debug.Log("[HidingSpot] Player visible again, restored layer");

        // Phát âm thanh
        if (exitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitSound);
        }

        // Hiện message
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowMessage($"Left {hidingSpotName}");
        }

        // Xóa hiding overlay UI
        if (hidingOverlayUI != null)
        {
            Destroy(hidingOverlayUI);
            hidingOverlayUI = null;
        }

        // Enable lại collider
        if (cabinetCollider != null)
        {
            cabinetCollider.enabled = true;
        }

        // Reset state
        hiddenPlayer = null;
        isOccupied = false;
        playerController = null;
        interactionSystem = null;
        playerCamera = null;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

    public bool IsPlayerHiding(GameObject player)
    {
        return isOccupied && hiddenPlayer == player;
    }

    void CreateHidingOverlay()
    {
        // Tạo Canvas overlay
        hidingOverlayUI = new GameObject("HidingOverlay");
        Canvas canvas = hidingOverlayUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = hidingOverlayUI.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        hidingOverlayUI.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Panel tối che toàn màn hình
        GameObject darkPanel = new GameObject("DarkPanel");
        darkPanel.transform.SetParent(hidingOverlayUI.transform, false);
        
        UnityEngine.UI.Image darkImage = darkPanel.AddComponent<UnityEngine.UI.Image>();
        darkImage.color = overlayTintColor;
        
        RectTransform darkRect = darkPanel.GetComponent<RectTransform>();
        darkRect.anchorMin = Vector2.zero;
        darkRect.anchorMax = Vector2.one;
        darkRect.sizeDelta = Vector2.zero;

        // Khung cửa tủ (nếu có sprite)
        if (cabinetFrameSprite != null)
        {
            GameObject framePanel = new GameObject("CabinetFrame");
            framePanel.transform.SetParent(hidingOverlayUI.transform, false);
            
            UnityEngine.UI.Image frameImage = framePanel.AddComponent<UnityEngine.UI.Image>();
            frameImage.sprite = cabinetFrameSprite;
            frameImage.color = Color.white;
            
            RectTransform frameRect = framePanel.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = Vector2.zero;
        }

        // Text hiển thị hint
        GameObject hintText = new GameObject("HintText");
        hintText.transform.SetParent(hidingOverlayUI.transform, false);
        
        UnityEngine.UI.Text text = hintText.AddComponent<UnityEngine.UI.Text>();
        text.text = "Press [E] to Exit";
        text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = hintText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.1f);
        textRect.anchorMax = new Vector2(0.5f, 0.1f);
        textRect.sizeDelta = new Vector2(400, 50);
        textRect.anchoredPosition = Vector2.zero;

        // Shadow cho text
        UnityEngine.UI.Shadow shadow = hintText.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 2f, 1f));
        
        if (hidePosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(hidePosition.position, 0.3f);
            Gizmos.DrawLine(transform.position, hidePosition.position);
        }

        if (hideViewDirection != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(hideViewDirection.position, hideViewDirection.forward * 2f);
        }
    }
}

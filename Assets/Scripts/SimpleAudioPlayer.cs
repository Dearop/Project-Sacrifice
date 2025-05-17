using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip[] audioClips;
    public float volume = 1.0f;
    
    [Header("UI Settings")]
    public bool createUI = true;
    public Canvas canvas;
    
    private AudioSource audioSource;
    private int currentClipIndex = 0;
    
    void Awake()
    {
        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        // Check if we have any audio clips
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("SimpleAudioPlayer: No audio clips assigned!");
            
            // Try to find audio clips in the project
            audioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            if (audioClips.Length > 0)
            {
                Debug.Log($"SimpleAudioPlayer: Found {audioClips.Length} audio clips in the project");
            }
        }
        
        // Set initial clip
        if (audioClips.Length > 0 && audioClips[0] != null)
        {
            audioSource.clip = audioClips[0];
            Debug.Log($"SimpleAudioPlayer: Initial clip set to {audioClips[0].name}");
        }
    }
    
    void Start()
    {
        // Create UI if needed
        if (createUI)
        {
            SetupUI();
        }
    }
    
    void SetupUI()
    {
        // Create canvas if needed
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Simple Audio Player Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create panel
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvas.transform);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(400, 200);
        panelRect.anchoredPosition = new Vector2(0, 20);
        
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelRect);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "SIMPLE AUDIO PLAYER";
        titleText.fontSize = 20;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 30);
        titleRect.anchoredPosition = new Vector2(0, 0);
        
        // Create clip name display
        GameObject clipNameObj = new GameObject("ClipName");
        clipNameObj.transform.SetParent(panelRect);
        
        TextMeshProUGUI clipNameText = clipNameObj.AddComponent<TextMeshProUGUI>();
        clipNameText.text = audioSource.clip != null ? audioSource.clip.name : "No clip selected";
        clipNameText.fontSize = 16;
        clipNameText.alignment = TextAlignmentOptions.Center;
        clipNameText.color = Color.yellow;
        
        RectTransform clipNameRect = clipNameObj.GetComponent<RectTransform>();
        clipNameRect.anchorMin = new Vector2(0, 1);
        clipNameRect.anchorMax = new Vector2(1, 1);
        clipNameRect.pivot = new Vector2(0.5f, 1);
        clipNameRect.sizeDelta = new Vector2(0, 30);
        clipNameRect.anchoredPosition = new Vector2(0, -30);
        
        // Create buttons
        CreateButton(panelRect, "Play", new Vector2(-100, -80), () => PlayAudio());
        CreateButton(panelRect, "Stop", new Vector2(0, -80), () => StopAudio());
        CreateButton(panelRect, "Next", new Vector2(100, -80), () => NextClip());
        
        // Create volume slider
        GameObject sliderObj = new GameObject("VolumeSlider");
        sliderObj.transform.SetParent(panelRect);
        
        Slider volumeSlider = sliderObj.AddComponent<Slider>();
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 1;
        volumeSlider.value = volume;
        volumeSlider.onValueChanged.AddListener((value) => SetVolume(value));
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0);
        sliderRect.anchorMax = new Vector2(0.5f, 0);
        sliderRect.pivot = new Vector2(0.5f, 0);
        sliderRect.sizeDelta = new Vector2(300, 20);
        sliderRect.anchoredPosition = new Vector2(0, 30);
        
        // Create slider background
        GameObject sliderBgObj = new GameObject("Background");
        sliderBgObj.transform.SetParent(sliderRect);
        
        Image sliderBgImage = sliderBgObj.AddComponent<Image>();
        sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);
        
        RectTransform sliderBgRect = sliderBgObj.GetComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.sizeDelta = Vector2.zero;
        
        // Create slider fill
        GameObject sliderFillObj = new GameObject("Fill");
        sliderFillObj.transform.SetParent(sliderRect);
        
        Image sliderFillImage = sliderFillObj.AddComponent<Image>();
        sliderFillImage.color = new Color(0.8f, 0.8f, 0.2f, 1);
        
        RectTransform sliderFillRect = sliderFillObj.GetComponent<RectTransform>();
        sliderFillRect.anchorMin = Vector2.zero;
        sliderFillRect.anchorMax = new Vector2(volumeSlider.value, 1);
        sliderFillRect.sizeDelta = Vector2.zero;
        
        // Set up the slider
        volumeSlider.targetGraphic = sliderBgImage;
        volumeSlider.fillRect = sliderFillRect;
        
        // Create volume label
        GameObject volumeLabelObj = new GameObject("VolumeLabel");
        volumeLabelObj.transform.SetParent(panelRect);
        
        TextMeshProUGUI volumeLabelText = volumeLabelObj.AddComponent<TextMeshProUGUI>();
        volumeLabelText.text = "Volume";
        volumeLabelText.fontSize = 14;
        volumeLabelText.alignment = TextAlignmentOptions.Center;
        volumeLabelText.color = Color.white;
        
        RectTransform volumeLabelRect = volumeLabelObj.GetComponent<RectTransform>();
        volumeLabelRect.anchorMin = new Vector2(0.5f, 0);
        volumeLabelRect.anchorMax = new Vector2(0.5f, 0);
        volumeLabelRect.pivot = new Vector2(0.5f, 0);
        volumeLabelRect.sizeDelta = new Vector2(100, 20);
        volumeLabelRect.anchoredPosition = new Vector2(0, 60);
    }
    
    private void CreateButton(RectTransform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1);
        button.targetGraphic = buttonImage;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(80, 40);
        buttonRect.anchoredPosition = position;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonRect);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        button.onClick.AddListener(action);
    }
    
    public void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log($"SimpleAudioPlayer: Playing {audioSource.clip.name}");
        }
        else
        {
            Debug.LogWarning("SimpleAudioPlayer: No clip to play");
        }
    }
    
    public void StopAudio()
    {
        audioSource.Stop();
        Debug.Log("SimpleAudioPlayer: Stopped audio");
    }
    
    public void NextClip()
    {
        if (audioClips.Length == 0) return;
        
        currentClipIndex = (currentClipIndex + 1) % audioClips.Length;
        
        // Skip null clips
        int attempts = 0;
        while (audioClips[currentClipIndex] == null && attempts < audioClips.Length)
        {
            currentClipIndex = (currentClipIndex + 1) % audioClips.Length;
            attempts++;
        }
        
        if (audioClips[currentClipIndex] != null)
        {
            bool wasPlaying = audioSource.isPlaying;
            audioSource.clip = audioClips[currentClipIndex];
            
            Debug.Log($"SimpleAudioPlayer: Changed clip to {audioClips[currentClipIndex].name}");
            
            // Update UI if it exists
            TextMeshProUGUI clipNameText = GetComponentInChildren<TextMeshProUGUI>();
            if (clipNameText != null && clipNameText.gameObject.name == "ClipName")
            {
                clipNameText.text = audioSource.clip.name;
            }
            
            if (wasPlaying)
            {
                audioSource.Play();
            }
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        audioSource.volume = volume;
        Debug.Log($"SimpleAudioPlayer: Volume set to {volume}");
    }
    
    void Update()
    {
        // Press P to play/pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (audioSource.isPlaying)
            {
                StopAudio();
            }
            else
            {
                PlayAudio();
            }
        }
        
        // Press N to go to next clip
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextClip();
        }
    }
} 
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AudioDebugger : MonoBehaviour
{
    [Tooltip("Canvas to display debug UI")]
    public Canvas debugCanvas;
    
    [Tooltip("Button prefab to create for each audio NPC")]
    public Button buttonPrefab;
    
    [Tooltip("Audio clips to test directly")]
    public AudioClip[] testClips;
    
    private List<AudioInteractableAdvanced> audioNpcs = new List<AudioInteractableAdvanced>();
    private List<Button> debugButtons = new List<Button>();
    private AudioSource debugAudioSource;
    
    private void Awake()
    {
        // Create our own AudioSource for direct testing
        debugAudioSource = gameObject.AddComponent<AudioSource>();
        debugAudioSource.playOnAwake = false;
        debugAudioSource.volume = 1.0f;
    }
    
    private void Start()
    {
        // Find all audio NPCs in the scene
        AudioInteractableAdvanced[] npcs = FindObjectsOfType<AudioInteractableAdvanced>();
        audioNpcs.AddRange(npcs);
        
        Debug.Log($"Found {audioNpcs.Count} audio NPCs in the scene");
        
        // Create debug canvas if not provided
        if (debugCanvas == null)
        {
            GameObject canvasObj = new GameObject("Debug Canvas");
            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create button prefab if not provided
        if (buttonPrefab == null)
        {
            GameObject buttonObj = new GameObject("Debug Button");
            buttonObj.transform.SetParent(debugCanvas.transform);
            
            // Add required components
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontSize = 16;
            
            // Set anchors and position
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Add button component
            buttonPrefab = buttonObj.AddComponent<Button>();
            buttonPrefab.targetGraphic = buttonImage;
            
            // Hide the template
            buttonObj.SetActive(false);
        }
        
        // Create buttons for each NPC
        CreateDebugButtons();
        
        // Try to find audio clips if none are assigned
        if (testClips == null || testClips.Length == 0)
        {
            testClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            Debug.Log($"Found {testClips.Length} audio clips in the project");
        }
        
        // Create buttons for direct audio testing
        CreateDirectAudioButtons();
    }
    
    private void CreateDebugButtons()
    {
        float buttonHeight = 40f;
        float spacing = 10f;
        float startY = Screen.height - buttonHeight - spacing;
        
        for (int i = 0; i < audioNpcs.Count; i++)
        {
            AudioInteractableAdvanced npc = audioNpcs[i];
            
            // Create button
            Button newButton = Instantiate(buttonPrefab, debugCanvas.transform);
            newButton.gameObject.SetActive(true);
            
            // Set position
            RectTransform rectTransform = newButton.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(200, buttonHeight);
            rectTransform.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * i);
            
            // Set text
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Test {npc.gameObject.name}";
            }
            
            // Add click handler
            int index = i; // Capture for lambda
            newButton.onClick.AddListener(() => TestAudioNPC(index));
            
            debugButtons.Add(newButton);
        }
        
        // Add a button to test all NPCs
        if (audioNpcs.Count > 0)
        {
            Button allButton = Instantiate(buttonPrefab, debugCanvas.transform);
            allButton.gameObject.SetActive(true);
            
            // Set position
            RectTransform rectTransform = allButton.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(200, buttonHeight);
            rectTransform.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * audioNpcs.Count);
            
            // Set text
            TextMeshProUGUI buttonText = allButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Test All NPCs";
            }
            
            // Add click handler
            allButton.onClick.AddListener(TestAllAudioNPCs);
            
            debugButtons.Add(allButton);
        }
        
        // Add a button to check audio settings
        Button checkButton = Instantiate(buttonPrefab, debugCanvas.transform);
        checkButton.gameObject.SetActive(true);
        
        // Set position
        RectTransform checkRectTransform = checkButton.GetComponent<RectTransform>();
        checkRectTransform.anchorMin = new Vector2(0, 1);
        checkRectTransform.anchorMax = new Vector2(0, 1);
        checkRectTransform.pivot = new Vector2(0, 1);
        checkRectTransform.sizeDelta = new Vector2(200, buttonHeight);
        int buttonCount = audioNpcs.Count > 0 ? audioNpcs.Count + 1 : 0;
        checkRectTransform.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * buttonCount);
        
        // Set text
        TextMeshProUGUI checkButtonText = checkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (checkButtonText != null)
        {
            checkButtonText.text = "Check Audio Settings";
        }
        
        // Add click handler
        checkButton.onClick.AddListener(CheckAudioSettings);
        
        debugButtons.Add(checkButton);
    }
    
    private void CreateDirectAudioButtons()
    {
        float buttonHeight = 40f;
        float spacing = 10f;
        int startIndex = debugButtons.Count;
        
        // Add a separator text
        GameObject separatorObj = new GameObject("Separator");
        separatorObj.transform.SetParent(debugCanvas.transform);
        
        TextMeshProUGUI separatorText = separatorObj.AddComponent<TextMeshProUGUI>();
        separatorText.text = "DIRECT AUDIO TESTING";
        separatorText.fontSize = 16;
        separatorText.alignment = TextAlignmentOptions.Center;
        separatorText.color = Color.yellow;
        
        RectTransform separatorRect = separatorObj.GetComponent<RectTransform>();
        separatorRect.anchorMin = new Vector2(0, 1);
        separatorRect.anchorMax = new Vector2(0, 1);
        separatorRect.pivot = new Vector2(0, 1);
        separatorRect.sizeDelta = new Vector2(200, buttonHeight);
        separatorRect.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * startIndex);
        
        // Create buttons for direct audio testing
        int maxClips = Mathf.Min(testClips.Length, 5); // Limit to 5 clips to avoid UI clutter
        
        for (int i = 0; i < maxClips; i++)
        {
            if (testClips[i] == null) continue;
            
            // Create button
            Button newButton = Instantiate(buttonPrefab, debugCanvas.transform);
            newButton.gameObject.SetActive(true);
            
            // Set position
            RectTransform rectTransform = newButton.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(200, buttonHeight);
            rectTransform.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * (startIndex + 1 + i));
            
            // Set text
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Play {testClips[i].name}";
            }
            
            // Add click handler
            int clipIndex = i; // Capture for lambda
            newButton.onClick.AddListener(() => PlayDirectAudio(clipIndex));
            
            debugButtons.Add(newButton);
        }
        
        // Add a stop button
        Button stopButton = Instantiate(buttonPrefab, debugCanvas.transform);
        stopButton.gameObject.SetActive(true);
        
        // Set position
        RectTransform stopRectTransform = stopButton.GetComponent<RectTransform>();
        stopRectTransform.anchorMin = new Vector2(0, 1);
        stopRectTransform.anchorMax = new Vector2(0, 1);
        stopRectTransform.pivot = new Vector2(0, 1);
        stopRectTransform.sizeDelta = new Vector2(200, buttonHeight);
        stopRectTransform.anchoredPosition = new Vector2(spacing, -spacing - (buttonHeight + spacing) * (startIndex + 1 + maxClips));
        
        // Set text
        TextMeshProUGUI stopButtonText = stopButton.GetComponentInChildren<TextMeshProUGUI>();
        if (stopButtonText != null)
        {
            stopButtonText.text = "Stop All Audio";
            stopButtonText.color = Color.red;
        }
        
        // Add click handler
        stopButton.onClick.AddListener(StopAllAudio);
        
        debugButtons.Add(stopButton);
    }
    
    private void TestAudioNPC(int index)
    {
        if (index >= 0 && index < audioNpcs.Count)
        {
            AudioInteractableAdvanced npc = audioNpcs[index];
            
            // Get the audio clip directly
            System.Reflection.FieldInfo clipField = typeof(AudioInteractableAdvanced).GetField("musicClip", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
            if (clipField != null)
            {
                AudioClip clip = clipField.GetValue(npc) as AudioClip;
                
                if (clip != null)
                {
                    // Try to play using our own AudioSource
                    debugAudioSource.clip = clip;
                    debugAudioSource.Play();
                    Debug.Log($"Direct playing audio clip: {clip.name}");
                }
                else
                {
                    Debug.LogError($"NPC {npc.gameObject.name} has no audio clip assigned");
                }
            }
            
            // Also try the original method
            System.Reflection.MethodInfo method = typeof(AudioInteractableAdvanced).GetMethod("TestAudio", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (method != null)
            {
                method.Invoke(npc, null);
                Debug.Log($"Testing audio on NPC: {npc.gameObject.name}");
            }
            else
            {
                Debug.LogError("TestAudio method not found on AudioInteractableAdvanced");
            }
        }
    }
    
    private void TestAllAudioNPCs()
    {
        for (int i = 0; i < audioNpcs.Count; i++)
        {
            TestAudioNPC(i);
        }
    }
    
    private void PlayDirectAudio(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < testClips.Length && testClips[clipIndex] != null)
        {
            debugAudioSource.clip = testClips[clipIndex];
            debugAudioSource.Play();
            Debug.Log($"Playing audio clip directly: {testClips[clipIndex].name}");
        }
    }
    
    private void StopAllAudio()
    {
        // Stop our debug audio source
        debugAudioSource.Stop();
        
        // Stop all audio sources in the scene
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            source.Stop();
        }
        
        Debug.Log("Stopped all audio");
    }
    
    private void CheckAudioSettings()
    {
        // Check if audio is muted in Unity
        Debug.Log($"Unity Audio Settings: Volume={AudioListener.volume}, Listener Count={FindObjectsOfType<AudioListener>().Length}");
        
        // Check each NPC's audio setup
        foreach (AudioInteractableAdvanced npc in audioNpcs)
        {
            AudioSource source = npc.GetComponent<AudioSource>();
            System.Reflection.FieldInfo clipField = typeof(AudioInteractableAdvanced).GetField("musicClip", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
            AudioClip clip = null;
            if (clipField != null)
            {
                clip = clipField.GetValue(npc) as AudioClip;
            }
            
            string status = $"NPC: {npc.gameObject.name}\n";
            status += $"  Has AudioSource: {source != null}\n";
            
            if (source != null)
            {
                status += $"  Source Volume: {source.volume}\n";
                status += $"  Source Mute: {source.mute}\n";
                status += $"  Source Clip: {(source.clip != null ? source.clip.name : "null")}\n";
                status += $"  Spatial: {source.spatialBlend > 0}\n";
            }
            
            status += $"  Script Clip: {(clip != null ? clip.name : "null")}\n";
            
            Debug.Log(status);
        }
    }
    
    private void Update()
    {
        // Toggle debug UI with F12 key
        if (Input.GetKeyDown(KeyCode.F12))
        {
            debugCanvas.gameObject.SetActive(!debugCanvas.gameObject.activeInHierarchy);
        }
    }
} 
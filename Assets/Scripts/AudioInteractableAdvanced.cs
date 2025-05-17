using UnityEngine;
using System.Collections;

public class AudioInteractableAdvanced : MonoBehaviour
{
    [Tooltip("Reference to the dialogue system")]
    public DialogueSystem dialogueSystem;
    
    [Header("Dialogue Settings")]
    [Tooltip("The dialogue lines to display when first interacting")]
    public DialogueLine[] introDialogueLines;
    
    [Tooltip("The dialogue lines to display when stopping the music")]
    public DialogueLine[] outroDialogueLines;
    
    [Header("Audio Settings")]
    [Tooltip("Audio clip to play")]
    public AudioClip musicClip;
    
    [Tooltip("Volume of the audio")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Should the audio loop?")]
    public bool loop = true;
    
    [Header("Fade Settings")]
    [Tooltip("Time in seconds to fade in the audio")]
    [Range(0f, 10f)]
    public float fadeInTime = 2.0f;
    
    [Tooltip("Time in seconds to fade out the audio")]
    [Range(0f, 10f)]
    public float fadeOutTime = 2.0f;
    
    [Header("Spatial Audio Settings")]
    [Tooltip("Should the audio be spatial (3D)?")]
    public bool spatialAudio = true;
    
    [Tooltip("How quickly the volume attenuates with distance")]
    [Range(0.1f, 5f)]
    public float spatialBlend = 1.0f;
    
    [Tooltip("Minimum distance before volume starts decreasing")]
    public float minDistance = 1f;
    
    [Tooltip("Maximum distance at which audio can be heard")]
    public float maxDistance = 20f;
    
    // Add this script to an object that also has the Interactable component
    private Interactable interactable;
    private AudioSource audioSource;
    
    // State tracking
    private enum AudioNPCState 
    {
        Initial,        // Initial state, ready for first interaction
        IntroDialogue,  // Showing intro dialogue
        FadingIn,       // Fading in the music
        Playing,        // Music is playing
        FadingOut,      // Fading out the music
        OutroDialogue   // Showing outro dialogue
    }
    
    private AudioNPCState currentState = AudioNPCState.Initial;
    private PlayerController playerController;
    private Coroutine fadeCoroutine;
    
    // Add these new fields for player control
    private Vector3 playerPositionBeforeLock;
    private Quaternion playerRotationBeforeLock;
    private CharacterController playerCharacterController;
    private Animator playerAnimator;
    
    private void Awake()
    {
        // Setup audio source in Awake to ensure it's available before Start
        SetupAudioSource();
    }
    
    private void SetupAudioSource()
    {
        // Check if we already have an AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // If no AudioSource exists, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log($"[{gameObject.name}] Created new AudioSource component");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Using existing AudioSource component");
        }
        
        // Configure the AudioSource
        audioSource.clip = musicClip;
        audioSource.volume = 0; // Start at 0 volume for fading
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        
        // Configure spatial audio settings
        if (spatialAudio)
        {
            audioSource.spatialBlend = spatialBlend;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Log audio configuration
        if (musicClip != null)
        {
            Debug.Log($"[{gameObject.name}] Audio clip configured: {musicClip.name}, Duration: {musicClip.length}s");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] No audio clip assigned!");
        }
    }
    
    private void Start()
    {
        // Get the Interactable component
        interactable = GetComponent<Interactable>();
        
        if (interactable == null)
        {
            Debug.LogError($"[{gameObject.name}] AudioInteractableAdvanced requires an Interactable component on the same GameObject");
            return;
        }
        
        // Find the dialogue system if not set in inspector
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            
            if (dialogueSystem == null)
            {
                Debug.LogError($"[{gameObject.name}] DialogueSystem not found in scene.");
                return;
            }
        }
        
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerController not found. Player movement locking may not work.");
        }
        
        // Subscribe to the interaction event
        interactable.onInteract.AddListener(HandleInteraction);
        
        // Verify audio setup
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError($"[{gameObject.name}] Audio setup failed. Attempting to fix...");
            SetupAudioSource();
        }
    }
    
    private void HandleInteraction()
    {
        Debug.Log($"[{gameObject.name}] Interaction detected. Current state: {currentState}");
        
        switch (currentState)
        {
            case AudioNPCState.Initial:
                // First interaction - show intro dialogue
                if (dialogueSystem != null && introDialogueLines != null && introDialogueLines.Length > 0)
                {
                    LockPlayerMovement();
                    dialogueSystem.StartDialogue(introDialogueLines);
                    currentState = AudioNPCState.IntroDialogue;
                    Debug.Log($"[{gameObject.name}] Started intro dialogue");
                }
                break;
                
            case AudioNPCState.IntroDialogue:
                // After intro dialogue - start playing music
                if (!dialogueSystem.IsDialogueActive)
                {
                    StartMusic();
                    currentState = AudioNPCState.FadingIn;
                    Debug.Log($"[{gameObject.name}] Starting music (fading in)");
                }
                break;
                
            case AudioNPCState.FadingIn:
            case AudioNPCState.Playing:
                // While music is playing or fading in - start fading out
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOut());
                currentState = AudioNPCState.FadingOut;
                Debug.Log($"[{gameObject.name}] Stopping music (fading out)");
                break;
                
            case AudioNPCState.FadingOut:
                // If already fading out, skip to dialogue
                if (dialogueSystem != null && outroDialogueLines != null && outroDialogueLines.Length > 0)
                {
                    // Stop any fade and immediately stop music
                    if (fadeCoroutine != null)
                    {
                        StopCoroutine(fadeCoroutine);
                    }
                    audioSource.Stop();
                    audioSource.volume = 0;
                    
                    dialogueSystem.StartDialogue(outroDialogueLines);
                    currentState = AudioNPCState.OutroDialogue;
                    Debug.Log($"[{gameObject.name}] Started outro dialogue");
                }
                else
                {
                    // If no outro dialogue, go back to initial state
                    UnlockPlayerMovement();
                    currentState = AudioNPCState.Initial;
                    Debug.Log($"[{gameObject.name}] No outro dialogue, returning to initial state");
                }
                break;
                
            case AudioNPCState.OutroDialogue:
                // After outro dialogue - go back to initial state
                if (!dialogueSystem.IsDialogueActive)
                {
                    UnlockPlayerMovement();
                    currentState = AudioNPCState.Initial;
                    Debug.Log($"[{gameObject.name}] Dialogue ended, returning to initial state");
                }
                break;
        }
    }
    
    private void StartMusic()
    {
        if (audioSource != null && musicClip != null)
        {
            // Double-check that the clip is assigned
            if (audioSource.clip == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Audio clip was missing, reassigning...");
                audioSource.clip = musicClip;
            }
            
            audioSource.volume = 0;
            audioSource.Play();
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
            
            // Verify that audio is actually playing
            if (!audioSource.isPlaying)
            {
                Debug.LogError($"[{gameObject.name}] Failed to start audio playback. Trying again...");
                audioSource.Play();
            }
            
            Debug.Log($"[{gameObject.name}] Started playing audio: {musicClip.name}");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Cannot play audio: {(audioSource == null ? "AudioSource is null" : "MusicClip is null")}");
        }
    }
    
    private IEnumerator FadeIn()
    {
        float startVolume = 0;
        float targetVolume = volume;
        float currentTime = 0;
        
        Debug.Log($"[{gameObject.name}] Starting fade in from {startVolume} to {targetVolume} over {fadeInTime} seconds");
        
        while (currentTime < fadeInTime)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeInTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        currentState = AudioNPCState.Playing;
        fadeCoroutine = null;
        
        Debug.Log($"[{gameObject.name}] Fade in complete. Volume: {audioSource.volume}");
    }
    
    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float targetVolume = 0;
        float currentTime = 0;
        
        Debug.Log($"[{gameObject.name}] Starting fade out from {startVolume} to {targetVolume} over {fadeOutTime} seconds");
        
        while (currentTime < fadeOutTime)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeOutTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        audioSource.Stop();
        fadeCoroutine = null;
        
        Debug.Log($"[{gameObject.name}] Fade out complete. Audio stopped.");
        
        // Show outro dialogue after fade out completes
        if (dialogueSystem != null && outroDialogueLines != null && outroDialogueLines.Length > 0)
        {
            dialogueSystem.StartDialogue(outroDialogueLines);
            currentState = AudioNPCState.OutroDialogue;
            Debug.Log($"[{gameObject.name}] Started outro dialogue after fade out");
        }
        else
        {
            // If no outro dialogue, go back to initial state
            UnlockPlayerMovement();
            currentState = AudioNPCState.Initial;
            Debug.Log($"[{gameObject.name}] No outro dialogue after fade out, returning to initial state");
        }
    }
    
    private void LockPlayerMovement()
    {
        if (playerController != null)
        {
            // Store current position and rotation
            playerPositionBeforeLock = playerController.transform.position;
            playerRotationBeforeLock = playerController.transform.rotation;
            
            // Get character controller and animator if not already cached
            if (playerCharacterController == null)
            {
                playerCharacterController = playerController.GetComponent<CharacterController>();
            }
            
            if (playerAnimator == null && playerController.modelTransform != null)
            {
                playerAnimator = playerController.modelTransform.GetComponent<Animator>();
            }
            
            // Disable the controller instead of the entire PlayerController component
            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = false;
            }
            
            // Set animation to idle
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isWalking", false);
                playerAnimator.SetBool("isRunning", false);
            }
            
            // Disable the player controller script
            playerController.enabled = false;
            
            Debug.Log($"[{gameObject.name}] Locked player movement at position {playerPositionBeforeLock}");
        }
    }
    
    private void UnlockPlayerMovement()
    {
        if (playerController != null)
        {
            // Re-enable the controller
            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = true;
            }
            
            // Re-enable the player controller script
            playerController.enabled = true;
            
            // Ensure player is still at the locked position
            if (Vector3.Distance(playerController.transform.position, playerPositionBeforeLock) > 0.5f)
            {
                Debug.LogWarning($"[{gameObject.name}] Player moved while locked. Resetting position.");
                
                // Only reset position if player has moved significantly
                if (playerCharacterController != null)
                {
                    playerCharacterController.enabled = false;
                    playerController.transform.position = playerPositionBeforeLock;
                    playerController.transform.rotation = playerRotationBeforeLock;
                    playerCharacterController.enabled = true;
                }
                else
                {
                    playerController.transform.position = playerPositionBeforeLock;
                    playerController.transform.rotation = playerRotationBeforeLock;
                }
            }
            
            Debug.Log($"[{gameObject.name}] Unlocked player movement");
        }
    }
    
    private void Update()
    {
        // Check if dialogue just ended
        if (!dialogueSystem.IsDialogueActive)
        {
            if (currentState == AudioNPCState.IntroDialogue)
            {
                StartMusic();
                currentState = AudioNPCState.FadingIn;
                Debug.Log($"[{gameObject.name}] Dialogue ended, starting music");
            }
            else if (currentState == AudioNPCState.OutroDialogue)
            {
                UnlockPlayerMovement();
                currentState = AudioNPCState.Initial;
                Debug.Log($"[{gameObject.name}] Outro dialogue ended, returning to initial state");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up when destroyed
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    // This can be called from the editor to test audio
    [ContextMenu("Test Audio")]
    private void TestAudio()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            SetupAudioSource();
        }
        
        if (audioSource != null && musicClip != null)
        {
            audioSource.volume = volume;
            audioSource.Play();
            Debug.Log($"[{gameObject.name}] Test playing audio: {musicClip.name}");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Cannot test audio: {(audioSource == null ? "AudioSource is null" : "MusicClip is null")}");
        }
    }
} 
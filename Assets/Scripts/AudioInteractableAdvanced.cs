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
    
    private void Start()
    {
        // Get the Interactable component
        interactable = GetComponent<Interactable>();
        
        if (interactable == null)
        {
            Debug.LogError("AudioInteractableAdvanced requires an Interactable component on the same GameObject");
            return;
        }
        
        // Find the dialogue system if not set in inspector
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            
            if (dialogueSystem == null)
            {
                Debug.LogError("DialogueSystem not found in scene.");
                return;
            }
        }
        
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
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
        
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not found. Player movement locking may not work.");
        }
        
        // Subscribe to the interaction event
        interactable.onInteract.AddListener(HandleInteraction);
    }
    
    private void HandleInteraction()
    {
        switch (currentState)
        {
            case AudioNPCState.Initial:
                // First interaction - show intro dialogue
                if (dialogueSystem != null && introDialogueLines != null && introDialogueLines.Length > 0)
                {
                    LockPlayerMovement();
                    dialogueSystem.StartDialogue(introDialogueLines);
                    currentState = AudioNPCState.IntroDialogue;
                }
                break;
                
            case AudioNPCState.IntroDialogue:
                // After intro dialogue - start playing music
                if (!dialogueSystem.IsDialogueActive)
                {
                    StartMusic();
                    currentState = AudioNPCState.FadingIn;
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
                }
                else
                {
                    // If no outro dialogue, go back to initial state
                    UnlockPlayerMovement();
                    currentState = AudioNPCState.Initial;
                }
                break;
                
            case AudioNPCState.OutroDialogue:
                // After outro dialogue - go back to initial state
                if (!dialogueSystem.IsDialogueActive)
                {
                    UnlockPlayerMovement();
                    currentState = AudioNPCState.Initial;
                }
                break;
        }
    }
    
    private void StartMusic()
    {
        if (audioSource != null && musicClip != null)
        {
            audioSource.volume = 0;
            audioSource.Play();
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
        }
    }
    
    private IEnumerator FadeIn()
    {
        float startVolume = 0;
        float targetVolume = volume;
        float currentTime = 0;
        
        while (currentTime < fadeInTime)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeInTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        currentState = AudioNPCState.Playing;
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float targetVolume = 0;
        float currentTime = 0;
        
        while (currentTime < fadeOutTime)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeOutTime);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        audioSource.Stop();
        fadeCoroutine = null;
        
        // Show outro dialogue after fade out completes
        if (dialogueSystem != null && outroDialogueLines != null && outroDialogueLines.Length > 0)
        {
            dialogueSystem.StartDialogue(outroDialogueLines);
            currentState = AudioNPCState.OutroDialogue;
        }
        else
        {
            // If no outro dialogue, go back to initial state
            UnlockPlayerMovement();
            currentState = AudioNPCState.Initial;
        }
    }
    
    private void LockPlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }
    
    private void UnlockPlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
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
            }
            else if (currentState == AudioNPCState.OutroDialogue)
            {
                UnlockPlayerMovement();
                currentState = AudioNPCState.Initial;
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
} 
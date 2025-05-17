using UnityEngine;
using System.Collections;

public class AudioInteractable : MonoBehaviour
{
    [Tooltip("Reference to the dialogue system")]
    public DialogueSystem dialogueSystem;
    
    [Tooltip("The dialogue lines to display when first interacting")]
    public DialogueLine[] introDialogueLines;
    
    [Tooltip("The dialogue lines to display when stopping the music")]
    public DialogueLine[] outroDialogueLines;
    
    [Tooltip("Audio clip to play")]
    public AudioClip musicClip;
    
    [Tooltip("Volume of the audio")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Should the audio loop?")]
    public bool loop = true;
    
    // Add this script to an object that also has the Interactable component
    private Interactable interactable;
    private AudioSource audioSource;
    
    // State tracking
    private enum AudioNPCState 
    {
        Initial,        // Initial state, ready for first interaction
        IntroDialogue,  // Showing intro dialogue
        Playing,        // Music is playing
        OutroDialogue   // Showing outro dialogue
    }
    
    private AudioNPCState currentState = AudioNPCState.Initial;
    private PlayerController playerController;
    
    private void Start()
    {
        // Get the Interactable component
        interactable = GetComponent<Interactable>();
        
        if (interactable == null)
        {
            Debug.LogError("AudioInteractable requires an Interactable component on the same GameObject");
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
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        
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
                    currentState = AudioNPCState.Playing;
                }
                break;
                
            case AudioNPCState.Playing:
                // While music is playing - stop it and show outro dialogue
                StopMusic();
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
            audioSource.Play();
        }
    }
    
    private void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
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
                currentState = AudioNPCState.Playing;
            }
            else if (currentState == AudioNPCState.OutroDialogue)
            {
                UnlockPlayerMovement();
                currentState = AudioNPCState.Initial;
            }
        }
    }
} 
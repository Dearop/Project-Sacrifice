using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    [Tooltip("UI panel that contains the interaction prompt")]
    public GameObject promptPanel;
    
    [Tooltip("Text component to display the interaction prompt")]
    public TMP_Text promptText;
    
    [Tooltip("Key used for interaction")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("How often to check for interactable objects (seconds)")]
    public float checkInterval = 0.1f;
    
    [Tooltip("Reference to the player's transform - assign your actual player GameObject here")]
    public Transform playerTransform;
    
    [Tooltip("Reference to the dialogue system in the scene")]
    public DialogueSystem dialogueSystem;
    
    // Track all interactable objects in the scene
    private List<Interactable> interactablesInScene = new List<Interactable>();
    
    // Currently closest interactable object in range
    private Interactable currentInteractable;
    
    // Timer for checking interactables
    private float checkTimer;
    
    private void Start()
    {
        // If playerTransform is not set in inspector, try to find it
        if (playerTransform == null)
        {
            // Try using the main camera's parent as fallback (this may not work with third-person cameras)
            playerTransform = Camera.main?.transform.parent;
            
            // If still null, use this transform as a last resort
            if (playerTransform == null)
            {
                playerTransform = transform;
                Debug.LogWarning("Player transform not assigned to InteractionManager. Using the InteractionManager's transform as fallback. For correct behavior, please assign the player's transform in the Inspector.");
            }
        }
        
        // Find dialogue system if not set in inspector
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<DialogueSystem>();
        }
        
        // Find all interactables in the scene at start
        Interactable[] sceneInteractables = FindObjectsOfType<Interactable>();
        interactablesInScene.AddRange(sceneInteractables);
        
        // Make sure prompt is hidden at start
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // If dialogue is active, hide prompt and don't process interactions
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
        {
            HidePrompt();
            return;
        }
        
        // Check for interactables on a timer to optimize performance
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            FindClosestInteractable();
            checkTimer = checkInterval;
        }
        
        // If we have an interactable in range, show prompt and check for input
        if (currentInteractable != null)
        {
            ShowPrompt(currentInteractable.promptText);
            
            // Check for interaction input
            if (Input.GetKeyDown(interactionKey))
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            HidePrompt();
        }
    }
    
    private void FindClosestInteractable()
    {
        float closestDistance = float.MaxValue;
        Interactable closestInteractable = null;
        
        foreach (Interactable interactable in interactablesInScene)
        {
            // Skip destroyed or disabled interactables
            if (interactable == null || !interactable.gameObject.activeInHierarchy) continue;
            
            float distance = Vector3.Distance(playerTransform.position, interactable.transform.position);
            
            // Check if player is within interaction radius
            if (distance <= interactable.interactionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }
        
        // Update the current interactable
        if (currentInteractable != closestInteractable)
        {
            // Clear the previous interactable's in-range status
            if (currentInteractable != null)
            {
                currentInteractable.SetPlayerInRange(false);
            }
            
            // Set the new current interactable
            currentInteractable = closestInteractable;
            
            // Set the new interactable's in-range status
            if (currentInteractable != null)
            {
                currentInteractable.SetPlayerInRange(true);
            }
        }
    }
    
    // Add a new interactable to the tracked list (useful for dynamically spawned objects)
    public void RegisterInteractable(Interactable interactable)
    {
        if (!interactablesInScene.Contains(interactable))
        {
            interactablesInScene.Add(interactable);
        }
    }
    
    // Remove an interactable from the tracked list
    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactablesInScene.Contains(interactable))
        {
            interactablesInScene.Remove(interactable);
            
            // If this was the current interactable, clear it
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                HidePrompt();
            }
        }
    }
    
    private void ShowPrompt(string text)
    {
        // Only show prompt if no dialogue is active
        if (dialogueSystem == null || !dialogueSystem.IsDialogueActive)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
                
                if (promptText != null)
                {
                    promptText.text = text;
                }
            }
        }
    }
    
    private void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
} 
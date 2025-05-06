using UnityEngine;

public class DialogueInteractable : MonoBehaviour
{
    [Tooltip("Reference to the dialogue system")]
    public DialogueSystem dialogueSystem;
    
    [Tooltip("The dialogue lines to display when this object is interacted with")]
    public DialogueLine[] dialogueLines;
    
    // Add this script to an object that also has the Interactable component
    private Interactable interactable;
    
    private void Start()
    {
        // Get the Interactable component
        interactable = GetComponent<Interactable>();
        
        if (interactable == null)
        {
            Debug.LogError("DialogueInteractable requires an Interactable component on the same GameObject");
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
        
        // Subscribe to the interaction event
        interactable.onInteract.AddListener(StartDialogue);
    }
    
    private void StartDialogue()
    {
        if (dialogueSystem != null && dialogueLines != null && dialogueLines.Length > 0)
        {
            dialogueSystem.StartDialogue(dialogueLines);
        }
    }
} 
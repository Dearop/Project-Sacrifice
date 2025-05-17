using UnityEngine;
using System.Collections;

public class BarmanDialogue : MonoBehaviour
{
    [Tooltip("Reference to the dialogue system")]
    public DialogueSystem dialogueSystem;
    
    [Tooltip("Reference to the player's DrunkEffect component")]
    public DrunkEffect playerDrunkEffect;
    
    [Tooltip("The dialogue lines to display when this object is interacted with")]
    public DialogueLine[] dialogueLines;
    
    private Interactable interactable;
    private bool hasTriggeredDrunkEffect = false;
    
    private void Start()
    {
        // Get the Interactable component
        interactable = GetComponent<Interactable>();
        
        if (interactable == null)
        {
            Debug.LogError("BarmanDialogue requires an Interactable component on the same GameObject");
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
        
        // Find the player's DrunkEffect if not set in inspector
        if (playerDrunkEffect == null)
        {
            playerDrunkEffect = FindObjectOfType<DrunkEffect>();
            
            if (playerDrunkEffect == null)
            {
                Debug.LogError("Player's DrunkEffect not found in scene.");
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
            
            // Start the drunk effect timer if not already triggered
            if (!hasTriggeredDrunkEffect)
            {
                Debug.Log("Starting drunk effect sequence");
                StartCoroutine(TriggerDrunkEffect());
                hasTriggeredDrunkEffect = true;
            }
        }
    }
    
    private IEnumerator TriggerDrunkEffect()
    {
        // Wait for the dialogue to finish
        while (dialogueSystem.IsDialogueActive)
        {
            yield return null;
        }
        
        Debug.Log("Dialogue finished, showing drunk message");
        
        // Show drunk dialogue
        DialogueLine[] drunkDialogue = new DialogueLine[]
        {
            new DialogueLine { speakerName = "System", dialogueText = "Oh No! You are drunk." }
        };
        
        dialogueSystem.StartDialogue(drunkDialogue);
        
        // Start the drunk effect
        if (playerDrunkEffect != null)
        {
            Debug.Log("Triggering drunk effect on player");
            playerDrunkEffect.StartDrunkEffect();
        }
        else
        {
            Debug.LogError("Player DrunkEffect reference is null!");
        }
    }
} 
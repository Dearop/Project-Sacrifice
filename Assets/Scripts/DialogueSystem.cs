using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Tooltip("The panel containing the dialogue UI elements")]
    public GameObject dialoguePanel;
    
    [Tooltip("Text component for the dialogue content")]
    public TMP_Text dialogueText;
    
    [Tooltip("Text component for the character name (optional)")]
    public TMP_Text nameText;
    
    [Tooltip("Button to continue to next dialogue line or close dialogue")]
    public Button continueButton;
    
    [Tooltip("How fast characters appear in the text box (characters per second)")]
    public float typingSpeed = 40f;
    
    [Tooltip("Key used to advance dialogue (should match InteractionManager's key)")]
    public KeyCode advanceKey = KeyCode.E;
    
    // The current dialogue being displayed
    private Queue<DialogueLine> currentDialogueLines = new Queue<DialogueLine>();
    
    // Are we currently showing dialogue?
    private bool isDialogueActive = false;
    
    // Are we currently typing text?
    private bool isTyping = false;
    
    // The coroutine for typing text
    private Coroutine typingCoroutine;
    
    // Track the current dialogue line being displayed
    private DialogueLine currentLine;
    
    // Public property to check if dialogue is active
    public bool IsDialogueActive => isDialogueActive;
    
    private void Start()
    {
        // Ensure dialogue panel is hidden at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // Setup continue button listener
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(DisplayNextLine);
        }
        
        // Try to get the interaction key from the InteractionManager if it exists
        InteractionManager interactionManager = FindObjectOfType<InteractionManager>();
        if (interactionManager != null)
        {
            advanceKey = interactionManager.interactionKey;
        }
    }
    
    private void Update()
    {
        // Check for key press to advance dialogue when dialogue is active
        if (isDialogueActive && Input.GetKeyDown(advanceKey))
        {
            DisplayNextLine();
        }
    }
    
    // Start a dialogue sequence with the given lines
    public void StartDialogue(DialogueLine[] lines)
    {
        // Don't restart dialogue if already active
        if (isDialogueActive)
        {
            return;
        }
        
        // Clear any existing dialogue
        currentDialogueLines.Clear();
        currentLine = null;
        
        // Queue up all the lines
        foreach (DialogueLine line in lines)
        {
            currentDialogueLines.Enqueue(line);
        }
        
        // Show the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        isDialogueActive = true;
        
        // Display the first line
        DisplayNextLine();
    }
    
    // Display the next line in the queue
    public void DisplayNextLine()
    {
        // If we're still typing, complete the current line
        if (isTyping)
        {
            CompleteTyping();
            return;
        }
        
        // If no more lines, end dialogue
        if (currentDialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        // Get the next line
        currentLine = currentDialogueLines.Dequeue();
        
        // Display character name if available
        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(currentLine.speakerName) ? "" : currentLine.speakerName;
        }
        
        // Start typing the dialogue text
        if (dialogueText != null)
        {
            typingCoroutine = StartCoroutine(TypeText(currentLine.dialogueText));
        }
    }
    
    // Type text character by character for a typing effect
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        float timePerChar = 1f / typingSpeed;
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(timePerChar);
        }
        
        isTyping = false;
    }
    
    // Immediately complete the current typing
    private void CompleteTyping()
    {
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            
            if (dialogueText != null && currentLine != null)
            {
                dialogueText.text = currentLine.dialogueText;
            }
            
            isTyping = false;
        }
    }
    
    // End the dialogue and hide the panel
    public void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        isDialogueActive = false;
        currentLine = null;
    }
}

// Structure to hold a single line of dialogue
[System.Serializable]
public class DialogueLine
{
    [Tooltip("The name of the character speaking")]
    public string speakerName;
    
    [TextArea(3, 10)]
    [Tooltip("The dialogue text to display")]
    public string dialogueText;
} 
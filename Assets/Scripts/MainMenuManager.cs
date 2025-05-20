using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text pressEToStartText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    
    [Header("Settings")]
    [SerializeField] private float blinkInterval = 0.8f;
    [SerializeField] private string barSceneName = "BarScene";
    [SerializeField] private KeyCode startKey = KeyCode.E;
    
    [Header("Dialogue Settings")]
    [SerializeField, TextArea(3, 10)] private string introDialogue = "Your mother is gone. In the quiet aftermath, you discover her old notebook—its pages worn, her handwriting gentle and familiar.\n\n" +
    "There, among memories, you find a single melody. She played it for you when you were young, a song now lost to time and grief.\n\n" +
    "But hope lingers. The local bar still has a piano, waiting in the dim light.\n\n" +
    "Go. Play her melody. Remember.";
    
    private bool mainMenuActive = true;
    private bool dialogueActive = false;
    
    private void Start()
    {
        // Start the blinking coroutine
        StartCoroutine(BlinkText());
        
        // Hide dialogue panel at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            if (mainMenuActive)
            {
                ShowDialogue();
            }
            else if (dialogueActive)
            {
                LoadBarScene();
            }
        }
    }
    
    private IEnumerator BlinkText()
    {
        while (mainMenuActive)
        {
            pressEToStartText.enabled = !pressEToStartText.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    
    private void ShowDialogue()
    {
        // Hide the main menu elements
        mainMenuActive = false;
        pressEToStartText.enabled = false;
        
        // Show dialogue panel
        dialoguePanel.SetActive(true);
        dialogueText.text = introDialogue;
        dialogueActive = true;
    }
    
    private void LoadBarScene()
    {
        // Load the bar scene
        SceneManager.LoadScene(barSceneName);
    }
} 
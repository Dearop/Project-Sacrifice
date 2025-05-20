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
    [SerializeField, TextArea(3, 10)] private string introDialogue = "Your mother just passed away and you found her notebook... It has a single melody she used to play for you when you were young, it seems lost to you now though. The local bar had a piano you could use, go play the melody...";
    
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
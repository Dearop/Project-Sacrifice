using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PianoGameCompleteManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private float delayBeforeTransition = 2f;
    
    [Header("Events")]
    [Tooltip("Set to true when the piano minigame is completed to trigger the transition")]
    [SerializeField] private bool gameCompleted = false;
    
    private bool transitionStarted = false;
    
    private void Update()
    {
        // Check if the game is completed and transition hasn't started yet
        if (gameCompleted && !transitionStarted)
        {
            StartCoroutine(TransitionToEndScene());
            transitionStarted = true;
        }
    }
    
    // Call this method from your minigame when it completes
    public void OnGameComplete()
    {
        gameCompleted = true;
    }
    
    private IEnumerator TransitionToEndScene()
    {
        // Wait for the specified delay before transitioning
        yield return new WaitForSeconds(delayBeforeTransition);
        
        // Load the end scene
        SceneManager.LoadScene(endSceneName);
    }
} 
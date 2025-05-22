using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text thanksText;
    [SerializeField] private TMP_Text creditsText;
    
    [Header("Audio")]
    [SerializeField] private AudioClip endMusic;
    [SerializeField] private float volume = 0.5f;
    
    [Header("Settings")]
    [SerializeField, TextArea(3, 10)] private string thanksMessage = "Thanks For Playing our demo...";
    [SerializeField, TextArea(5, 15)] private string creditsMessage = "Created by:\n\nYour Team Names Here\n\nMusic by:\n\nYour Music Credits Here";
    
    private SmoothLoopingAudioPlayer audioPlayer;
    
    private void Start()
    {
        // Set up text
        if (thanksText != null)
        {
            thanksText.text = thanksMessage;
        }
        
        if (creditsText != null)
        {
            creditsText.text = creditsMessage;
        }
        
        // Set up music
        SetupMusic();
    }
    
    private void SetupMusic()
    {
        // Get or add the SmoothLoopingAudioPlayer component
        audioPlayer = GetComponent<SmoothLoopingAudioPlayer>();
        if (audioPlayer == null)
        {
            audioPlayer = gameObject.AddComponent<SmoothLoopingAudioPlayer>();
        }
        
        // Configure the player
        if (endMusic != null)
        {
            audioPlayer.SetMusicClip(endMusic, true);
            audioPlayer.SetVolume(volume);
        }
    }
} 
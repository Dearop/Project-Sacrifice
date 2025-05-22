using UnityEngine;

public class EnhancedMainMenuMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volume = 0.5f;

    private SmoothLoopingAudioPlayer audioPlayer;

    void Awake()
    {
        // Get or add the SmoothLoopingAudioPlayer component
        audioPlayer = GetComponent<SmoothLoopingAudioPlayer>();
        if (audioPlayer == null)
        {
            audioPlayer = gameObject.AddComponent<SmoothLoopingAudioPlayer>();
        }
        
        // Configure the player
        if (musicClip != null)
        {
            audioPlayer.SetMusicClip(musicClip, true);
            audioPlayer.SetVolume(volume);
        }
    }
} 
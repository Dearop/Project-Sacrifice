using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SmoothLoopingAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volume = 0.5f;
    
    [Header("Loop Settings")]
    [Tooltip("Start time (in seconds) for the loop point")]
    [SerializeField] private float loopStartTime = 0f;
    
    [Tooltip("End time (in seconds) for the loop point (0 = use full clip length)")]
    [SerializeField] private float loopEndTime = 0f;
    
    [Tooltip("Time (in seconds) before loop end to start fading out")]
    [SerializeField] private float crossfadeDuration = 1.0f;
    
    [Tooltip("Whether to use a smooth crossfade between loop points")]
    [SerializeField] private bool useCrossfade = true;
    
    private AudioSource primarySource;
    private AudioSource secondarySource;
    private bool isPrimaryPlaying = true;
    private bool isLooping = false;
    private float musicLength;

    private void Awake()
    {
        // Create primary audio source
        primarySource = GetComponent<AudioSource>();
        ConfigureAudioSource(primarySource);
        
        // Create secondary audio source for crossfading
        secondarySource = gameObject.AddComponent<AudioSource>();
        ConfigureAudioSource(secondarySource);
        secondarySource.volume = 0; // Start at zero volume
        
        // Set the correct loop end time if it's 0
        if (loopEndTime <= 0 && musicClip != null)
        {
            loopEndTime = musicClip.length;
        }
        
        musicLength = (musicClip != null) ? musicClip.length : 0f;
    }

    private void Start()
    {
        if (musicClip != null)
        {
            primarySource.Play();
            isLooping = true;
        }
        else
        {
            Debug.LogWarning("SmoothLoopingAudioPlayer: No music clip assigned!");
        }
    }

    private void Update()
    {
        if (!isLooping || musicClip == null) return;
        
        AudioSource currentSource = isPrimaryPlaying ? primarySource : secondarySource;
        AudioSource nextSource = isPrimaryPlaying ? secondarySource : primarySource;
        
        // Check if we need to prepare for looping
        if (useCrossfade && currentSource.time >= loopEndTime - crossfadeDuration && nextSource.time == 0)
        {
            // Start the next source at the loop start position
            nextSource.time = loopStartTime;
            nextSource.Play();
            
            // Start the crossfade
            StartCrossfade();
        }
        // Without crossfade, just check if we've reached the end
        else if (!useCrossfade && currentSource.time >= loopEndTime)
        {
            // Reset to loop start
            currentSource.time = loopStartTime;
        }
    }
    
    private void StartCrossfade()
    {
        // Start a coroutine for the crossfade
        StartCoroutine(CrossfadeAudio());
    }
    
    private System.Collections.IEnumerator CrossfadeAudio()
    {
        AudioSource fadeOutSource = isPrimaryPlaying ? primarySource : secondarySource;
        AudioSource fadeInSource = isPrimaryPlaying ? secondarySource : primarySource;
        
        float startVolume = volume;
        float timer = 0;
        
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;
            
            fadeOutSource.volume = startVolume * (1 - t);
            fadeInSource.volume = startVolume * t;
            
            yield return null;
        }
        
        // Ensure the volumes are set correctly at the end
        fadeOutSource.volume = 0;
        fadeInSource.volume = startVolume;
        
        // Stop the faded-out source
        fadeOutSource.Stop();
        fadeOutSource.time = 0;
        
        // Swap the active source
        isPrimaryPlaying = !isPrimaryPlaying;
    }
    
    private void ConfigureAudioSource(AudioSource source)
    {
        source.clip = musicClip;
        source.volume = volume;
        source.loop = !useCrossfade; // Only set loop to true if not using crossfade
        source.playOnAwake = false;
    }
    
    // Public method to set the music clip at runtime
    public void SetMusicClip(AudioClip clip, bool playImmediately = true)
    {
        if (clip == null) return;
        
        musicClip = clip;
        primarySource.clip = clip;
        secondarySource.clip = clip;
        
        // Update music length
        musicLength = clip.length;
        
        // Set default loop end if not specified
        if (loopEndTime <= 0)
        {
            loopEndTime = musicLength;
        }
        
        if (playImmediately)
        {
            // Stop any playing audio
            primarySource.Stop();
            secondarySource.Stop();
            
            // Reset sources
            primarySource.time = 0;
            secondarySource.time = 0;
            primarySource.volume = volume;
            secondarySource.volume = 0;
            
            // Start playing
            primarySource.Play();
            isPrimaryPlaying = true;
            isLooping = true;
        }
    }
    
    // Public method to adjust volume
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        
        if (isPrimaryPlaying)
        {
            primarySource.volume = volume;
        }
        else
        {
            secondarySource.volume = volume;
        }
    }
} 
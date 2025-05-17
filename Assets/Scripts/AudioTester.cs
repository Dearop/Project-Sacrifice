using UnityEngine;

public class AudioTester : MonoBehaviour
{
    public AudioClip testClip;
    public float volume = 1.0f;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        if (testClip != null)
        {
            audioSource.clip = testClip;
            Debug.Log($"AudioTester: Audio clip assigned: {testClip.name}, Length: {testClip.length}s");
        }
        else
        {
            Debug.LogError("AudioTester: No audio clip assigned!");
        }
    }
    
    void Update()
    {
        // Press Space to play the test clip
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("AudioTester: Stopped audio");
            }
            else
            {
                audioSource.Play();
                Debug.Log($"AudioTester: Playing audio clip: {testClip.name}");
            }
        }
    }
} 
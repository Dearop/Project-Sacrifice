using UnityEngine;

public class MainMenuMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float volume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = true;
        audioSource.Play();
    }
} 
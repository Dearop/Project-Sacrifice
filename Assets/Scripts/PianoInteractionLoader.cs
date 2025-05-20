using UnityEngine;
using UnityEngine.SceneManagement;

public class PianoInteractionLoader : MonoBehaviour
{
    [SerializeField] private string pianoSceneName = "PianoScene";

    // This method can be called by the interaction system or hooked up in the Inspector
    public void Interact()
    {
        SceneManager.LoadScene(pianoSceneName);
    }
} 
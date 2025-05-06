using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("How close the player needs to be to interact")]
    public float interactionRadius = 2f;
    
    [Tooltip("Text to display in the interaction popup")]
    public string promptText = "Press E to interact";
    
    [Tooltip("Event triggered when player interacts with this object")]
    public UnityEvent onInteract;
    
    private bool playerInRange = false;
    
    // Used by InteractionManager to check if player is in range
    public bool PlayerInRange => playerInRange;
    
    // Called by InteractionManager when player presses the interaction key
    public void Interact()
    {
        onInteract?.Invoke();
    }
    
    // For debugging - visualize the interaction radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    
    // These methods will be called by the InteractionManager
    public void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;
    }
} 
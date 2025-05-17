using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [Tooltip("Player walking speed (Running speed will multiply it for 2).")]
  [Range(0, Mathf.Infinity)]
  public float speed = 10.0f;
  [Tooltip("The gravity force that will be applied to you Player to prevent it goes off the ground when going down a ramp or change angles abruptly.")]
  [Range(0, Mathf.Infinity)]
  public float gravity = 100f;
  [Tooltip("Player's Gameobject rotation speed when changing directions.")]
  [Range(0, Mathf.Infinity)]
  public float rotationSpeed = 360f;
  [Tooltip("Reference to the child GameObject that contains the Animator component")]
  public Transform modelTransform;

  // Components
  private CharacterController controller;
  private Animator animator;
  private DrunkEffect drunkEffect;

  // Attributes
  private bool isWalking = false;
  private bool isRunning = false;
  private float gravityInfluence;
  private float effectiveSpeed;
  private Vector3 movement;

  void Start()
  {
    gravityInfluence = gravity * Time.deltaTime * -1;
    controller = GetComponent<CharacterController>();
    drunkEffect = GetComponent<DrunkEffect>();
    
    // Get animator from child model GameObject
    if (modelTransform != null)
    {
      animator = modelTransform.GetComponent<Animator>();
    }
    else
    {
      // Try to find animator in children if model reference is not set
      animator = GetComponentInChildren<Animator>();
      Debug.LogWarning("Model Transform not set. Using GetComponentInChildren to find Animator.");
    }
    
    if (animator == null)
    {
      Debug.LogError("Animator component not found in children. Please assign the model GameObject.");
    }
  }

  void Update()
  {
    // Set all values for movement and animation.
    UpdatePlayerValues();
    PlayerMovement();
    AnimationManagement();
  }

  private void UpdatePlayerValues()
  {
    isWalking = getIsWalking();
    isRunning = getIsRunning();
    effectiveSpeed = getEffectiveSpeed();

    float horizontalAxis = Input.GetAxis("Horizontal");
    float verticalAxis = Input.GetAxis("Vertical");

    // Flip controls if drunk
    if (drunkEffect != null && drunkEffect.IsDrunk())
    {
      Debug.Log("Controls are flipped due to drunk effect");
      horizontalAxis = -horizontalAxis;
      verticalAxis = -verticalAxis;
    }

    movement = new Vector3(horizontalAxis, gravityInfluence, verticalAxis);
  }

  private void PlayerMovement()
  {
    controller.Move(movement * effectiveSpeed * Time.deltaTime);

    // Handle Gameobject smoothly rotation.
    if (isWalking)
    {
      movement.y = 0;
      Quaternion toRotation = Quaternion.LookRotation(movement);
      transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }
  }

  private void AnimationManagement()
  {
    animator.SetBool("isWalking", isWalking);
    animator.SetBool("isRunning", isRunning);
  }

  private bool getIsWalking()
  {
    return controller.velocity.magnitude > 0;
  }

  private bool getIsRunning()
  {
    return isWalking && Input.GetKey("left shift");
  }

  private float getEffectiveSpeed()
  {
    return isRunning ? speed * 2 : speed;
  }
}
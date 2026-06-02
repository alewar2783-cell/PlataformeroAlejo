using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// We use reflection/component checks or assume Cinemachine is present.
// Note: If you get a compile error here, ensure the Cinemachine package is installed
// via the Package Manager.
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 14f;
    public float acceleration = 10f;
    
    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public int maxJumps = 2; // Supports double jump
    
    [Header("Drag & Physics")]
    public float groundDrag = 5f;
    public float playerHeight = 2f;
    public LayerMask whatIsGround;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Cinemachine FOV")]
    public CinemachineVirtualCamera cam;
    public float baseFOV = 60f;
    public float sprintFOV = 80f;
    public float fovTransitionSpeed = 5f;

    [Header("References")]
    public Transform orientation;
    private PlayerStamina playerStamina;
    private Rigidbody rb;

    // State Variables
    public enum MovementState { Walking, Sprinting, Air, Dashing, Wallrunning }
    public MovementState state;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    
    private bool grounded;
    private bool readyToJump = true;
    private int jumpsLeft;
    private float moveSpeed;

    [Header("Stamina Costs")]
    public float sprintStaminaCost = 15f;

    // Expose for other scripts (Dash, Wallrun) to override states
    public bool dashing;
    public bool wallrunning;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        playerStamina = GetComponent<PlayerStamina>();
        readyToJump = true;
        jumpsLeft = maxJumps;
        moveSpeed = walkSpeed;
    }

    private void Update()
    {
        // Ground check using a raycast from the center of the player downwards
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        UpdateCameraFOV();

        // Handle drag
        if (grounded && !dashing)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jumping
        if (Input.GetKeyDown(jumpKey) && readyToJump && jumpsLeft > 0)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.Dashing;
            return;
        }

        if (wallrunning)
        {
            state = MovementState.Wallrunning;
            return;
        }

        if (grounded && Input.GetKey(sprintKey) && playerStamina != null && playerStamina.currentStamina > 0)
        {
            state = MovementState.Sprinting;
            // Smoothly accelerate to sprint speed
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
            playerStamina.ConsumeStaminaContinuous(sprintStaminaCost);
        }
        else if (grounded)
        {
            state = MovementState.Walking;
            // Smoothly decelerate to walk speed
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
            
            // Regenerate stamina when just walking/idle on ground
            if (playerStamina != null)
                playerStamina.RegenerateStamina();
        }
        else
        {
            state = MovementState.Air;
        }

        // Reset double jumps when grounded
        if (grounded && readyToJump)
        {
            jumpsLeft = maxJumps;
        }
    }

    private void MovePlayer()
    {
        if (dashing) return; // Dash controls its own movement
        if (wallrunning) return; // Wallrun controls its own movement

        // Calculate movement direction relative to where the player is looking
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();

        if (grounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            // In air, we apply multiplier to keep some control, but not full ground speed
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        if (dashing) return; // Don't limit speed during a dash

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        jumpsLeft--;

        // If we jump during a dash or wallrun, we want to maintain the momentum, 
        // so we don't necessarily reset Y velocity if we are wallrunning.
        // But normally for a consistent jump height (especially double jump), we reset Y velocity.
        
        // Reset Y velocity to ensure double jump reaches exact same height
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void UpdateCameraFOV()
    {
        if (cam == null) return;

        float targetFOV = (state == MovementState.Sprinting || state == MovementState.Dashing) ? sprintFOV : baseFOV;
        cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
    }
}

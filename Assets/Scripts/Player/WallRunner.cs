using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStamina))]
public class WallRunner : MonoBehaviour
{
    [Header("Wall Running Settings")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 200f;
    public float wallJumpUpForce = 12f;
    public float wallJumpSideForce = 15f;
    public float wallRunStaminaCost = 15f;

    [Header("Detection")]
    public float wallCheckDistance = 0.7f;
    public float minJumpHeight = 1.5f;

    [Header("References")]
    public Transform orientation;
    
    private Rigidbody rb;
    private PlayerMovement pm;
    private PlayerStamina ps;

    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        ps = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
        {
            WallRunMovement();
        }
    }

    private void CheckForWall()
    {
        // Check for walls to the left and right
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        // State 1: Start Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !pm.wallrunning)
        {
            if (ps.currentStamina > 0)
            {
                StartWallRun();
            }
        }
        // State 2: Wallrunning
        else if (pm.wallrunning)
        {
            if (!ps.ConsumeStaminaContinuous(wallRunStaminaCost) || !(wallLeft || wallRight) || verticalInput <= 0 || !AboveGround())
            {
                // Stop if out of stamina, lost the wall, stopped pressing forward, or too close to ground
                StopWallRun();
            }

            // Wall Jump
            if (Input.GetKeyDown(pm.jumpKey))
            {
                WallJump();
            }
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
        
        // Zero out gravity so the player doesn't fall (or falls very slowly)
        // We will just disable gravity completely as requested: "gravity is ignored completely"
        rb.useGravity = false;
        
        // Reset Y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    private void WallRunMovement()
    {
        // Find wall normal
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        // Find forward direction along the wall
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        // Fix direction based on where the player is looking
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        // Apply forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // Weak force to keep the player attached to the wall
        if (!(wallLeft && Input.GetAxisRaw("Horizontal") > 0) && !(wallRight && Input.GetAxisRaw("Horizontal") < 0))
        {
            rb.AddForce(-wallNormal * 100f, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
        
        // Re-enable gravity unless we are dashing
        if (!pm.dashing)
        {
            rb.useGravity = true;
        }
    }

    private void WallJump()
    {
        // Jumping off the wall pushes the player forward and away from the wall's normal
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // Ensure we stop wall running immediately to apply gravity again
        StopWallRun();

        // Reset y velocity for consistent height
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply forces
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        
        // Add orientation forward as requested: "pushes the player forward and away"
        forceToApply += orientation.forward * (wallJumpSideForce * 0.5f);
        
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}

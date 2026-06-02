using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashForce = 30f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;
    public float dashStaminaCost = 25f;
    public KeyCode dashKey = KeyCode.E;

    [Header("References")]
    public Transform orientation;
    
    private Rigidbody rb;
    private PlayerMovement pm;
    private PlayerStamina ps;
    
    private float dashCooldownTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        ps = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(dashKey))
        {
            Dash();
        }
    }

    private void Dash()
    {
        if (dashCooldownTimer > 0) return;
        
        if (!ps.ConsumeStaminaBurst(dashStaminaCost))
        {
            // Not enough stamina
            return;
        }

        dashCooldownTimer = dashCooldown;
        pm.dashing = true;

        // Calculate dash direction based on input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 direction = orientation.forward * vertical + orientation.right * horizontal;
        direction.Normalize();

        // If no input, default to forward
        if (direction == Vector3.zero)
            direction = orientation.forward;

        // Reset velocity to ensure exact dash burst
        rb.velocity = Vector3.zero;

        // Ignore gravity during the dash
        rb.useGravity = false;

        // Apply dash force
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        pm.dashing = false;
        rb.useGravity = true;
    }
}

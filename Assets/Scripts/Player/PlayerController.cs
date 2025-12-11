using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D playerCollider;
    [Header("Movement Settings")]
    [SerializeField] float baseSpeed = 10f;
    [SerializeField] float currentSpeed;
    [SerializeField] float acceleration = 40f;
    [SerializeField] float breakingForce = 15f;
    [SerializeField] float stickToGroundForce = 15f;
    private float horizontal;
    [Header("Sprint")]
    [SerializeField] float sprintModifier;
    [SerializeField] bool isSprinting = false;
    [Header("Dashing")]
    [SerializeField] float dashPower = 30f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] int maxDashCharges = 1;
    [SerializeField] float dashRechargeTime = 2f;
    private float currentDashCharges;
    private bool isDashing = false; 
    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform playerTransform;
    [SerializeField] private bool isGrounded;
    [SerializeField] private Vector2 groundNormal;
    [Header("Jumping")]
    [SerializeField] float jumpingPower;
    [SerializeField] private float minGroundedTime = 0.1f;
    private float groundedTimeCounter = 0f;
    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [Header("State Control")]
    [SerializeField] private int movementLockCounter = 0;
    [Header("Spawn Point")]
    [SerializeField] Transform spawnPoint;
    private void Start()
    {
        currentDashCharges = maxDashCharges;
        gameObject.transform.position = spawnPoint.position;

    }
    private void FixedUpdate()
    {
        CheckGround();
        currentSpeed= isSprinting ? baseSpeed+sprintModifier : baseSpeed;
        if (movementLockCounter==0)
        {
            if (horizontal != 0)
            {
                rb.AddForce(new Vector2((((horizontal * currentSpeed) - rb.linearVelocity.x) * rb.mass * acceleration), 0));
            }
            else if (isGrounded)
            {
                rb.AddForce(new Vector2(-rb.linearVelocity.x * rb.mass * breakingForce, 0));
            }
        }
        if(isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        if (isGrounded)
        {
            groundedTimeCounter += Time.fixedDeltaTime; 
        }
        else
        {
            groundedTimeCounter = 0f; 
        }
        if (isGrounded)
        {
            if(rb.linearVelocity.y <= 0.1f) 
               { 
                    rb.AddForce(Vector2.down * stickToGroundForce, ForceMode2D.Force); 
               }
        }
    }
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }
    public void Jump(InputAction.CallbackContext context)
    {
        bool canJumpFromGrounded = isGrounded && groundedTimeCounter >= minGroundedTime;
        if (context.performed && (canJumpFromGrounded || coyoteTimeCounter>0))
        {
            coyoteTimeCounter = 0f;
            groundedTimeCounter = 0f;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(groundNormal * jumpingPower * rb.mass, ForceMode2D.Impulse);
        }
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isSprinting = true;
        }
        else if(context.canceled)
        {
            isSprinting = false;
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && horizontal != 0 && !isDashing)
        {
            if (!isGrounded && currentDashCharges == 0) return;
            isDashing = true;
            movementLockCounter++;
            if(!isGrounded)
            {
                currentDashCharges--;
                if (currentDashCharges < maxDashCharges)
                    StartCoroutine(RechargeHandler(
                          () => currentDashCharges,
                          (charge) => currentDashCharges = charge,
                          maxDashCharges,
                          dashRechargeTime));
            }

            Vector2 dashDirection = new Vector2(horizontal, 0).normalized;
            rb.AddForce(dashDirection * rb.mass * dashPower, ForceMode2D.Impulse);
            StartCoroutine(StopDash());
        }
    }

    private IEnumerator RechargeHandler(System.Func<float> getCurrent, System.Action<float> setCurrent,float maxCharges,float rechargeTime)
    {
        while(getCurrent()<maxCharges)
        {
            yield return new WaitForSeconds(rechargeTime);
            float newCharge = getCurrent() + 1f;
            setCurrent(Mathf.Clamp(newCharge,0f,maxCharges));
        }
    }
    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        movementLockCounter--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y);
    }
  
    public void CheckGround()
    {
        float castHeight = 0.05f;
        float castDistance = 0.01f;
        float castWidth = playerCollider.bounds.size.x*0.9f;
        Vector2 castCenter = playerCollider.bounds.center;
        castCenter.y -= playerCollider.bounds.extents.y + (castHeight / 2);
        RaycastHit2D hit = Physics2D.BoxCast(castCenter, new Vector2(castWidth, castHeight),0,Vector2.down,castDistance,groundLayer);
        if(hit)
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector2.up;
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Ensure we have a collider to work with
        if (playerCollider == null) return;

        // --- RECALCULATE THE EXACT PARAMETERS USED IN CheckGround() ---

        // Parameters are pulled directly from your CheckGround() logic:
        float castHeight = 0.05f;
        float castWidth = playerCollider.bounds.size.x * 0.9f;
        float castDistance = 0.01f;

        // 1. Calculate the center of the checking box (Start Point)
        Vector2 castCenter = playerCollider.bounds.center;
        // Offset down by half the collider height PLUS half the cast height
        castCenter.y -= playerCollider.bounds.extents.y + (castHeight / 2);

        // The size of the box
        Vector2 castSize = new Vector2(castWidth, castHeight);

        // The End Point of the sweep (where the box stops)
        Vector3 castEnd = castCenter + Vector2.down * castDistance;

        // --- DRAWING THE GIZMOS ---

        // Set the color for the Gizmos
        Gizmos.color = Color.green;

        // 1. Draw the initial box (at the start of the sweep)
        // This shows where the BoxCast starts right at the player's feet.
        Gizmos.DrawWireCube(castCenter, castSize);

        // 2. Set the color for the swept area
        Gizmos.color = Color.yellow;

        // 3. Draw the swept area (The box at the end of the 0.01f cast distance)
        Gizmos.DrawWireCube(castEnd, castSize);

        // 4. Draw a line connecting the start and end points of the sweep
        Gizmos.DrawLine(castCenter, castEnd);

        // 5. If we have a ground normal from the last check, visualize it (Optional)
        if (isGrounded)
        {
            Gizmos.color = Color.red;
            // Draw the ground normal vector from the center of the player
            Gizmos.DrawLine(playerCollider.bounds.center, playerCollider.bounds.center + (Vector3)groundNormal);
        }
    }
}

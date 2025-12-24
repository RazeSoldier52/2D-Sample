using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
public class PlayerController : MonoBehaviour, IBoundaryBehaviour
{

    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D playerCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private Vector3 baseScale;
    [SerializeField] private PlayerStateProfile state;
    [Header("Movement Settings")]
    [SerializeField] float acceleration = 40f;
    [SerializeField] float breakingForce = 15f;
    [SerializeField] float stickToGroundForce = 15f;
    private float horizontal;
    [Header("Dashing")]
    [SerializeField] float dashPower = 30f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] int maxDashCharges = 1;
    [SerializeField] float dashRechargeTime = 2f;
    private float currentDashCharges;
    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] private Vector2 groundNormal;
    [SerializeField] private float minNormalYThreshold = 0.7f;
    [SerializeField] private float xSpeed;
    [SerializeField] private float ySpeed;
    [Header("Jumping")]
    [SerializeField] float jumpingPower;
    [SerializeField] private float minGroundedTime = 0.1f;
    private float groundedTimeCounter = 0f;
    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [Header("Spawn Point")]
    [SerializeField] Transform spawnPoint;

    [SerializeField] public GameObject SwordAttack;

    private void Start()
    {
        currentDashCharges = maxDashCharges;
        gameObject.transform.position = spawnPoint.position;
        baseScale = transform.localScale;
    }
    private void FixedUpdate()
    {
        CheckGround();
        if (horizontal != 0 && state.canMove)
        {
            Vector2 movementDirection = Vector2.right;

            if (state.vertical == VerticalState.Grounded && groundNormal.y >= minNormalYThreshold)
            {
                // Calculate the slope-parallel vector
                movementDirection = Vector2.Perpendicular(groundNormal);
            }

            // 2. Adjust the vector to point exactly in the INPUT direction
            // Check the dot product to see if the vector already points in the input direction (e.g., Right).
            // If the dot product is negative, the direction vector is opposite to the input, so flip it.
            if (Vector2.Dot(movementDirection, Vector2.right) * horizontal < 0)
            {
                movementDirection *= -1;
            }

            // 3. Calculate ABSOLUTE speed and use the vector for direction
            float absoluteTargetSpeed = state.currentSpeed; // No 'horizontal' multiplier here

            // 4. Calculate current speed ALONG the movementDirection vector
            float currentSpeedAlongDirection = Vector2.Dot(rb.linearVelocity, movementDirection);

            // 5. Calculate the force needed to reach the ABSOLUTE speed
            float forceMagnitude = (absoluteTargetSpeed - currentSpeedAlongDirection) * rb.mass * acceleration;

            // 6. Apply the force along the calculated movementDirection vector
            rb.AddForce(movementDirection * forceMagnitude * Mathf.Abs(horizontal), ForceMode2D.Force);
            // We multiply by Mathf.Abs(horizontal) to handle analog input (0 to 1)

        }
        else if (state.vertical == VerticalState.Grounded)
        {
            rb.AddForce(new Vector2(-rb.linearVelocity.x * rb.mass * breakingForce, 0));
        }
        if (state.vertical == VerticalState.Grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        if (state.vertical == VerticalState.Grounded)
        {
            groundedTimeCounter += Time.fixedDeltaTime;
        }
        else
        {
            groundedTimeCounter = 0f;
        }
        if (state.vertical == VerticalState.Grounded)
        {
            if (rb.linearVelocity.y <= 0.1f)
            {
                rb.AddForce(Vector2.down * stickToGroundForce, ForceMode2D.Force);
            }
        }
        xSpeed = rb.linearVelocity.x;
        ySpeed = rb.linearVelocity.y;
    }
    private void Update()
    {
        animator.SetBool("IsGrounded", state.vertical == VerticalState.Grounded);
        if(state.canMove)
        {
            if (horizontal > 0)
            {
                transform.localScale = baseScale;
            }
            else if (horizontal < 0)
            {
                transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
            }
        }
        animator.SetBool("IsGrounded", state.vertical == VerticalState.Grounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        // Press T to toggle slow motion
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Time.timeScale = (Time.timeScale == 1.0f) ? 0.2f : 1.0f;

            // Adjust fixedDeltaTime so physics remains smooth while slow
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

    }
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        if (horizontal != 0)
        {
            state.movement |= MovementState.Running;
        }
        else
        {
            state.movement &= ~MovementState.Running;
        }
    }
    public void Jump(InputAction.CallbackContext context)
    {

        bool groundedAndReady = state.vertical == VerticalState.Grounded && groundedTimeCounter >= minGroundedTime;
        if (context.performed && !state.IsMovementRestricted)
        {
            if (groundedAndReady || coyoteTimeCounter > 0)
            {
                coyoteTimeCounter = 0f;
                groundedTimeCounter = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);   
                rb.AddForce(groundNormal * jumpingPower * rb.mass, ForceMode2D.Impulse);
            }
        }
    }
    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            state.movement |= MovementState.Sprinting;
        }
        else if (context.canceled)
        {
            state.movement &= ~MovementState.Sprinting;
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && horizontal != 0 && !state.movement.HasFlag(MovementState.Dashing))
        {
            if (state.vertical == VerticalState.Airborne && currentDashCharges == 0) return;
            state.movement |= MovementState.Dashing;
            state.movementLockCounter++;
            if (state.vertical == VerticalState.Airborne)
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
    public void LightAttack(InputAction.CallbackContext context)
    {
        if (state.vertical == VerticalState.Grounded && state.canAttack)
        {
            state.primaryAction |= PrimaryAction.Attacking;
            animator.SetTrigger("PressLightAttack");
        }
    }
    public void HeavyAttack(InputAction.CallbackContext context)
    {
        if (state.vertical == VerticalState.Grounded && state.canAttack)
        {
            state.primaryAction |= PrimaryAction.Attacking;
            animator.SetTrigger("PressHeavyAttack");
        }
    }
    private IEnumerator RechargeHandler(System.Func<float> getCurrent, System.Action<float> setCurrent, float maxCharges, float rechargeTime)
    {
        while (getCurrent() < maxCharges)
        {
            yield return new WaitForSeconds(rechargeTime);
            float newCharge = getCurrent() + 1f;
            setCurrent(Mathf.Clamp(newCharge, 0f, maxCharges));
        }
    }
    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        state.movement &= ~MovementState.Dashing;
        state.movementLockCounter--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y);
    }

    public void CheckGround()
    {
        float castHeight = 0.05f;
        float castDistance = 0.01f;
        float castWidth = playerCollider.bounds.size.x * 0.9f;
        Vector2 castCenter = playerCollider.bounds.center;
        castCenter.y -= playerCollider.bounds.extents.y + (castHeight / 2);
        RaycastHit2D hit = Physics2D.BoxCast(castCenter, new Vector2(castWidth, castHeight), 0, Vector2.down, castDistance, groundLayer);
        if (hit)
        {
            state.vertical = VerticalState.Grounded;
            groundNormal = hit.normal;
        }
        else
        {
            state.vertical = VerticalState.Airborne;
            groundNormal = Vector2.up;
        }
    }
    public void HandleBoundaryBehaviour()
    {
        rb.linearVelocity *= 0;
        transform.position = spawnPoint.position;
    }
    public void ActivateHitbox() => SwordAttack.SetActive(true);
    public void DeactivateHitbox()
    {
        state.primaryAction &= ~PrimaryAction.Attacking;
        SwordAttack.SetActive(false);
    }
}

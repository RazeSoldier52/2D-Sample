using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D playerCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private Vector3 baseScale;
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
    [SerializeField] private float minNormalYThreshold = 0.7f;
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
    [Header("Animation State")]
    [SerializeField] private AnimatorStateInfo animatorStateInfo;
    private void Awake()
    {
        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
    }
    private void Start()
    {
        currentDashCharges = maxDashCharges;
        gameObject.transform.position = spawnPoint.position;
        baseScale = transform.localScale;

    }
    private void FixedUpdate()
    {
        CheckGround();
        currentSpeed= isSprinting ? baseSpeed+sprintModifier : baseSpeed;
        if (movementLockCounter==0)
        {
            if (horizontal != 0)
            {
                Vector2 movementDirection = Vector2.right;

                if (isGrounded && groundNormal.y >=minNormalYThreshold)
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
                float absoluteTargetSpeed = currentSpeed; // No 'horizontal' multiplier here

                // 4. Calculate current speed ALONG the movementDirection vector
                float currentSpeedAlongDirection = Vector2.Dot(rb.linearVelocity, movementDirection);

                // 5. Calculate the force needed to reach the ABSOLUTE speed
                float forceMagnitude = (absoluteTargetSpeed - currentSpeedAlongDirection) * rb.mass * acceleration;

                // 6. Apply the force along the calculated movementDirection vector
                rb.AddForce(movementDirection * forceMagnitude * Mathf.Abs(horizontal), ForceMode2D.Force);
                // We multiply by Mathf.Abs(horizontal) to handle analog input (0 to 1)
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
    private void Update()
    {
        animatorStateInfo= animator.GetCurrentAnimatorStateInfo(0);
        animator.SetBool("IsGrounded", isGrounded);
        if (horizontal > 0)
        {
            transform.localScale = baseScale;
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(-baseScale.x,baseScale.y,baseScale.z); 
        }
        animator.SetBool("IsGrounded", isGrounded);
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
    public void LightAttack(InputAction.CallbackContext context)
    {
        if(animatorStateInfo.IsName("Horizontal Movement")&& isGrounded && context.performed)
        animator.SetTrigger("PressLightAttack");
    }
    public void HeavyAttack(InputAction.CallbackContext context)
    {
        if (animatorStateInfo.IsName("Horizontal Movement") && isGrounded && context.performed)
            animator.SetTrigger("PressHeavyAttack");
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
    
}

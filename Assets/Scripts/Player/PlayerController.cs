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
    [SerializeField] private float jumpIgnoreDuration = 0.05f;
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
            rb.AddForce(Vector2.down * stickToGroundForce, ForceMode2D.Force);
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
            Physics2D.IgnoreLayerCollision(gameObject.layer, (int)Mathf.Log(groundLayer.value, 2), true);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(groundNormal * jumpingPower * rb.mass, ForceMode2D.Impulse);
            StartCoroutine(EnableGroundCollisionAfterJump(jumpIgnoreDuration));

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
    private IEnumerator EnableGroundCollisionAfterJump(float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreLayerCollision(gameObject.layer, (int)Mathf.Log(groundLayer.value, 2), false);
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

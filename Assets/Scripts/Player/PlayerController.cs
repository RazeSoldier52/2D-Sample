using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D playerCollider;
    [Header("Player Settings")]
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float currentSpeed=10;
    [SerializeField] float jumpingPower;
    [SerializeField] float sprintModifier;
    [SerializeField] bool isSprinting = false;
    [SerializeField] float acceleration = 40f;
    [SerializeField] float breakingForce = 15f;
    [SerializeField] float stickToGroundForce = 15f;
    [Header("Dashing")]
    [SerializeField] float dashPower = 30;
    [SerializeField] float dashDuration = 0.2f; 
    private bool isDashing = false; 
    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform playerTransform;
    private float horizontal;
    private float grounded;
    
    private void FixedUpdate()
    {
        currentSpeed= isSprinting ? baseSpeed+sprintModifier : baseSpeed;
        if (!isDashing)
        {
            if (horizontal != 0)
            {
                rb.AddForce(new Vector2((((horizontal * currentSpeed) - rb.linearVelocity.x) * rb.mass * acceleration), 0));
            }
            else if (IsGrounded())
            {
                rb.AddForce(new Vector2(-rb.linearVelocity.x * rb.mass * breakingForce, 0));
            }
            if (IsGrounded())
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

        if (context.performed && IsGrounded())
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpingPower * rb.mass, ForceMode2D.Impulse);

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
        // Check if dash performed, movement input exists, and not already dashing
        if (context.performed && horizontal != 0 && !isDashing)
        {
            isDashing = true;

            Vector2 dashDirection = new Vector2(horizontal, 0).normalized;
            rb.AddForce(dashDirection * rb.mass * dashPower, ForceMode2D.Impulse);

            StartCoroutine(StopDash());
        }
    }
    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y);
    }
    public bool IsGrounded()
    {
        if (playerCollider == null) return false;

        // 1. Define the dimensions for the thin check box0
        float checkHeight = 0.2f; // Tolerance height
                                  // Use most of the player's width for the check (e.g., 90%)
        float checkWidth = playerCollider.bounds.size.x * 0.9f;
        Vector2 checkSize = new Vector2(checkWidth, checkHeight);

        // 2. Calculate the center point for the check
        // Start at the bottom center of the player's collider bounds (in World Space).
        Vector2 checkCenter = playerCollider.bounds.center;

        // Offset the Y position downward by half the collider height AND half the check box height
        // This places the center of the check box just outside the bottom of the player's collider.
        float colliderHalfHeight = playerCollider.bounds.extents.y;
        checkCenter.y -= (colliderHalfHeight + (checkHeight / 2));

        // 3. Get the rotation angle from the player's transform
        float rotationAngle = playerTransform.rotation.eulerAngles.z;

        // 4. Perform the OverlapBox check with rotation
        // We use OverlapBox here instead of OverlapCapsule, as it's often more intuitive 
        // when calculating from rectangular bounds.
        return Physics2D.OverlapBox(
            checkCenter,      // Calculated position at the feet
            checkSize,        // Thin, wide check area
            0,    // **Applies Z-axis rotation**
            groundLayer       // Filter
        );
    }


    private void OnDrawGizmosSelected()
    {
        // Ensure we have the necessary references before drawing
        if (playerCollider == null || playerTransform == null) return;

        // --- Recalculate all the same values as IsGrounded() ---

        // 1. Check Dimensions
        float checkHeight = 0.2f;
        float checkWidth = playerCollider.bounds.size.x * 0.9f;
        Vector3 checkSize = new Vector3(checkWidth, checkHeight, 0f);

        // 2. Calculated Center Position
        Vector2 checkCenter = playerCollider.bounds.center;
        float colliderHalfHeight = playerCollider.bounds.extents.y;
        checkCenter.y -= (colliderHalfHeight + (checkHeight / 2));

        // 3. Rotation Angle
        //float rotationAngle = playerTransform.rotation.eulerAngles.z;

        // --- Gizmo Drawing ---

        // A. Set Color
        Gizmos.color = Color.cyan;

        // B. Apply Rotation and Position to the Gizmos drawing matrix
        // This is essential! It tells Unity to draw the subsequent shapes relative to this new rotation/position.
        Gizmos.matrix = Matrix4x4.TRS(
            checkCenter, // Center of the check
            Quaternion.Euler(0, 0, 0), // Apply Z-axis rotation
            Vector3.one
        );

        // C. Draw the Wire Cube
        // We draw the cube centered at Vector3.zero because the matrix (step B) already handles its world position and rotation.
        Gizmos.DrawWireCube(Vector3.zero, checkSize);

        // D. Reset the Gizmos matrix to avoid affecting other editor drawings
        Gizmos.matrix = Matrix4x4.identity;
    }

}

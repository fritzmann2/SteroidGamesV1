using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Generel Settings")]
    private LayerMask groundLayer;
    private LayerMask wall;
    private GameControls controls;
    public Rigidbody2D rb;

    [Header("Movement Settings")]
    [SerializeField] private const float movementSpeed = 8f;
    [SerializeField] private const float wallSlideSpeed = 2f;

    [Header("Accelerating Settings")]
    [SerializeField] private const float groundAcceleration = 15f;
    [SerializeField] private const float groundDeceleration = 20f;
    [SerializeField] private const float airAcceleration = 10f;
    [SerializeField] private const float airDecelerationSpeed = 10f;
    [SerializeField] private const float BaseGravityScale = 3f;

    [Header("Jump Settings")]
    [SerializeField] private const float jumpForce = 10f;
    [SerializeField] private const float wallJumpMultiplier = 0.5f;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Dash Settings")]
    [SerializeField] private const float dashforce = 12f;
    [SerializeField] private const float dashCooldown = 0.4f;
    [SerializeField] private const float dashDuration = 0.2f;
    private float dashtimer = dashDuration;

    [Header("Collider Settings")]

        [Header("Ground Check Settings")]
        private const float groundCheckPosy = -0.5f;
        private const float groundCheckLengthx = 0.9f;
    
        [Header("Wall Check Settings")]
        private const float wallCheckDistanceX = 0.5f;
        private const float wallCheckHeighty = 0.7f;

        [Header("Boolean States")]
        private bool isGrounded;
        [SerializeField] private bool isWallJumpPossible;
        private bool isJumping;
        private bool canDash;
        private bool canDashJump;
        private bool isDashing;
        private bool didWallJump;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        while (Camera.main == null || Camera.main.GetComponent<CameraFollow>() == null)
        {
            Debug.Log("Warte auf Kamera...");
        }
    
        Camera.main.GetComponent<CameraFollow>().target = transform;
        groundLayer = LayerMask.GetMask("Ground");
        wall = LayerMask.GetMask("Wall", "Ground");
        rb = GetComponent<Rigidbody2D>();
        controls = new GameControls();
        controls.Enable();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        controls.Disable();
    }

    void FixedUpdate()
    {   
        if (!IsOwner) return;
        checkColliders();
        move();
        checkJump();
        checkDash();
        checkSlide();
    }

    private void move()
    {
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        float moveInput = inputVector.x;
        float targetSpeed = moveInput * movementSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        if (moveInput != 0)
        {
            if (moveInput < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        float accelRate;
        if (isGrounded)
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? groundAcceleration : groundDeceleration;
        }
        else
        {
            accelRate = airAcceleration;
        }

        float movement = speedDif * accelRate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        if (isWallJumpPossible)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));

        }
        else if (!controls.Gameplay.jump.IsPressed() || rb.linearVelocity.y <= 0f)
        {
            rb.gravityScale = BaseGravityScale;
        }
    }

    private void checkJump()
    {
        if (controls.Gameplay.jump.IsPressed())
        {
            if (NormalJump())
            {
                isJumping = true;
            }
            else if (WallJump())
            {
                isJumping = true;
            }
        }
        else
        {
            didWallJump = false;
        }

        if (!isGrounded)
        {
            coyoteTimeCounter += Time.fixedDeltaTime;
        }
        else
        {
            isJumping = false;
            canDashJump = false;
            coyoteTimeCounter = 0f;
        }
    }

    private bool NormalJump()
    {
        if (coyoteTimeCounter < coyoteTime && !isWallJumpPossible && !isJumping || canDashJump)
        {
            Debug.Log("Normal Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if(canDashJump) canDashJump = false;
            return true;
        }
        if (isJumping && rb.linearVelocity.y > 0f && controls.Gameplay.jump.IsPressed())
        {
            rb.gravityScale = BaseGravityScale * 0.5f;
        }
        
        return false;
    }

    private bool WallJump()
    {
        if (isWallJumpPossible && !didWallJump)
        {
            rb.linearVelocity = new Vector2(-transform.localScale.x * jumpForce * 3f, jumpForce * wallJumpMultiplier);
            Flip();
            didWallJump = true;
            return true;
        }
        return false;
    }

    private void checkDash()
    {
        if (canDash && !isDashing)
        {
            if (controls.Gameplay.dash.IsPressed())
            {
                PerformDash();
            }
        }
        if (isDashing && dashtimer > 0f)
        {
            dashtimer -= Time.fixedDeltaTime;
        }
        else
        {
            isDashing = false;
            dashtimer = dashDuration;}
    }

    private void PerformDash()
    {
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        float dashside = 0;
        float dashup = 0;
        
        if (inputVector.y > 0)
        {
            dashup = 1;
        }
        else if (inputVector.y < 0)
        {
            dashup = -1;
        }
    
        if (dashup == 0)
        {
            dashside = transform.localScale.x;
        }
        rb.linearVelocity = new Vector2(dashforce * 4f * dashside,dashforce * dashup);
        canDash = false;
        canDashJump = true;
        isDashing = true;
    }

    private void checkSlide()
    {
        //if ()
    }


    private void checkColliders()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, groundCheckPosy);
        
        isGrounded = Physics2D.BoxCast(origin, new Vector2(groundCheckLengthx, 0.1f), 0, Vector2.down, 0.05f, groundLayer);
        if (isGrounded)
        {
            coyoteTimeCounter = 0f;
            isJumping = false;
            canDash = true;
        }
        else
        {
            coyoteTimeCounter += Time.fixedDeltaTime;
        }
        float facingDirection = transform.localScale.x;
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(wallCheckDistanceX * facingDirection, 0);
        isWallJumpPossible = Physics2D.OverlapBox(wallOrigin, new Vector2(0.1f, wallCheckHeighty), 0, wall);
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
{
    // GroundCheck (Red)
    Gizmos.color = Color.red;
    Vector2 boxCenter = (Vector2)transform.position + new Vector2(0, groundCheckPosy);
    Gizmos.DrawWireCube(boxCenter, new Vector2(groundCheckLengthx, 0.1f));

    // WallCheck (Green)
    Gizmos.color = Color.green;

    float facingDirection = transform.localScale.x; 
    
    Vector2 wallCheckPos = (Vector2)transform.position + new Vector2(wallCheckDistanceX * facingDirection, 0);
    
    Gizmos.DrawWireCube(wallCheckPos, new Vector2(0.1f, wallCheckHeighty));
}
}

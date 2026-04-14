using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Generel Settings")]
    private LayerMask groundLayer;
    private LayerMask wall;
    private GameControls controls;
    public Rigidbody2D rb;
    private PlayerStatsUI statsUI;

    [Header("Movement Settings")]
    [SerializeField] private const float movementSpeed = 8f;
    [SerializeField] private const float wallSlideSpeed = 2f;

    [Header("Custom Gravity")]
    [SerializeField] private float customGravity; 
    [SerializeField] private float maxFallSpeed;


    public NetworkVariable<float> facingDirectionX = new NetworkVariable<float>(
        1f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );
    public void Reset()
    {
        customGravity = 35f; 
        maxFallSpeed = 15f;
    }

    [Header("Accelerating Settings")]
    [SerializeField] private const float groundAcceleration = 15f;
    [SerializeField] private const float groundDeceleration = 20f;
    [SerializeField] private const float airAcceleration = 10f;
    [SerializeField] private const float airDecelerationSpeed = 10f;
    [SerializeField] private const float BaseGravityScale = 3f;

    [Header("Jump Settings")]
    [SerializeField] private const float jumpForce = 11f;
    [SerializeField] private const float wallJumpMultiplier = 0.5f;
    private int possiblejumps = 1;
    private int jumpcounter = 0;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Dash Settings")]
    [SerializeField] private const float dashforce = 10f;
    [SerializeField] private const float dashCooldown = 0.4f;
    [SerializeField] private const float dashDuration = 0.2f;
    public float dashtimer = dashDuration;
    public bool isDashUnlocked = false;

    [Header("Collider Settings")]

        [Header("Ground Check Settings")]
        private const float groundCheckPosy = -0.5f;
        private const float groundCheckLengthx = 0.9f;
    
        [Header("Wall Check Settings")]
        private const float wallCheckDistanceX = 0.5f;
        private const float wallCheckHeighty = 0.7f;

        [Header("Boolean States")]
        [SerializeField] private bool isGrounded;
        [SerializeField] private bool isWallJumpPossible;
        private bool isJumping;
        private bool pressedjump = false;
        private bool canDash;
        public bool isDashing;
        private bool didWallJump;

    public override void OnNetworkSpawn()
    {
        facingDirectionX.OnValueChanged += OnFacingDirectionChanged;
        ApplyScale(facingDirectionX.Value);
        if (!IsOwner) return;
        while (Camera.main == null || Camera.main.GetComponent<CameraFollow>() == null)
        {
            Debug.Log("Warte auf Kamera...");
        }
        statsUI = FindAnyObjectByType<PlayerStatsUI>();
        Camera.main.GetComponent<CameraFollow>().target = transform;
        groundLayer = LayerMask.GetMask("Ground");
        wall = LayerMask.GetMask("Wall", "Ground");
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        controls = InputManager.Instance.Controls;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
    }

    void FixedUpdate()
    {   
        if (!IsOwner) return;

        if (statsUI != null && !statsUI.gameObject.activeInHierarchy)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return; 
        }

        checkColliders();
        ApplyCustomGravity();
        move();
        checkJump();
        checkDash();
    }

    private void ApplyCustomGravity()
    {
        if (isDashing) return;

        if (!isGrounded)
        {
            float currentGravity = customGravity;

            if (isJumping && rb.linearVelocity.y > 0f && controls.Gameplay.jump.IsPressed())
            {

                currentGravity *= 0.5f;
            }
            else if (isJumping && rb.linearVelocity.y > 0f && !controls.Gameplay.jump.IsPressed())
            {
                currentGravity *= 3f;
            }
            float newVelocityY = rb.linearVelocity.y - (currentGravity * Time.fixedDeltaTime);
            newVelocityY = Mathf.Max(newVelocityY, -maxFallSpeed);
            if (isWallJumpPossible && newVelocityY < 0 && controls.Gameplay.WallSlide.IsPressed())
            {
                newVelocityY = Mathf.Max(newVelocityY, -wallSlideSpeed);
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVelocityY);
        }
    }

    private void move()
    {
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        float rawInputX = inputVector.x;
        float moveInput = 0f;

        if (Mathf.Abs(rawInputX) > 0.15f)
        {
            moveInput = rawInputX > 0 ? 1f : -1f;
            if (facingDirectionX.Value != moveInput)
            {
                facingDirectionX.Value = moveInput;
            }
        }

        float targetSpeed = moveInput * movementSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        
        float accelRate = isGrounded ? 
            (Mathf.Abs(targetSpeed) > 0.01f ? groundAcceleration : groundDeceleration) : 
            airAcceleration;

        float movement = speedDif * accelRate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
    private void checkJump()
    {
        if (controls.Gameplay.jump.IsPressed())
        {
            if (pressedjump == false)
            {
                if (NormalJump())
                {
                    didWallJump = false;
                    isJumping = true;
                }
                else if (WallJump())
                {
                    isJumping = true;
                }
                pressedjump = true;
            }
        }
        else
        {
            pressedjump = false;
        }
    }

    private bool NormalJump()
    {
        if (coyoteTimeCounter <= coyoteTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            jumpcounter = 1; 
            coyoteTimeCounter = 100f; 
            
            return true;
        }
        else if (jumpcounter < possiblejumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpcounter = (jumpcounter == 0) ? 2 : jumpcounter + 1;
            
            return true;
        }

        return false; 
    }

    private bool WallJump()
    {
        if (isWallJumpPossible && !didWallJump && !isGrounded && coyoteTimeCounter > coyoteTime)
        {
            rb.linearVelocity = new Vector2(-transform.localScale.x * jumpForce * 2f, jumpForce * wallJumpMultiplier);
            Flip();
            didWallJump = true;
            return true;
        }
        return false;
    }

    private void checkDash()
    {
        if (isDashUnlocked && canDash && !isDashing)
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
        isDashing = true;
        
        if (inputVector.magnitude > 0.3f)
        {
            if (Mathf.Abs(inputVector.y) > Mathf.Abs(inputVector.x))
            {
                dashup = inputVector.y > 0 ? 1 : -1;
            }
            else
            {
                dashside = inputVector.x > 0 ? 1 : -1;
            }
        }
        else
        {
            dashside = transform.localScale.x;
        }

        rb.linearVelocity = new Vector2(dashforce * 4f * dashside, dashforce * dashup);
        canDash = false;
    }

    private void checkColliders()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, groundCheckPosy);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(groundCheckLengthx, 0.1f), 0, Vector2.down, 0.05f, groundLayer);
        isGrounded = hit.collider != null;
        if (isGrounded)
        {
            coyoteTimeCounter = 0f;
            isJumping = false;
            jumpcounter = 0;
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
        facingDirectionX.Value *= -1f;
    }

    public void pauseGravity()
    {
        rb.linearVelocity = new Vector2(0f, 0f);
    }

    private void OnFacingDirectionChanged(float previousValue, float newValue)
    {
        ApplyScale(newValue);
    }

    private void ApplyScale(float scaleX)
    {
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }
    public void UnlockDash()
    {
        isDashUnlocked = true;
    }

    public void SetMaxJumps(int amount)
    {
        possiblejumps = amount;
    }

    public void ResetAbilities()
    {
        isDashUnlocked = false;
        possiblejumps = 1;
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

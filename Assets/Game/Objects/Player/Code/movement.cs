using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class movement : NetworkBehaviour
{
    [Header("Boden-Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPos; 
    public LayerMask slideLayer;
    private Vector2 groundCheckSize = new Vector2(0.95f, 0.1f);

    [Header("Wall Slide")]
    public float wallSlideSpeed = 1.5f;
    public Transform wallCheckPos;
    public LayerMask wallLayer;

    [Header("Jump")]
    public float jumpForce = 1f;
    public int jumpcount = 1;
    private float jumptimer = 0f;
    private float offgroundpuffer = 0.1f;

    [Header("Variable Jump")]
    private float jumpHoldForce; 
    private float maxJumpTime = 0.25f; 
    private float jumpTimeCounter;    
    private bool isJumping;


    [Header("Acceleration")]
    public float groundAcceleration = 15f;
    public float basedeceleration = 15f;
    private float groundDeceleration;
    public float airAcceleration = 4f;
    

    [Header("Dash")]
    public float dashtimer = 0f;
    public float dashcooldown = 0.4f;
    public float dashforce = 2f;
    public int dashjumpcount = 0;

    private Rigidbody2D rb;
    public float basemoveSpeed = 8;
    private float moveSpeed = 8;
    public float GravityScale = 3;
    

    public bool isGrounded = true;
    public bool isTouchingWall = false;
    public bool isWallSliding = false;
    public bool isSliding = false;
    private bool LooksRight = true;
    private bool didjump = false;
    private bool wasonground = false;
    private GameControls controls;
    private float moveInput;
    private float inputy;

    [ContextMenu("Update Movement Values")] 
    void UpdateValues()
    {
        // Wall Slide
        wallSlideSpeed = 1.5f;

        // Jump
        jumpForce = 1f;
        jumpcount = 1;
        maxJumpTime = 0.25f;

        // Acceleration
        groundAcceleration = 15f;
        basedeceleration = 15f;
        airAcceleration = 4f;

        // Dash
        dashcooldown = 0.4f;
        dashforce = 2f; 

        // Movement
        basemoveSpeed = 8f;
        GravityScale = 3f;

        Debug.Log("Werte im Inspector wurden aktualisiert!");
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => IsSpawned);

        if (!IsOwner) yield break;
//        Debug.Log("Ich bin der Owner dieses Spielers, verbinde die Kamera...");
        while (Camera.main == null || Camera.main.GetComponent<CameraFollow>() == null)
        {
            Debug.Log("Warte auf Kamera...");
            yield return null; // Wartet bis zum nächsten Frame
        }

        Camera.main.GetComponent<CameraFollow>().target = transform;
        
//        Debug.Log("Kamera erfolgreich im Start verbunden!");
    }
    
    public override void OnNetworkSpawn()
    {
        // Debug.Log($"Spawn Start Owner: {IsOwner}");
        base.OnNetworkSpawn();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.gravityScale = GravityScale;
        controls = new GameControls();
        groundDeceleration = basedeceleration;
        jumpHoldForce = jumpForce * 30f;
       // Debug.LogError("PLAYER AWAKE: Ich versuche zu existieren!");
    }
    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }
    
    
    void Update()
    {
        if (!IsOwner) return;
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        moveInput = inputVector.x;
        inputy = inputVector.y;
        if (controls.Gameplay.jump.IsPressed() && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime; 
            }
            else
            {
                isJumping = false; 
            }
        }
        if (!controls.Gameplay.jump.IsPressed())
        {
            isJumping = false;
        }
        
    }
    
    void FixedUpdate()
    {
        if (!IsOwner) return;
        collidetest();
        Run();
        Jump();
        Dash();
        Slide();
        wallSlide();
        viewdir();
    }


    //Änderung Blickrichtung
    private void viewdir()
    {
        if (moveInput == -1 && LooksRight)
        {
            Flip();
        }
        else if (moveInput == 1 && !LooksRight)
        {
            Flip();
        }
    }
    //Bewegung
    private void Run()
    {
        float targetSpeed = moveInput * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
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
    }

    //sprung
    private void Jump()
    {
        if (didjump && !controls.Gameplay.jump.IsPressed())
        {
            didjump = false;
            return;
        }
        //jump abfrage
        if (controls.Gameplay.jump.IsPressed() && jumptimer < 0f)
        {
            jumptimer = 0.1f;
            dojump();
        }
        else if (jumptimer >= 0f)
        {
            jumptimer -= Time.fixedDeltaTime;
        }
        else if (isGrounded)
        {
            jumpcount = 1;
        }
        if (isJumping)
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
        }
    }
    // Abfrage welchen sprung ausführen
    private void dojump()
    {
        //normales springen
        if (jumpcount >= 1 && (offgroundpuffer >= 0f || !isWallSliding) || isSliding && !controls.Gameplay.slide.IsPressed())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * moveSpeed);
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            jumpcount--;
            didjump = true;
        }
        //Slide jump
        else if (isSliding && jumpcount >= 1 && controls.Gameplay.slide.IsPressed())
            {
                rb.linearVelocity = Vector2.zero;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * moveSpeed * 0.6f);
                rb.linearVelocity = new Vector2(jumpForce * moveSpeed * 1.5f, rb.linearVelocity.y);
            jumpcount--;
            }
        //Wall jump
        else if (!isGrounded && isWallSliding && didjump == false)
        {
            wallJump();
        }
        //Dash jump
        else if (dashjumpcount >= 1 && !isWallSliding && didjump == false)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * moveSpeed);
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            dashjumpcount--;
        }
    }
    //Dash
    public void Dash()
    {
        if (dashtimer < dashcooldown - 0.4f && dashtimer > 0f)
        {
            groundDeceleration = basedeceleration;
        }
        
        //Dash abfrage
        if (controls.Gameplay.dash.IsPressed() && wasonground == true)
        {
            wasonground = false;
            isJumping = false;
            if (inputy != 0)
            { 
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, inputy * dashforce * moveSpeed * 0.7f);
            }
            else
            {
                float dir = LooksRight ? 1 : -1;
                rb.linearVelocity = new Vector2(dir * dashforce * moveSpeed, rb.linearVelocity.y);
            }
            groundDeceleration = 2f;
            dashtimer = dashcooldown;
            dashjumpcount = 1;
        }
        else if (dashtimer > 0f)
        {
            dashtimer -= Time.fixedDeltaTime;
        }
    }
    // Slide
    public void Slide()
    {
        if (isSliding && controls.Gameplay.slide.IsPressed())
        {
            moveSpeed = (int) (basemoveSpeed * 1.3f);
        }
        else
        {
            moveSpeed = basemoveSpeed;
        }
    }


    //Collision test
    private void collidetest()
    {
        isGrounded = (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer) || Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, slideLayer)) ? true : false;
        isTouchingWall = Physics2D.OverlapBox(wallCheckPos.position, new Vector2(1.1f, 0.9f), 0f, wallLayer);
        isSliding = (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, slideLayer) && !Physics2D.OverlapBox(wallCheckPos.position, new Vector2(0.2f, 1f), 0f, slideLayer)) ? true : false;
        
        if (!isGrounded)
        {
            offgroundpuffer -= Time.fixedDeltaTime;
        }
        else
        {
            if (wasonground == false)
            {
                wasonground = true;
                isJumping = false;
            }
            dashjumpcount = 0;
            offgroundpuffer = 0.1f;
        }
    }

    // Wall Slide
    private void wallSlide()
    {
        if (isTouchingWall && !isGrounded)
        {   
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    // Wall Jump ausführen
    private void wallJump()
    {
        didjump = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpForce * 6f, ForceMode2D.Impulse);
        rb.AddForce(Vector2.right * moveSpeed * 2.2f * (LooksRight ? -1 : 1), ForceMode2D.Impulse);
            jumpcount--;
        Flip();
    }
    // Zeichnen von hitboxen
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheckPos != null) Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        
        Gizmos.color = Color.blue;
        if (wallCheckPos != null) Gizmos.DrawWireCube(wallCheckPos.position, new Vector2(1.1f, 0.9f));
    }
    // Spieler umdrehen
    private void Flip()
    {
        LooksRight = !LooksRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}


public static class GameData
{
    public static string PlayerName = "Unbekannt";
}
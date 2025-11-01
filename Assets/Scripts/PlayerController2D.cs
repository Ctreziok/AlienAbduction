using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    [Header("Run")]
    public float runSpeed = 6f;     //baseline speed
    public float lateralBiasSpeed = 4.5f;       //left/right bias

    [Header("Jump")]
    public float jumpImpulse = 15f;
    public float coyoteTime = 0.12f;        //grace time
    public float jumpBuffer = 0.12f;        //press slightly before landing
    public float lowJumpGravity = 4f;       //small jump
    public float fallGravity = 5.5f;

    [Header("Ground Check")]
    public Transform groundCheck;             
    public Vector2 groundCheckSize = new(0.8f, 0.12f);
    public LayerMask groundMask;
    
    [Header("Bounds")]
    public float minX = -20f;
    public float maxX =  60f;
    
    [Header("SFX")]
    public AudioClip jumpSfx;
    public AudioClip landSfx;


    Rigidbody2D rb;
    Vector2 moveInput;
    bool jumpHeld;
    float coyoteTimer;
    float bufferTimer;
    float defaultGravity;
    bool wasGrounded;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;
        rb.gravityScale = 3.2f;     //base
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);
        if (!wasGrounded && grounded && landSfx) AudioManager.I?.PlaySFX(landSfx, 0.8f);
        wasGrounded = grounded;
        
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
        bufferTimer = Mathf.Max(0f, bufferTimer - Time.deltaTime);

        //jump start
        if (coyoteTimer > 0f && bufferTimer > 0f)
        {
            //reseting vertical velocity in order to have a consistent jump height
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
            coyoteTimer = 0f;
            bufferTimer = 0f;
        }

        //better gravity
        if (rb.velocity.y < -0.01f) rb.gravityScale = fallGravity;
        else if (rb.velocity.y > 0.01f && !jumpHeld) rb.gravityScale = lowJumpGravity;
        else rb.gravityScale = defaultGravity;
    }

    void FixedUpdate()
    {
        //lateral bias
        float targetX = moveInput.x * lateralBiasSpeed;
        float vx = Mathf.MoveTowards(rb.velocity.x, targetX, 40f * Time.fixedDeltaTime);
        rb.velocity = new Vector2(vx, rb.velocity.y);
        
        //Clamping horizontal position
        float clampedX = Mathf.Clamp(rb.position.x, minX, maxX);
        rb.position = new Vector2(clampedX, rb.position.y);


        //face direction
        if (Mathf.Abs(moveInput.x) > 0.02f)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
    }

    //Input System callbacks
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) bufferTimer = jumpBuffer; //pressed this frame
        jumpHeld = ctx.performed || ctx.started;   //held while performed
        if (ctx.canceled) jumpHeld = false;        //released
        if (ctx.started && jumpSfx) AudioManager.I?.PlaySFX(jumpSfx, 0.9f);
    }

    
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}

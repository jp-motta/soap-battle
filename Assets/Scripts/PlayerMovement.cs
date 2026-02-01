using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
  [Header("Movement")]
  public float walkSpeed = 4f;
  public float runSpeed = 8f;
  public float acceleration = 60f;
  public float deceleration = 80f;

  [Header("Jump Control")]
  public float jumpForce = 12f;
  public float jumpCutMultiplier = 0.5f;
  private bool jumpRequested;

  [Header("Air Jump")]
  public int maxAirJumps = 1;
  public float airJumpForceMultiplier = 1f; // pode ser < 1 para pulo aéreo menor
  private int airJumpsRemaining;

  [Header("Gravity")]
  public float normalGravityScale = 6f;
  public float fastFallGravityScale = 9f;

  private Rigidbody2D rb;
  private float moveInput;
  private bool isGrounded;

  [Header("Ground Check")]
  public LayerMask groundLayer;
  private Collider2D col;

  [SerializeField] private Transform visual;

  public bool facingRight { get; private set; } = true;

  [Header("Combat State")]
  public bool isShooting { get; set; } = false;
  private float shootingMultiplier = 1f;

  private PlayerHealth playerHealth;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    col = GetComponent<Collider2D>();
    playerHealth = GetComponent<PlayerHealth>();
  }

  private float verticalInput;

  public void OnMove(InputAction.CallbackContext context)
  {
    Vector2 input = context.ReadValue<Vector2>();
    moveInput = input.x;
    verticalInput = input.y;

    if (Mathf.Abs(moveInput) > 0.01f)
      playerHealth.Damage(0.01f);
  }

  public void OnJump(InputAction.CallbackContext context)
  {
    if (context.started)
    {
      jumpRequested = true;
    }

    if (context.canceled)
    {
      CutJump();
    }
  }

  void Update()
  {
    CheckGrounded();

    if (isGrounded)
    {
      airJumpsRemaining = maxAirJumps;
    }

    if (moveInput > 0.01f && !facingRight)
      SetFacing(true);
    else if (moveInput < -0.01f && facingRight)
      SetFacing(false);
  }

  void FixedUpdate()
  {
    ApplyHorizontalMovement();

    // 🔹 PULO (chão ou ar)
    if (jumpRequested)
    {
      if (isGrounded)
      {
        Jump();
      }
      else if (airJumpsRemaining > 0)
      {
        AirJump();
        airJumpsRemaining--;
      }

      jumpRequested = false;
    }

    // 🔹 GRAVIDADE / FAST FALL
    if (isGrounded)
    {
      rb.gravityScale = normalGravityScale;
    }
    else
    {
      if (verticalInput < -0.5f && rb.linearVelocity.y <= 0f)
      {
        rb.gravityScale = fastFallGravityScale;
      }
      else
      {
        rb.gravityScale = normalGravityScale;
      }
    }
  }

  void Jump()
  {
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
  }

  void CutJump()
  {
    if (rb.linearVelocity.y > 0)
    {
      rb.linearVelocity = new Vector2(
        rb.linearVelocity.x,
        rb.linearVelocity.y * jumpCutMultiplier
      );
    }
  }

  void AirJump()
  {
    // Cancela queda / subida atual
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

    rb.AddForce(
      Vector2.up * jumpForce * airJumpForceMultiplier,
      ForceMode2D.Impulse
    );
  }

  void ApplyHorizontalMovement()
  {
    if (isShooting)
    {
      shootingMultiplier = 0.5f;
    }
    else
    {
      shootingMultiplier = 1f;
    }

    float targetSpeed = 0f;

    if (Mathf.Abs(moveInput) > 0.01f)
    {
      bool walking = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
      float baseSpeed = walking ? walkSpeed : runSpeed;

      targetSpeed = moveInput * baseSpeed * shootingMultiplier;
    }

    float speedDiff = targetSpeed - rb.linearVelocity.x;

    float accelRate = Mathf.Abs(targetSpeed) > 0.01f
        ? acceleration
        : deceleration;

    float movement = speedDiff * accelRate * Time.fixedDeltaTime;

    rb.linearVelocity = new Vector2(
        rb.linearVelocity.x + movement,
        rb.linearVelocity.y
    );
  }

  void CheckGrounded()
  {
    Vector2 bottomCenter = new Vector2(
      col.bounds.center.x,
      col.bounds.min.y
    );

    float checkRadius = 0.1f;

    isGrounded = Physics2D.OverlapCircle(
      bottomCenter,
      checkRadius,
      groundLayer
    );
  }

  void SetFacing(bool faceRight)
  {
    facingRight = faceRight;

    Vector3 vScale = visual.localScale;
    vScale.x = Mathf.Abs(vScale.x) * (facingRight ? 1 : -1);
    visual.localScale = vScale;
  }
}

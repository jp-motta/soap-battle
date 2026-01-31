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


  [Header("Wear Settings")]
  public float wearSpeed = 0.15f;
  public float minScale = 0.5f;

  private float initialScale;

  [Header("Growth Settings")]
  public float maxScale = 1.5f;
  public float bubbleGrowAmount = 0.1f;

  [SerializeField] private Transform visual;

  [Header("Knockback")]
  public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 2f);

  public bool facingRight { get; private set; } = true;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    col = GetComponent<Collider2D>();

    initialScale = transform.localScale.x;
  }

  private float verticalInput;

  public void OnMove(InputAction.CallbackContext context)
  {
    Vector2 input = context.ReadValue<Vector2>();
    moveInput = input.x;
    verticalInput = input.y;

    if (Mathf.Abs(moveInput) > 0.01f)
      ApplyWear();
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
    float targetSpeed = 0f;

    if (Mathf.Abs(moveInput) > 0.01f)
    {
      bool walking = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
      float baseSpeed = walking ? walkSpeed : runSpeed;

      targetSpeed = moveInput * baseSpeed;
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
  void ApplyWear()
  {
    float currentScale = transform.localScale.x;
    float newScale = Mathf.Max(currentScale - wearSpeed * Time.deltaTime, minScale);

    transform.localScale = new Vector3(newScale, newScale, 1f);

    float t = Mathf.InverseLerp(initialScale, minScale, newScale);
  }

  void SetFacing(bool faceRight)
  {
    facingRight = faceRight;

    Vector3 vScale = visual.localScale;
    vScale.x = Mathf.Abs(vScale.x) * (facingRight ? 1 : -1);
    visual.localScale = vScale;
  }

  public void Grow()
  {
    float currentScale = transform.localScale.x;
    float newScale = Mathf.Min(currentScale + bubbleGrowAmount, maxScale);

    transform.localScale = new Vector3(newScale, newScale, 1f);

    float t = Mathf.InverseLerp(initialScale, maxScale, newScale);
  }

  public void Shrink(float amount)
  {
    float currentScale = transform.localScale.x;
    float newScale = Mathf.Max(currentScale - amount, minScale);

    transform.localScale = new Vector3(newScale, newScale, 1f);

    float t = Mathf.InverseLerp(initialScale, minScale, newScale);
  }

  public void ApplyKnockback(Vector2 direction, float baseForce)
  {
    Rigidbody2D rb = GetComponent<Rigidbody2D>();

    // Quanto menor o jogador, maior o knockback
    // Exemplo: scale varia de 0.5 (pequeno) até 1.5 (grande)
    float sizeFactor = Mathf.InverseLerp(
      maxScale,   // sabonete grande
      minScale,   // sabonete pequeno
      transform.localScale.x
    );

    // Curva deixa a diferença mais dramática
    float knockbackMultiplier = knockbackCurve.Evaluate(sizeFactor);

    rb.linearVelocity = Vector2.zero;
    rb.AddForce(direction.normalized * baseForce * knockbackMultiplier, ForceMode2D.Impulse);
  }
}

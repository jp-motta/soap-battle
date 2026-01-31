using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
  [Header("Movement")]
  public float walkSpeed = 4f;
  public float runSpeed = 8f;
  public float acceleration = 60f;
  public float deceleration = 80f;
  public float jumpForce = 12f;

  [Header("Gravity")]
  public float normalGravityScale = 6f;
  public float fastFallGravityScale = 9f;

  private Rigidbody2D rb;

  private float moveInput;
  private bool jumpRequested;
  private bool isGrounded;

  [Header("Ground Check")]
  public Transform groundCheck;
  public float groundCheckRadius = 0.2f;
  public LayerMask groundLayer;

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

  [Header("Surface Effects")]
  private Tilemap groundTilemap;
  public TileBase[] slowTiles;
  public float slowMultiplier = 0.5f;
  public float tileCheckYOffset = 1.25f;
  private float currentSpeedMultiplier = 1f;

  public bool facingRight { get; private set; } = true;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    initialScale = transform.localScale.x;

    FindGroundTilemap();
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
    if (context.performed)
      jumpRequested = true;
  }

  void Update()
  {
    isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        groundCheckRadius,
        groundLayer
    );

    if (moveInput > 0.01f && !facingRight)
      SetFacing(true);
    else if (moveInput < -0.01f && facingRight)
      SetFacing(false);

    CheckSurfaceTile();
  }

  void FixedUpdate()
  {
    ApplyHorizontalMovement();

    if (isGrounded)
    {
      if (jumpRequested)
      {
        Jump();
        jumpRequested = false;
      }

      rb.gravityScale = normalGravityScale;
    }

    if (!isGrounded)
    {
      if (verticalInput < -0.5f && rb.linearVelocity.y <= 0f)
      {
        rb.gravityScale = fastFallGravityScale;
        Debug.Log("Fast Fall Applied");
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
    rb.angularVelocity = 0f;
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

  void CheckSurfaceTile()
  {
    if (groundTilemap == null || slowTiles == null || slowTiles.Length == 0)
    {
      currentSpeedMultiplier = 1f;
      return;
    }

    Vector3 checkPos = groundCheck.position;
    checkPos.y -= tileCheckYOffset;

    Vector3Int cellPos = groundTilemap.WorldToCell(checkPos);

    // FORÇA o centro exato do tile
    Vector3 cellCenter = groundTilemap.GetCellCenterWorld(cellPos);

    TileBase tileUnderPlayer = groundTilemap.GetTile(cellPos);

    if (tileUnderPlayer == null)
    {
      currentSpeedMultiplier = 1f;
      return;
    }

    for (int i = 0; i < slowTiles.Length; i++)
    {
      if (tileUnderPlayer == slowTiles[i])
      {
        currentSpeedMultiplier = slowMultiplier;
        return;
      }
    }

    currentSpeedMultiplier = 1f;
  }

  void FindGroundTilemap()
  {
    GameObject tilemapObj = GameObject.FindGameObjectWithTag("Tilemap");

    if (tilemapObj == null)
    {
      Debug.LogError($"PlayerMovement: Nenhum GameObject com a tag 'Tilemap' foi encontrado!");
      return;
    }

    groundTilemap = tilemapObj.GetComponent<Tilemap>();

    if (groundTilemap == null)
    {
      Debug.LogError("PlayerMovement: O objeto com a tag Tilemap não possui componente Tilemap!");
    }
  }
}

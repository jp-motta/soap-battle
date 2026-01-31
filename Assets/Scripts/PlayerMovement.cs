using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
  public float moveSpeed = 8f;
  public float jumpForce = 12f;
  public float maxSpeed = 12f;

  [Header("Ground Check")]
  public Transform groundCheck;
  public float groundCheckRadius = 0.2f;
  public LayerMask groundLayer;

  [Header("Balance")]
  public float tiltStrength = 2f;
  public float uprightStrength = 5f;

  [Header("Wear Settings")]
  public float wearSpeed = 0.15f;
  public float minScale = 0.5f;
  public float minMass = 0.8f;

  [Header("Growth Settings")]
  public float maxScale = 1.5f;
  public float maxMass = 1.2f;
  public float bubbleGrowAmount = 0.1f;

  [SerializeField] private Transform visual;

  private Rigidbody2D rb;

  private float moveInput;
  private bool jumpRequested;
  private bool isGrounded;

  private float initialScale;
  private float initialMass;

  public bool facingRight { get; private set; } = true;

  [Header("Knockback")]
  public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 2f);

  [Header("Surface Effects")]
  private Tilemap groundTilemap;
  public TileBase[] slowTiles;
  public float slowMultiplier = 0.5f;
  public float tileCheckYOffset = 1.25f;

  private float currentSpeedMultiplier = 1f;


  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();

    rb.centerOfMass = new Vector2(0f, -0.5f);

    initialScale = transform.localScale.x;
    initialMass = rb.mass;

    FindGroundTilemap();
  }

  // 🔹 INPUT SYSTEM CALLBACKS (PlayerInput chama isso)

  public void OnMove(InputAction.CallbackContext context)
  {
    moveInput = context.ReadValue<Vector2>().x;

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
    rb.AddForce(
      Vector2.right * moveInput * moveSpeed * currentSpeedMultiplier,
      ForceMode2D.Force
    );


    rb.linearVelocity = new Vector2(
        Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed),
        rb.linearVelocity.y
    );

    if (isGrounded)
    {
      rb.AddTorque(-moveInput * tiltStrength);
      rb.AddTorque(-rb.rotation * uprightStrength);

      if (jumpRequested)
      {
        Jump();
        jumpRequested = false;
      }
    }
    else
    {
      rb.angularVelocity = 0f;
    }
  }

  void Jump()
  {
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.angularVelocity = 0f;
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
  }

  void ApplyWear()
  {
    float currentScale = transform.localScale.x;
    float newScale = Mathf.Max(currentScale - wearSpeed * Time.deltaTime, minScale);

    transform.localScale = new Vector3(newScale, newScale, 1f);

    float t = Mathf.InverseLerp(initialScale, minScale, newScale);
    rb.mass = Mathf.Lerp(initialMass, minMass, t);
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
    rb.mass = Mathf.Lerp(initialMass, maxMass, t);
  }

  public void Shrink(float amount)
  {
    float currentScale = transform.localScale.x;
    float newScale = Mathf.Max(currentScale - amount, minScale);

    transform.localScale = new Vector3(newScale, newScale, 1f);

    float t = Mathf.InverseLerp(initialScale, minScale, newScale);
    rb.mass = Mathf.Lerp(initialMass, minMass, t);
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

    Debug.Log(
      $"CheckPos: {checkPos} | " +
      $"Cell: {cellPos} | " +
      $"CellCenter: {cellCenter} | " +
      $"Tile: {(tileUnderPlayer != null ? tileUnderPlayer.name : "null")}"
    );



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

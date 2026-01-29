using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class Weapon : MonoBehaviour
{
  [Header("Shooting")]
  public GameObject projectilePrefab;
  public Transform firePoint;
  public float projectileSpeed = 12f;
  public float fireRate = 0.25f;

  private float lastFireTime;

  [Header("Player")]
  public Transform player; // arrasta o player aqui
  private PlayerMovement playerController;
  private PlayerInput playerInput;

  private Gamepad playerGamepad;
  private bool usesMouse;

  private Vector2 gamepadAimInput;
  private Vector2 lastAimDirection = Vector2.right;

  void Start()
  {
    playerController = player.GetComponent<PlayerMovement>();
    playerInput = GetComponentInParent<PlayerInput>();

    DetectPlayerDevices();
  }

  void DetectPlayerDevices()
  {
    // Gamepad atribuído a ESTE player
    playerGamepad = playerInput.devices
      .OfType<Gamepad>()
      .FirstOrDefault();

    // Mouse só pertence a um player (normalmente P1)
    usesMouse = playerInput.devices.Any(d => d is Mouse);
  }

  void Update()
  {
    HandleGamepadInput();
    HandleMouseInput();

    Aim();
    HandleShoot();
  }

  // -------------------------
  // INPUT
  // -------------------------

  void HandleGamepadInput()
  {
    if (playerGamepad == null) return;

    gamepadAimInput = playerGamepad.leftStick.ReadValue();

    if (gamepadAimInput.magnitude >= 0.1f)
    {
      lastAimDirection = gamepadAimInput.normalized;
    }
  }

  void HandleMouseInput()
  {
    if (!usesMouse || Mouse.current == null) return;

    // Se o gamepad estiver ativo, ele tem prioridade
    if (playerGamepad != null && gamepadAimInput.magnitude >= 0.1f)
      return;

    Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
    mouseWorldPos.z = 0f;

    lastAimDirection = (mouseWorldPos - transform.position).normalized;
  }

  // -------------------------
  // AIM
  // -------------------------

  void Aim()
  {
    Vector2 direction = lastAimDirection;

    if (direction.sqrMagnitude < 0.001f)
      return;

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    // Corrige para lado que o jogador está olhando
    if (!playerController.facingRight)
      angle += 180f;

    transform.rotation = Quaternion.Euler(0f, 0f, angle);
  }

  // -------------------------
  // SHOOT
  // -------------------------

  void HandleShoot()
  {
    // Gamepad
    if (playerGamepad != null &&
        playerGamepad.squareButton.isPressed &&
        Time.time >= lastFireTime + fireRate)
    {
      Shoot();
      lastFireTime = Time.time;
      return;
    }

    // Mouse (somente se este player usa mouse)
    if (usesMouse &&
        Mouse.current != null &&
        Mouse.current.leftButton.isPressed &&
        Time.time >= lastFireTime + fireRate)
    {
      Shoot();
      lastFireTime = Time.time;
    }
  }

  void Shoot()
  {
    Vector2 shootDirection = lastAimDirection;

    // Limita disparo aos 180 graus à frente do jogador
    Vector2 playerForward = playerController.facingRight ? Vector2.right : Vector2.left;
    if (Vector2.Dot(shootDirection, playerForward) < 0)
      return;

    GameObject projectile = Instantiate(
      projectilePrefab,
      firePoint.position,
      Quaternion.identity
    );

    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
    rb.linearVelocity = shootDirection * projectileSpeed;
  }
}

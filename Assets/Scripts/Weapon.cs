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

  [Header("Ammo")]
  public int maxAmmo = 250;
  public int currentAmmo;
  public bool infiniteAmmo = false;

  [Header("Player")]
  public Transform player;
  private PlayerMovement playerController;
  private PlayerInput playerInput;

  private Gamepad playerGamepad;
  private bool usesMouse;

  private Vector2 gamepadAimInput;
  private Vector2 lastAimDirection = Vector2.right;

  [Header("UI")]
  public PlayerAmmoUI ammoUI;

  [Header("Bubble Shot")]
  public int bubblesPerShot = 5;
  public float coneAngle = 25f;

  void Start()
  {
    playerController = player.GetComponent<PlayerMovement>();
    playerInput = GetComponentInParent<PlayerInput>();

    currentAmmo = maxAmmo;

    DetectPlayerDevices();
    UpdateAmmoUI();
  }


  void DetectPlayerDevices()
  {
    playerGamepad = playerInput.devices
      .OfType<Gamepad>()
      .FirstOrDefault();

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
      lastAimDirection = gamepadAimInput.normalized;
  }

  void HandleMouseInput()
  {
    if (!usesMouse || Mouse.current == null) return;

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
    if (lastAimDirection.sqrMagnitude < 0.001f) return;

    float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;

    if (!playerController.facingRight)
      angle += 180f;

    transform.rotation = Quaternion.Euler(0f, 0f, angle);
  }

  // -------------------------
  // SHOOT
  // -------------------------

  void HandleShoot()
  {
    if (Time.time < lastFireTime + fireRate)
      return;

    bool wantsShoot =
      (playerGamepad != null && playerGamepad.squareButton.isPressed) ||
      (usesMouse && Mouse.current != null && Mouse.current.leftButton.isPressed);

    if (!wantsShoot)
      return;

    if (!infiniteAmmo && currentAmmo <= 0)
    {
      // Aqui você pode tocar som de "sem munição"
      return;
    }

    Shoot();
    lastFireTime = Time.time;

    if (!infiniteAmmo)
    {
      currentAmmo--;
      UpdateAmmoUI();
    }

  }

  void Shoot()
  {
    Vector2 shootDirection = lastAimDirection;

    Vector2 playerForward = playerController.facingRight ? Vector2.right : Vector2.left;
    if (Vector2.Dot(shootDirection, playerForward) < 0)
      return;

    float halfCone = coneAngle * 0.5f;

    for (int i = 0; i < bubblesPerShot; i++)
    {
      float angleOffset = Random.Range(-halfCone, halfCone);
      Vector2 finalDirection = Quaternion.Euler(0, 0, angleOffset) * shootDirection;

      GameObject projectile = Instantiate(
        projectilePrefab,
        firePoint.position,
        Quaternion.identity
      );

      BubbleProjectile bubble = projectile.GetComponent<BubbleProjectile>();
      bubble.Launch(finalDirection);
    }

    lastFireTime = Time.time;
  }

  // -------------------------
  // AMMO API
  // -------------------------

  public void Reload(int amount)
  {
    currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    UpdateAmmoUI();
  }

  public void FullReload()
  {
    currentAmmo = maxAmmo;
    UpdateAmmoUI();
  }


  void UpdateAmmoUI()
  {
    if (ammoUI != null)
      ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
  }
}

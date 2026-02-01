using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
  [Header("Life")]
  public float maxLife = 100f;
  public float currentLife;
  private float initialLife = 50f;

  [Header("Scale by Life")]
  public float minScale = 0.5f;
  public float maxScale = 1.5f;

  [Header("Death FX")]
  public GameObject deathFoamFX;

  [Header("Knockback")]
  public AnimationCurve knockbackByLife;
  public float knockbackHorizontalDamping = 0.2f;

  private Rigidbody2D rb;
  private PlayerRespawner playerRespawner;
  private bool isRespawning;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    playerRespawner = GameObject.FindGameObjectWithTag("GameController")
      .GetComponent<PlayerRespawner>();

    currentLife = initialLife;

    UpdateScale();
  }

  // -------------------------
  // LIFE
  // -------------------------

  public void Damage(float amount)
  {
    SetLife(currentLife - amount);

    if (currentLife <= 0f)
      Die();
  }

  public void Heal(float amount)
  {
    SetLife(currentLife + amount);
  }

  void SetLife(float value)
  {
    currentLife = Mathf.Clamp(value, 0f, maxLife);
    UpdateScale();
  }

  void UpdateScale()
  {
    float t = currentLife / maxLife;
    float scale = Mathf.Lerp(minScale, maxScale, t);
    transform.localScale = new Vector3(scale, scale, 1f);
  }

  // -------------------------
  // DEATH / RESPAWN
  // -------------------------

  void Die()
  {
    if (isRespawning) return;

    isRespawning = true;

    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;

    Instantiate(
      deathFoamFX,
      transform.position,
      Quaternion.identity
    );

    playerRespawner.StartCoroutine(
      playerRespawner.RespawnAfterDelay(gameObject)
    );
  }

  public void ResetHealth()
  {
    isRespawning = false;

    currentLife = initialLife;
    UpdateScale();
  }

  // -------------------------
  // KNOCKBACK
  // -------------------------

  public void ApplyKnockback(Vector2 direction, float baseForce)
  {
    direction.Normalize();

    float life01 = currentLife / maxLife;
    float lifeMultiplier = knockbackByLife.Evaluate(life01);
    float finalForce = baseForce * lifeMultiplier;

    float horizontalDir = Mathf.Sign(direction.x);
    if (horizontalDir == 0)
      horizontalDir = 1f;

    float angleRad = 35f * Mathf.Deg2Rad;

    Vector2 knockDir = new Vector2(
      Mathf.Cos(angleRad) * horizontalDir,
      Mathf.Sin(angleRad)
    );

    rb.linearVelocity = knockDir.normalized * finalForce;
  }
}

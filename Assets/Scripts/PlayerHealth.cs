using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
  [Header("Life")]
  [Range(0f, 100f)]
  public float currentLife = 50f;
  public float maxLife = 100f;

  [Header("Scale Mapping")]
  public float minScale = 0.5f;   // 0%
  public float maxScale = 1.5f;   // 100%

  [Header("Knockback")]
  public AnimationCurve knockbackByLife =
    AnimationCurve.EaseInOut(0f, 2f, 1f, 0.3f);
  // X = life normalized (0–1)
  // Y = knockback multiplier

  [Header("Tuning")]
  public float knockbackHorizontalDamping = 0.2f;

  public float knockbackAngle = 35f;
  private Rigidbody2D rb;


  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    UpdateScale();
  }

  // -------------------------
  // LIFE API
  // -------------------------

  public void Damage(float amount)
  {
    SetLife(currentLife - amount);
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

  // -------------------------
  // SCALE
  // -------------------------

  void UpdateScale()
  {
    float t = currentLife / maxLife;
    float scale = Mathf.Lerp(minScale, maxScale, t);
    transform.localScale = new Vector3(scale, scale, 1f);
  }

  // -------------------------
  // KNOCKBACK
  // -------------------------

  public void ApplyKnockback(Vector2 direction, float baseForce)
  {
    direction.Normalize();

    // Vida normalizada (0 = morto / pequeno, 1 = cheio / grande)
    float life01 = currentLife / maxLife;

    // Curva define quanto knockback a vida gera
    float lifeMultiplier = knockbackByLife.Evaluate(life01);

    float finalForce = baseForce * lifeMultiplier;

    // ❗ Forma profissional: definir velocidade diretamente
    Vector2 knockVelocity = direction * finalForce;

    // Preserva um pouco do movimento horizontal atual
    knockVelocity.x += rb.linearVelocity.x * knockbackHorizontalDamping;

    rb.linearVelocity = knockVelocity;
  }
}

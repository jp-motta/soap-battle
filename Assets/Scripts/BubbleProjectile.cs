using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BubbleProjectile : MonoBehaviour
{
  [Header("Bubble Movement")]
  public float initialSpeed = 14f;
  public float deceleration = 20f;
  public float lifetime = 2.5f;

  [Header("Gameplay")]
  public float knockbackForce = 25f;

  private Rigidbody2D rb;
  private Vector2 attackDirection;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  public void Launch(Vector2 direction)
  {
    attackDirection = direction.normalized;
    rb.linearVelocity = attackDirection * initialSpeed;
  }


  void Start()
  {
    Destroy(gameObject, lifetime);
  }

  void FixedUpdate()
  {
    // Reduz velocidade gradualmente
    rb.linearVelocity = Vector2.MoveTowards(
      rb.linearVelocity,
      Vector2.zero,
      deceleration * Time.fixedDeltaTime
    );
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
    if (playerHealth != null)
    {
      playerHealth.ApplyKnockback(-attackDirection, knockbackForce);
      playerHealth.Damage(0.5f);

      Destroy(gameObject);
      return;
    }

    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
    {
      Destroy(gameObject);
    }
  }
}

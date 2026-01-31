using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BubbleProjectile : MonoBehaviour
{
  [Header("Bubble Movement")]
  public float initialSpeed = 14f;
  public float deceleration = 20f;
  public float lifetime = 2.5f;

  [Header("Gameplay")]
  public float shrinkAmount = 0.05f;
  public float knockbackForce = 10f;

  private Rigidbody2D rb;
  private Vector2 moveDirection;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  public void Launch(Vector2 direction)
  {
    moveDirection = direction.normalized;
    rb.linearVelocity = moveDirection * initialSpeed;
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
    PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();
    if (player != null)
    {
      Vector2 knockDir = (player.transform.position - transform.position).normalized;
      player.ApplyKnockback(knockDir, knockbackForce);

      player.Shrink(shrinkAmount);
      Destroy(gameObject);
      return;
    }

    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
    {
      Destroy(gameObject);
    }
  }
}

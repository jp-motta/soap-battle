using UnityEngine;

public class StompHitbox : MonoBehaviour
{
  private Rigidbody2D playerRigidbody;
  private PlayerHealth playerHealth;

  void Awake()
  {
    playerRigidbody = GetComponentInParent<Rigidbody2D>();
    playerHealth = GetComponentInParent<PlayerHealth>();
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.layer != LayerMask.NameToLayer("HeadHitbox"))
      return;

    // Só funciona se estiver caindo
    if (playerRigidbody.linearVelocity.y >= 0f)
      return;

    PlayerHealth targetPlayerHealth = other.GetComponentInParent<PlayerHealth>();

    if (targetPlayerHealth == null || targetPlayerHealth == playerHealth)
      return;

    // Mata o jogador pisado
    targetPlayerHealth.Die();
  }
}

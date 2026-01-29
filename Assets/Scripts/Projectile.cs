using UnityEngine;

public class Projectile2D : MonoBehaviour
{
  public float shrinkAmount = 0.05f;
  public float lifetime = 3f;

  void Start()
  {
    Destroy(gameObject, lifetime);
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    // Jogador
    PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();
    if (player != null)
    {
      player.Shrink(shrinkAmount);
      Destroy(gameObject);
      return;
    }

    // Chão / Tilemap
    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
    {
      Destroy(gameObject);
      return;
    }

    // Qualquer outra coisa
    Destroy(gameObject);
  }
}

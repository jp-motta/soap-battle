using UnityEngine;

public class LifePickup : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D other)
  {
    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

    if (playerHealth != null)
    {
      playerHealth.Heal(25f);
      Destroy(gameObject);
    }
  }
}

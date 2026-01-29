using UnityEngine;

public class BubblePickup : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D other)
  {
    PlayerMovement player = other.GetComponent<PlayerMovement>();

    if (player != null)
    {
      player.Grow();
      Destroy(gameObject);
    }
  }
}

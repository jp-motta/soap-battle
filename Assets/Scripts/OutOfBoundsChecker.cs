using UnityEngine;

public class OutOfBoundsChecker : MonoBehaviour
{
  public float fallYThreshold = -15f;

  void Update()
  {
    if (transform.position.y < fallYThreshold)
    {
      GetComponent<PlayerHealth>().Damage(100f); // Damage the player by 100 points when they fall below the threshold
    }
  }
}

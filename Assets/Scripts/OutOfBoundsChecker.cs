using UnityEngine;

public class OutOfBoundsChecker : MonoBehaviour
{
  public float fallYThreshold = -15f;
  private PlayerRespawner playerRespawner;

  void Awake()
  {
    playerRespawner = GameObject.FindGameObjectWithTag("GameController")
      .GetComponent<PlayerRespawner>();
  }
  void Update()
  {
    if (transform.position.y < fallYThreshold)
    {
      playerRespawner.StartCoroutine(
        playerRespawner.RespawnAfterDelay(gameObject)
      );
    }
  }
}

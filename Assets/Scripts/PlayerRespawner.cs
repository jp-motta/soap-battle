using UnityEngine;
using System.Collections;

public class PlayerRespawner : MonoBehaviour
{
  public float respawnDelay = 5f;
  public Transform respawnPoint;

  public IEnumerator RespawnAfterDelay(GameObject player)
  {
    player.SetActive(false);

    Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

    playerHealth.ResetHealth();

    // 🚫 Para a câmera de seguir este jogador
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.UnregisterPlayer(player.transform);

    yield return new WaitForSeconds(respawnDelay);

    player.SetActive(true);

    player.transform.position = respawnPoint.position;

    // Reseta física
    playerRigidbody.linearVelocity = Vector2.zero;
    playerRigidbody.angularVelocity = 0f;
    playerRigidbody.rotation = 0f;

    // ✅ Volta a ser seguido pela câmera
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.RegisterPlayer(transform);
  }
}

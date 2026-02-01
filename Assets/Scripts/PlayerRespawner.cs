using UnityEngine;
using System.Collections;

public class PlayerRespawner : MonoBehaviour
{
  public float respawnDelay = 5f;
  public Transform respawnPoint;

  public IEnumerator RespawnAfterDelay(GameObject player)
  {
    DisablePlayer(player);

    // 🚫 Para a câmera de seguir este jogador
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.UnregisterPlayer(player.transform);

    yield return new WaitForSeconds(respawnDelay);

    Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

    EnablePlayer(player);
    playerHealth.ResetHealth();

    player.transform.position = respawnPoint.position;

    // Reseta física
    playerRigidbody.linearVelocity = Vector2.zero;
    playerRigidbody.angularVelocity = 0f;
    playerRigidbody.rotation = 0f;

    // ✅ Volta a ser seguido pela câmera
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.RegisterPlayer(player.transform);
  }

  void DisablePlayer(GameObject player)
  {
    player.GetComponent<Collider2D>().enabled = false;
    player.GetComponent<Rigidbody2D>().simulated = false;

    foreach (var r in player.GetComponentsInChildren<Renderer>())
      r.enabled = false;
  }

  void EnablePlayer(GameObject player)
  {
    player.GetComponent<Collider2D>().enabled = true;
    player.GetComponent<Rigidbody2D>().simulated = true;

    foreach (var r in player.GetComponentsInChildren<Renderer>())
      r.enabled = true;
  }
}

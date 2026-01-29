using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ReturnIfFell : MonoBehaviour
{
  public float fallYThreshold = -15f;
  public float returnDelay = 5f;
  public Vector2 respawnPosition = Vector2.zero;

  private Rigidbody2D rb;
  private bool isRespawning = false;

  void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  void Update()
  {
    if (isRespawning) return;

    if (transform.position.y < fallYThreshold)
    {
      StartCoroutine(RespawnAfterDelay());
    }
  }

  IEnumerator RespawnAfterDelay()
  {
    isRespawning = true;

    // 🚫 Para a câmera de seguir este jogador
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.UnregisterPlayer(transform);

    yield return new WaitForSeconds(returnDelay);

    transform.position = respawnPosition;

    // Reseta física
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;
    rb.rotation = 0f;

    // ✅ Volta a ser seguido pela câmera
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.RegisterPlayer(transform);

    isRespawning = false;
  }
}

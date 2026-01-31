using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
  public TextMeshProUGUI player1ScoreUI;
  public TextMeshProUGUI player2ScoreUI;

  public TextMeshProUGUI player1AmmoUI;
  public TextMeshProUGUI player2AmmoUI;

  public Sprite player1Sprite;
  public Sprite player2Sprite;

  private int playerCount = 0;

  public void OnPlayerJoined(PlayerInput playerInput)
  {
    playerCount++;

    var score = playerInput.GetComponent<PlayerScore>();
    var ammo = playerInput.GetComponent<PlayerAmmoUI>();

    score.playerIndex = playerCount;
    ammo.SetPlayerIndex(playerCount - 1);

    if (playerCount == 1)
    {
      score.scoreText = player1ScoreUI;
      ammo.ammoText = player1AmmoUI;
      SetPlayerSprite(playerInput.gameObject, player1Sprite);
    }
    else if (playerCount == 2)
    {
      score.scoreText = player2ScoreUI;
      ammo.ammoText = player2AmmoUI;
      SetPlayerSprite(playerInput.gameObject, player2Sprite);
    }
  }

  private void SetPlayerSprite(GameObject playerObject, Sprite sprite)
  {
    // Procura o SpriteRenderer no objeto filho
    SpriteRenderer spriteRenderer = playerObject.GetComponentInChildren<SpriteRenderer>();

    Debug.Log("PlayerSpawner: Setando sprite para jogador " + playerObject.name);
    Debug.Log("PlayerSpawner: Sprite encontrado: " + (spriteRenderer != null ? spriteRenderer.name : "null"));
    if (spriteRenderer != null && sprite != null)
    {
      spriteRenderer.sprite = sprite;
    }
    else if (spriteRenderer == null)
    {
      Debug.LogWarning($"PlayerSpawner: SpriteRenderer não encontrado em filhos de {playerObject.name}");
    }
  }
}

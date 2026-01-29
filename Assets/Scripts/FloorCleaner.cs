using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FloorCleaner : MonoBehaviour
{
  private Tilemap tilemap;

  public TileBase[] dirtStages; // 0..n-2 sujo | último limpo
  public float cleaningCooldown = 1.5f;
  public float yOffset = -0.5f;

  private Dictionary<Vector3Int, float> tileLastCleanedTime = new();

  private PlayerScore playerScore;

  void Awake()
  {
    playerScore = GetComponentInParent<PlayerScore>();

    GameObject tilemapObject = GameObject.FindGameObjectWithTag("Tilemap");
    if (tilemapObject != null)
      tilemap = tilemapObject.GetComponent<Tilemap>();

    if (tilemap == null)
      Debug.LogError("FloorCleaner: Tilemap não encontrado!");
  }

  void OnCollisionStay2D(Collision2D collision)
  {
    if (collision.gameObject.layer != LayerMask.NameToLayer("Ground"))
      return;

    Vector2 contactPoint = collision.contacts[0].point + Vector2.down * yOffset;
    Vector3Int cellPos = tilemap.WorldToCell(contactPoint);

    TileBase tile = tilemap.GetTile(cellPos);
    if (tile == null) return;

    if (tileLastCleanedTime.TryGetValue(cellPos, out float lastTime))
    {
      if (Time.time - lastTime < cleaningCooldown)
        return;
    }

    for (int i = 0; i < dirtStages.Length - 1; i++)
    {
      if (tile == dirtStages[i])
      {
        tilemap.SetTile(cellPos, dirtStages[i + 1]);
        tileLastCleanedTime[cellPos] = Time.time;

        // ⭐ ponto vai para ESTE jogador
        playerScore.AddPoint();
        break;
      }
    }
  }
}

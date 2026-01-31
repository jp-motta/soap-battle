using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
  [Header("Tilemap")]
  public Tilemap groundTilemap;

  [Header("Items")]
  public GameObject[] itemPrefabs;

  [Header("Spawn Settings")]
  public float spawnInterval = 20f;

  private List<Vector3Int> validCells = new List<Vector3Int>();

  void Awake()
  {
    CacheValidTiles();
  }

  void Start()
  {
    StartCoroutine(SpawnRoutine());
  }

  void CacheValidTiles()
  {
    validCells.Clear();

    BoundsInt bounds = groundTilemap.cellBounds;

    foreach (Vector3Int cell in bounds.allPositionsWithin)
    {
      if (groundTilemap.HasTile(cell))
      {
        validCells.Add(cell);
      }
    }
  }

  IEnumerator SpawnRoutine()
  {
    // Espera inicial (opcional)
    yield return new WaitForSeconds(spawnInterval);

    while (true)
    {
      SpawnRandomItem();
      yield return new WaitForSeconds(spawnInterval);
    }
  }

  void SpawnRandomItem()
  {
    if (validCells.Count == 0 || itemPrefabs.Length == 0)
      return;

    Vector3Int cell = validCells[Random.Range(0, validCells.Count)];

    Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);

    // 🔽 Ajuste para spawnar SOBRE o tile
    worldPos.y += groundTilemap.cellSize.y * 1f;

    Instantiate(
      itemPrefabs[Random.Range(0, itemPrefabs.Length)],
      worldPos,
      Quaternion.identity
    );
  }
}

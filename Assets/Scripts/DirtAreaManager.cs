using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class DirtAreaManager : MonoBehaviour
{
  public Tilemap tilemap;
  public TileBase dirtyTile;        // Primeiro estágio de sujeira
  public float dirtyInterval = 30f;

  public Vector2Int areaSize = new Vector2Int(8, 6); // largura x altura em tiles

  void Start()
  {
    StartCoroutine(DirtyLoop());
  }

  IEnumerator DirtyLoop()
  {
    while (true)
    {
      yield return new WaitForSeconds(dirtyInterval);
      DirtyRandomVisibleArea();
    }
  }

  void DirtyRandomVisibleArea()
  {
    Bounds camBounds = GetCameraBounds();

    Vector3Int minCell = tilemap.WorldToCell(camBounds.min);
    Vector3Int maxCell = tilemap.WorldToCell(camBounds.max);

    int startX = Random.Range(minCell.x, maxCell.x - areaSize.x);
    int startY = Random.Range(minCell.y, maxCell.y - areaSize.y);

    for (int x = 0; x < areaSize.x; x++)
    {
      for (int y = 0; y < areaSize.y; y++)
      {
        Vector3Int cellPos = new Vector3Int(startX + x, startY + y, 0);
        TileBase currentTile = tilemap.GetTile(cellPos);

        // Só suja tiles válidos (ex: chão)
        if (currentTile != null && currentTile != dirtyTile)
        {
          tilemap.SetTile(cellPos, dirtyTile);
        }
      }
    }
  }

  Bounds GetCameraBounds()
  {
    Camera cam = Camera.main;

    Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
    Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

    Bounds bounds = new Bounds();
    bounds.SetMinMax(bottomLeft, topRight);

    return bounds;
  }
}

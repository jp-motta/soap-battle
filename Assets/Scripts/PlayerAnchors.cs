using UnityEngine;

public class PlayerAnchors : MonoBehaviour
{
  public SpriteRenderer sprite;

  public Transform headAnchor;
  public Transform feetAnchor;

  void LateUpdate()
  {
    Bounds b = sprite.bounds;

    headAnchor.position = new Vector2(
      b.center.x,
      b.max.y
    );

    feetAnchor.position = new Vector2(
      b.center.x,
      b.min.y
    );
  }
}

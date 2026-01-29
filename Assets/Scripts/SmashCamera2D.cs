using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SmashCamera2D : MonoBehaviour
{
  public static SmashCamera2D Instance;

  [Header("Targets")]
  public List<Transform> players = new List<Transform>();

  [Header("Movement")]
  public float smoothTime = 0.25f;
  public Vector3 offset = new Vector3(0, 0, -10);

  [Header("Zoom")]
  public float minZoom = 6f;
  public float maxZoom = 14f;
  public float zoomLimiter = 10f;

  private Camera cam;
  private Vector3 velocity;

  void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);

    cam = GetComponent<Camera>();
    cam.orthographic = true;
  }

  void LateUpdate()
  {
    if (players.Count == 0) return;

    Move();
    Zoom();
  }

  public void RegisterPlayer(Transform player)
  {
    if (!players.Contains(player))
      players.Add(player);
  }

  public void UnregisterPlayer(Transform player)
  {
    if (players.Contains(player))
      players.Remove(player);
  }

  void Move()
  {
    Vector3 centerPoint = GetCenterPoint();
    Vector3 targetPosition = centerPoint + offset;

    transform.position = Vector3.SmoothDamp(
      transform.position,
      targetPosition,
      ref velocity,
      smoothTime
    );
  }

  void Zoom()
  {
    float greatestDistance = GetGreatestDistance();
    float targetZoom = Mathf.Lerp(
      maxZoom,
      minZoom,
      greatestDistance / zoomLimiter
    );

    cam.orthographicSize = Mathf.Lerp(
      cam.orthographicSize,
      targetZoom,
      Time.deltaTime * 5f
    );
  }

  Vector3 GetCenterPoint()
  {
    if (players.Count == 1)
      return players[0].position;

    Bounds bounds = new Bounds(players[0].position, Vector3.zero);

    foreach (Transform p in players)
      bounds.Encapsulate(p.position);

    return bounds.center;
  }

  float GetGreatestDistance()
  {
    Bounds bounds = new Bounds(players[0].position, Vector3.zero);

    foreach (Transform p in players)
      bounds.Encapsulate(p.position);

    return Mathf.Max(bounds.size.x, bounds.size.y);
  }
}

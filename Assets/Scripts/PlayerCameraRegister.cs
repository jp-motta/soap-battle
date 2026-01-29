using UnityEngine;

public class PlayerCameraRegister : MonoBehaviour
{
  void Start()
  {
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.RegisterPlayer(transform);
  }

  void OnDestroy()
  {
    if (SmashCamera2D.Instance != null)
      SmashCamera2D.Instance.UnregisterPlayer(transform);
  }
}

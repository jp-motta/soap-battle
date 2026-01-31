
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D other)
  {
    Weapon weapon = other.GetComponentInChildren<Weapon>();

    if (weapon != null)
    {
      weapon.Reload(250);
      Destroy(gameObject);
    }
  }
}

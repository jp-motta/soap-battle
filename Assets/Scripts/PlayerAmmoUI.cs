using TMPro;
using UnityEngine;

public class PlayerAmmoUI : MonoBehaviour
{
  public TextMeshProUGUI ammoText;
  private int playerIndex;

  public void SetPlayerIndex(int index)
  {
    playerIndex = index;
  }

  public void UpdateAmmo(int current, int max)
  {
    ammoText.text = $"P{playerIndex + 1} Ammo: {current}/{max}";
  }
}

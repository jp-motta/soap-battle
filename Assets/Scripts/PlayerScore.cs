using TMPro;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
  public int playerIndex = 1; // 1 = P1, 2 = P2, etc.
  public int score;
  public TextMeshProUGUI scoreText;

  void Start()
  {
    UpdateUI();
  }

  public void AddPoint(int amount = 1)
  {
    score += amount;
    UpdateUI();
  }

  void UpdateUI()
  {
    scoreText.text = $"P{playerIndex} score: {score}";
  }
}

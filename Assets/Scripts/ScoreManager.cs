using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
  public static ScoreManager Instance;

  public TMP_Text scoreText;

  private int score;

  void Awake()
  {
    if (Instance == null)
      Instance = this;
    else
      Destroy(gameObject);
  }

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
    scoreText.text = $"Score: {score}";
  }
}

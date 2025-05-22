using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class score : MonoBehaviour
{
    public int Score = 0; // スコアを保持する変数
    public TextMeshProUGUI scoreText; // スコア表示用のUI
    void Start()
    {
        UpdateScoreUI(); // 初期表示
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "SCORE: " + Score; // スコアを更新
    }
    public void SaveScore()
    {
        PlayerPrefs.SetInt("FinalScore", Score); // スコアを保存
        PlayerPrefs.Save(); // 保存を確定
    }
}

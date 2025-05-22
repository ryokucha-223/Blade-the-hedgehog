using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class score : MonoBehaviour
{
    public int Score = 0; // �X�R�A��ێ�����ϐ�
    public TextMeshProUGUI scoreText; // �X�R�A�\���p��UI
    void Start()
    {
        UpdateScoreUI(); // �����\��
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "SCORE: " + Score; // �X�R�A���X�V
    }
    public void SaveScore()
    {
        PlayerPrefs.SetInt("FinalScore", Score); // �X�R�A��ۑ�
        PlayerPrefs.Save(); // �ۑ����m��
    }
}

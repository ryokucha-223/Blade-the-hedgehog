using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ene : MonoBehaviour
{
    public float boostRecoveryAmount = 20f; // �񕜗ʂ�ݒ�\��
    public int points = 100; // ���̓G��|�������̃X�R�A

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag=="atk")
        {
            // �v���C���[���������ău�[�X�g�Q�[�W����
            PlayerControll player = FindObjectOfType<PlayerControll>();
            if (player != null)
            {
                player.RecoverBoost(boostRecoveryAmount);
            }
            score scoreManager = FindObjectOfType<score>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(points);
            }
            Destroy(gameObject);
        }
    }
}

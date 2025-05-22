using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ene : MonoBehaviour
{
    public float boostRecoveryAmount = 20f; // 回復量を設定可能に
    public int points = 100; // この敵を倒した時のスコア

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
            // プレイヤーを検索してブーストゲージを回復
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

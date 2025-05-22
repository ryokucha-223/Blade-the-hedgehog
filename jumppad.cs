using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumppad : MonoBehaviour
{
    public float jumpForce = 10f; // ジャンプ台で加える速度

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突したオブジェクトのRigidbody2Dを取得
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 衝突したオブジェクトの接触点を取得
            ContactPoint2D contact = collision.contacts[0];

            // デバッグ: 接触点の法線確認
            Debug.Log($"Contact Normal Y: {contact.normal.y}");

            // 上から触れた場合のみ処理
            if (contact.normal.y <-0.5f) // Y方向の法線が上を向いている
            {
                Debug.Log($"Applying jump force to {collision.gameObject.name}");

                // 上方向に速度を設定
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            else
            {
                Debug.Log($"{collision.gameObject.name} did not approach from above.");
            }
        }
        else
        {
            Debug.LogWarning($"Object {collision.gameObject.name} does not have a Rigidbody2D.");
        }
    }
}

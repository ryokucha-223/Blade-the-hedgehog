using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumppad : MonoBehaviour
{
    public float jumpForce = 10f; // �W�����v��ŉ����鑬�x

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �Փ˂����I�u�W�F�N�g��Rigidbody2D���擾
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // �Փ˂����I�u�W�F�N�g�̐ڐG�_���擾
            ContactPoint2D contact = collision.contacts[0];

            // �f�o�b�O: �ڐG�_�̖@���m�F
            Debug.Log($"Contact Normal Y: {contact.normal.y}");

            // �ォ��G�ꂽ�ꍇ�̂ݏ���
            if (contact.normal.y <-0.5f) // Y�����̖@������������Ă���
            {
                Debug.Log($"Applying jump force to {collision.gameObject.name}");

                // ������ɑ��x��ݒ�
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

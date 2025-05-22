using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform target;              // �v���C���[��Transform
    public Vector3 offset = new Vector3(2, 1, -10); // �J�����̊�{�I�t�Z�b�g
    public float smoothSpeed = 0.125f;    // �J�����Ǐ]�̃X���[�Y��
    public Vector2 minBounds;            // �J�����̍ŏ����W����
    public Vector2 maxBounds;            // �J�����̍ő���W����
    public float jumpHeightThreshold = 2f; // �W�����v����Y����Ǐ]���Ȃ��͈�
    public LayerMask groundLayer;         // �n�ʂ̃��C���[

    private Rigidbody2D targetRb;        // �v���C���[��Rigidbody2D
    private bool isGrounded;             // �v���C���[���n�ʂɂ��Ă��邩
    private float groundedY;             // �n�ʂɂ��鎞��Y���W

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            groundedY = target.position.y; // ������Y�ʒu���L�^
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // �v���C���[��Y���̕ψʂ��������l�𒴂��Ă��邩���m�F
        bool shouldFollowY = Mathf.Abs(target.position.y - groundedY) > jumpHeightThreshold;

        // �v���C���[��X���Ɋ�Â��ĖڕW�ʒu��ݒ�
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            shouldFollowY ? target.position.y + offset.y : groundedY + offset.y, // �����ɉ�����Y�ʒu��ݒ�
            offset.z
        );

        // �J�����̈ʒu��͈͓��ɐ���
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);

        // �J�������X���[�Y�Ɉړ�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }


    // �n�ʔ�����s�����\�b�h
    private bool IsGrounded()
    {
        float rayLength = 0.1f; // ����̂��߂̃��C�̒���
        RaycastHit2D hit = Physics2D.Raycast(target.position, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }
}

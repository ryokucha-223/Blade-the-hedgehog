using UnityEngine;

public class camera : MonoBehaviour
{
    public Transform target;              // プレイヤーのTransform
    public Vector3 offset = new Vector3(2, 1, -10); // カメラの基本オフセット
    public float smoothSpeed = 0.125f;    // カメラ追従のスムーズさ
    public Vector2 minBounds;            // カメラの最小座標制限
    public Vector2 maxBounds;            // カメラの最大座標制限
    public float jumpHeightThreshold = 2f; // ジャンプ時にY軸を追従しない範囲
    public LayerMask groundLayer;         // 地面のレイヤー

    private Rigidbody2D targetRb;        // プレイヤーのRigidbody2D
    private bool isGrounded;             // プレイヤーが地面についているか
    private float groundedY;             // 地面にいる時のY座標

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            groundedY = target.position.y; // 初期のY位置を記録
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // プレイヤーのY軸の変位がしきい値を超えているかを確認
        bool shouldFollowY = Mathf.Abs(target.position.y - groundedY) > jumpHeightThreshold;

        // プレイヤーのX軸に基づいて目標位置を設定
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            shouldFollowY ? target.position.y + offset.y : groundedY + offset.y, // 条件に応じてY位置を設定
            offset.z
        );

        // カメラの位置を範囲内に制限
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);

        // カメラをスムーズに移動
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }


    // 地面判定を行うメソッド
    private bool IsGrounded()
    {
        float rayLength = 0.1f; // 判定のためのレイの長さ
        RaycastHit2D hit = Physics2D.Raycast(target.position, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }
}

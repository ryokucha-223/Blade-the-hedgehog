using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks; // Task を使用するために必要
using UnityEngine.SceneManagement;


public class PlayerControll : MonoBehaviour
{
    [SerializeField]
    GameObject groundHitObject;//地面判定を調べるオブジェクト
    [SerializeField]
    LayerMask groundLayer;//地面のレイヤー
    [SerializeField]
    private LayerMask loopLayer; // ループ専用のレイヤー

    // 地面との補正用パラメータ
    [SerializeField] private float groundCheckDistance = 1f; // レイキャストの距離

    [SerializeField]
    private bool isInLoop = false; // ループ内にいるかどうか

    [SerializeField] float rayLength = 0.5f; // レイキャストの長さ
    [SerializeField]
    Slider boostsl;

    [SerializeField] int MAXHP = 2;
    int HP = 2;
    [SerializeField]
    float Boostpower = 100f, lostbp = 1f, MAXboostpower = 100f;
    [SerializeField]
    float speed = 10f;
    [SerializeField]
    float boostspeed = 15f;
    [SerializeField]
    float JumpPower = 400f;
    [SerializeField]
    float decelerationRate = 0.99f;//減速
    public float floatUpSpeed = 5f;  // 浮かせる速度（上向きの力）

    public Transform playerTransform;
    bool muki = true;
    bool isInAttackCooldown;//攻撃できるかどうかの判定
    bool OnGround;//地面判定true=地面の上
    bool prevOnGround;
    bool takingdamage = false;//被ダメ時
    bool boosting = false;//ブーストフラグ
    bool ishoming = false;
    bool isdead = false;
    int numJump = 0;
    private float lastAttackTime;
    private float lastAttackInputTime;
    private int attackCount = 0;//近接の回数（最大３）
    private Quaternion targetRotation;
    // int count = 0;
    Rigidbody2D rb;
    Animator anim;
    [SerializeField] AudioSource snd;
    private SpriteRenderer sprite;
    private Tween blinkTween; // 点滅処理用のTween
    public GameObject atkcol;//攻撃判定
    [SerializeField] GameObject slobj;

    [SerializeField]
    AudioClip se_jump,se_coin,se_heal,se_hit,se_boost;
    [SerializeField]
    Image[] heartImages; // ハートの画像を格納する配列
    [SerializeField]
    Sprite fullHeart; // フルのハート画像
    [SerializeField]
    Sprite emptyHeart; // 空のハート画像
    [SerializeField] GameObject cursorPrefab; // カーソルのプレハブ
    [SerializeField] GameObject cursorInstance;

    [SerializeField] private float homingRange = 5f; // ホーミング範囲
    [SerializeField] private float homingSpeed = 10f; // ホーミング速度
    [SerializeField] private LayerMask enemyLayer; // 敵のレイヤーマスク
    [SerializeField] private GameObject attackHitbox; // 攻撃判定のオブジェクト
    [SerializeField] GameObject BoostCol;
    [SerializeField] float attackWindow = 0.3f; // 攻撃入力受付時間（秒）
    [SerializeField] float attackCooldown = 0.5f;//近接のクールタイム
    [SerializeField] float attackCooldownAfterCombo = 1.0f; // 三段目の後のクールタイム

    private Transform targetEnemy; // ホーミング対象の敵

    private Fademane fademane;//シーン切り替え用
     score scoreManager;

    // Start is called before the first frame update
    void Awake()
    {
        fademane = FindObjectOfType<Fademane>();//fademaneの取得
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        snd = gameObject.GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>(); // SpriteRendererの取得
        HP = MAXHP;
        isdead = false;
        numJump = 0;
        boostsl.value = 1f;
        Boostpower = MAXboostpower;
        boosting = false;
        string currentScene = SceneManager.GetActiveScene().name;
        GameManager.Instance.SavePreviousScene(currentScene); // 現在のシーンを保存
        score scoreManager = FindObjectOfType<score>();
    }

    // Update is called once per frame
    void Update()
    {
        if (HP <= 0)
        {
            anim.SetBool("dead", true);
            isdead = true;
        }
        if (transform.position.y < -240) // -10 は高さの基準値
        {
            fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // フェード時間は適宜調整
        }
        Hurt();
        RotToGround();
        ApplyRotation();
        Move();
        Boost();
        Jump();
        checkGround();
        FindNearestEnemy();
        ATK();
    }



    void Move()
    {
        if (takingdamage) return;// ダメージ中は動けない
        if (isdead) return;//死亡時は動けない
        float Inhol = 0;//velocityに代入するためにspeedを入れとくやつ
        if (!Input.GetKey(KeyCode.X))//ブースト上書き防止
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Inhol = speed;
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                muki = true;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Inhol = -speed;
                transform.localScale = new Vector3(-0.7f, 0.7f, 0.7f);
                muki = false;
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x * decelerationRate, rb.velocity.y);
            }
        }

        if (OnGround)
        {
            anim.SetFloat("speed", Mathf.Abs(Inhol));
            anim.speed = Mathf.Abs(Inhol) > 0 ? 1.2f : 1f; // 移動中は速く再生
        }
        else
        {
            anim.SetFloat("speed", 0); // 空中ではスピードアニメーションを止める
            anim.speed = 1f;
        }

        // 現在の垂直速度を維持しつつ、横方向の速度を設定
        rb.velocity = new Vector2(Inhol, rb.velocity.y);

        // 減速処理（左右入力がない場合のみ）
        if (Inhol == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * decelerationRate, rb.velocity.y);
        }

    }
    void Boost()
    {
        if (takingdamage) return;// ダメージ中は動けない
        if (isdead) return;//死亡時は動けない
        if (Input.GetKeyDown(KeyCode.X) && Boostpower > 0 && !ishoming)
        {
            snd.PlayOneShot(se_boost);
        }
        if (Input.GetKey(KeyCode.X) && Boostpower > 0 && !ishoming) // Xキーを押している間
        {
            boosting = true;
            anim.SetBool("Boost", true);
            BoostCol.SetActive(true);
            rb.velocity = new Vector2((muki ? boostspeed : -boostspeed), rb.velocity.y);
            Boostpower -= lostbp;
            boostsl.value = (float)Boostpower / (float)MAXboostpower;
        }
        else
        {
            // Xキーを離したら
            anim.SetBool("Boost", false);
            BoostCol.SetActive(false);
            boosting = false;
            if (Input.GetKeyUp(KeyCode.X))
            {
                rb.velocity = new Vector2((muki ? speed : -speed), rb.velocity.y);
            }
        }
    }
    void Jump()
    {
        if (takingdamage) return;// ダメージ中は動けない
        if (isdead) return;//死亡時は動けない
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (OnGround)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + JumpPower);
                //rb.AddForce(transform.up * JumpPower);
                snd.PlayOneShot(se_jump);
                anim.SetBool("isJump", true);
                numJump++;
            }
            else if (numJump == 1)
            {
                rb.velocity = new Vector2(rb.velocity.x, JumpPower);
                // rb.AddForce(transform.up * JumpPower);
                snd.PlayOneShot(se_jump);
                anim.SetBool("isJump", true);
                numJump++;
            }
        }
        if (Input.GetKeyDown(KeyCode.Z) && Input.anyKeyDown)
        {
            return;
        }
        if (!prevOnGround && OnGround)//前回空中今回地面の時
        {
            anim.SetBool("isJump", false);
            numJump = 0;
        }
    }
    void checkGround()
    {
        prevOnGround = OnGround;

        // 地面判定（足元のオブジェクト取得）
        // 0.2fは判定半径、groundLayerとloopLayerの両方をチェック
        bool isOnGround = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.2f, groundLayer);
        bool isOnLoop = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.2f, loopLayer);

        // groundLayerまたはloopLayerに重なっている場合にOnGroundをtrueにする
        OnGround = isOnGround || isOnLoop;
    }

    private async Task ATK()
    {
        if (takingdamage) return; // ダメージ中は動けない
        if (isdead) return;//死亡時は動けない

        if (Input.GetKeyDown(KeyCode.C))
        {
            // ジャンプ中で敵がいる場合、ホーミング開始
            if (!OnGround && FindNearestEnemy() != null)
            {
                StartCoroutine(HomingToEnemy());
            }
            else if (!OnGround && FindNearestEnemy() == null)
            {
                return;
            }
            else
            {
                // クールタイム中かどうかをチェック
                if (isInAttackCooldown)
                {
                    return;
                }
                float currentTime = Time.time;

                // クールタイムが経過していれば攻撃カウントをリセット
                if (currentTime - lastAttackTime > attackCooldown)
                {
                    attackCount = 0;
                }

                // 攻撃カウントと攻撃ウィンドウのチェック
                if (attackCount == 0 || currentTime - lastAttackInputTime < attackWindow)
                {
                    attackCount++;
                    lastAttackTime = currentTime;
                    lastAttackInputTime = currentTime;

                    // 攻撃アニメーションのトリガー
                    switch (attackCount)
                    {
                        case 1:
                            anim.SetTrigger("Atk1");
                            // canstep = false;
                            break;
                        case 2:
                            anim.SetTrigger("Atk2");
                            // canstep = false;
                            break;
                        case 3:
                            anim.SetTrigger("Atk3");
                            // canstep = false;
                            attackCount = 0; // 3回目の攻撃後はリセット

                            // 3発目の攻撃後にクールタイムを設定
                            isInAttackCooldown = true;
                            await Task.Delay((int)(attackCooldownAfterCombo * 1000)); // クールタイム後にフラグをリセット
                            isInAttackCooldown = false;
                            break;
                    }
                }
                else
                {
                    // 攻撃カウントが3未満であれば通常のクールタイム
                    await Task.Delay((int)(attackCooldown * 1000));
                }
            }
        }
    }
    //アニメーションから起動するやつ
    void enddamage()
    {
        takingdamage = false;
        rb.velocity = Vector2.zero; // ノックバックを完全に停止
        // 点滅処理がある場合に停止し、透明度を1に戻す
        if (blinkTween != null && blinkTween.IsActive())
        {
            blinkTween.Kill(); // Tweenの停止
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f); // 透明度を戻す
        }
    }

    void enddead()
    {
        fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // フェード時間は適宜調整
    }

    void Hurt()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < HP)
            {
                heartImages[i].sprite = fullHeart; // HPがある場合はフルのハートを表示
            }
            else
            {
                heartImages[i].sprite = emptyHeart; // HPが無い場合は空のハートを表示
            }
        }
    }

    void atkanim()
    {
        boosting = true;
        // snd.PlayOneShot(SE_slash);
        float direction = muki ? 1 : -1;
        float rot = muki ? 0 : 180;
        Vector2 position = new Vector2(transform.position.x + 1f * direction, transform.position.y + 0.2f);
        Quaternion rotation = Quaternion.Euler(-238.51f, 0, 0 + rot);
        atkcol = Instantiate(slobj, position, Quaternion.identity);
    }

    void atkend()
    {
        boosting = false;
        Destroy(atkcol);
    }

    public void RecoverBoost(float amount)
    {
        Boostpower = Mathf.Clamp(Boostpower + amount, 0, MAXboostpower);
        boostsl.value = (float)Boostpower / (float)MAXboostpower;
        Debug.Log("Boost Gauge: " + Boostpower);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (anim.GetBool("Homing"))
        {
            StopHoming();
        }
        if (col.gameObject.tag == "ene" && !boosting && !takingdamage && !ishoming)
        {
            snd.PlayOneShot(se_hit);
            HP -= 1;
            Debug.Log("HP: " + HP);
            takingdamage = true;
            // 吹き飛ばす方向の計算
            Vector2 hitDirection = (transform.position - col.transform.position).normalized;
            float knockbackForce = 8f;

            // 吹き飛ばし処理
            rb.velocity = new Vector2(hitDirection.x * knockbackForce, rb.velocity.y + 5f);

            // 点滅アニメーション（透明度を0.3fまで落とす → 1f に戻す）
            blinkTween = sprite.DOFade(0.3f, 0.1f)
                .SetLoops(-1, LoopType.Yoyo) // 無限ループで透明度を往復
                .SetEase(Ease.InOutQuad);

            // ダメージアニメーションの再生
            anim.SetTrigger("damage");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "goal")
        {
            FindObjectOfType<score>().SaveScore();
            fademane.ChangeSceneWithFade(1f, 0.5f, "result"); // フェード時間は適宜調整
        }
        if (col.gameObject.tag == "fall")
        {
            fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // フェード時間は適宜調整
        }
        if(col.gameObject.tag=="heal")
        {
            snd.PlayOneShot(se_heal);
            if (HP<MAXHP)
            {
                HP++;
            }
            Destroy(col.gameObject);
        }
        if (col.gameObject.tag=="coin")
        {
            snd.PlayOneShot(se_coin);
            score scoreManager = FindObjectOfType<score>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(10);
            }
            Boostpower = Mathf.Clamp(Boostpower + 5, 0, MAXboostpower);
            boostsl.value = (float)Boostpower / (float)MAXboostpower;
            Destroy(col.gameObject);
        }
    }

    // 近くの敵を検出するメソッド
    Transform FindNearestEnemy()
    {
        targetEnemy = null; // ターゲットリセット
        float nearestDistance = Mathf.Infinity;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, homingRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetEnemy = enemy.transform; // 最も近い敵を更新
            }
        }

        // ターゲットが正しく設定された場合のみカーソルを表示
        UpdateCursor(targetEnemy);

        return targetEnemy;
    }

    void UpdateCursor(Transform target)
    {
        // ターゲットが存在しない場合、カーソルを非表示
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            if (cursorInstance != null) cursorInstance.SetActive(false); // カーソルを非表示
            return;
        }


        // カーソルが存在しない場合、新たに生成
        if (cursorInstance == null)
        {
            cursorInstance = Instantiate(cursorPrefab);
        }

        if (!ishoming)
        {
            if (cursorInstance != null) cursorInstance.SetActive(false); // カーソルを非表示
            return;
        }

        // ホーミング中ならカーソルを表示してターゲットの位置に移動
        if (ishoming)
        {
            cursorInstance.SetActive(true);
            cursorInstance.transform.position = target.position; // ターゲットの位置にカーソルを配置
        }
    }

    IEnumerator HomingToEnemy()
    {
        Transform target = FindNearestEnemy();

        if (target == null) yield break;

        anim.SetBool("Homing", true);
        ishoming = true;
        attackHitbox.SetActive(true);

        while (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * homingSpeed;

            if (Vector2.Distance(transform.position, target.position) < 0.5f)
            {
                rb.velocity = Vector2.zero;
                break;
            }

            yield return null;
        }

        // ホーミング終了処理
        EndHoming();
    }

    // ホーミング終了後に浮かせる処理
    private void EndHoming()
    {
        ishoming = false;
        anim.SetBool("Homing", false);
        attackHitbox.SetActive(false);
        rb.velocity = new Vector2(rb.velocity.x, floatUpSpeed); // 上方向に浮かせる


        // カーソル非表示
        if (cursorInstance != null) cursorInstance.SetActive(false);

        Debug.Log("ホーミング終了、浮かせました");
    }

    private void StopHoming()
    {
        StopCoroutine("HomingToEnemy"); // ホーミングコルーチンを強制終了
        rb.velocity = Vector2.zero;     // 速度を停止
        anim.SetBool("Homing", false); // ホーミングアニメーション終了
        attackHitbox.SetActive(false); // 攻撃判定を無効化
        if (cursorInstance != null) cursorInstance.SetActive(false); // カーソルを非表示
        anim.SetTrigger("atk");        // 通常攻撃アニメーションを再生
    }

    private void RotToGround()
    {
        // 通常の地面判定
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        RaycastHit2D loopHit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, loopLayer);

        // プレイヤーが空中にいる場合
        if (!OnGround)
        {
            // 空中の場合は元の回転に戻す
            targetRotation = Quaternion.identity;
            return; // 空中の場合は回転を更新しない
        }

        if (loopHit.collider != null)
        {
            // ループの地形に対する角度を計算
            Vector2 loopNormal = loopHit.normal;
            float loopAngle = Mathf.Atan2(loopNormal.y, loopNormal.x) * Mathf.Rad2Deg;

            // ループにいる場合の回転を設定
            targetRotation = Quaternion.Euler(0, 0, loopAngle - 90);
            isInLoop = true; // ループ内フラグを設定
        }
        else if (groundHit.collider != null && !isInLoop)
        {
            // 通常地面の角度を計算
            Vector2 groundNormal = groundHit.normal;
            float groundAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;

            // 通常の地面回転を設定
            targetRotation = Quaternion.Euler(0, 0, groundAngle - 90);
        }
        else if (isInLoop && loopHit.collider == null)
        {
            // ループから離れた場合、角度を元に戻す
            isInLoop = false;
            targetRotation = Quaternion.identity; // デフォルトの回転に戻す
        }
    }

    private void ApplyRotation()
    {
        // 角度の適用（スムーズに回転する場合は補間）
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}
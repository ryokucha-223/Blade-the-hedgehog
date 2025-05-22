using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks; // Task ���g�p���邽�߂ɕK�v
using UnityEngine.SceneManagement;


public class PlayerControll : MonoBehaviour
{
    [SerializeField]
    GameObject groundHitObject;//�n�ʔ���𒲂ׂ�I�u�W�F�N�g
    [SerializeField]
    LayerMask groundLayer;//�n�ʂ̃��C���[
    [SerializeField]
    private LayerMask loopLayer; // ���[�v��p�̃��C���[

    // �n�ʂƂ̕␳�p�p�����[�^
    [SerializeField] private float groundCheckDistance = 1f; // ���C�L���X�g�̋���

    [SerializeField]
    private bool isInLoop = false; // ���[�v���ɂ��邩�ǂ���

    [SerializeField] float rayLength = 0.5f; // ���C�L���X�g�̒���
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
    float decelerationRate = 0.99f;//����
    public float floatUpSpeed = 5f;  // �������鑬�x�i������̗́j

    public Transform playerTransform;
    bool muki = true;
    bool isInAttackCooldown;//�U���ł��邩�ǂ����̔���
    bool OnGround;//�n�ʔ���true=�n�ʂ̏�
    bool prevOnGround;
    bool takingdamage = false;//��_����
    bool boosting = false;//�u�[�X�g�t���O
    bool ishoming = false;
    bool isdead = false;
    int numJump = 0;
    private float lastAttackTime;
    private float lastAttackInputTime;
    private int attackCount = 0;//�ߐڂ̉񐔁i�ő�R�j
    private Quaternion targetRotation;
    // int count = 0;
    Rigidbody2D rb;
    Animator anim;
    [SerializeField] AudioSource snd;
    private SpriteRenderer sprite;
    private Tween blinkTween; // �_�ŏ����p��Tween
    public GameObject atkcol;//�U������
    [SerializeField] GameObject slobj;

    [SerializeField]
    AudioClip se_jump,se_coin,se_heal,se_hit,se_boost;
    [SerializeField]
    Image[] heartImages; // �n�[�g�̉摜���i�[����z��
    [SerializeField]
    Sprite fullHeart; // �t���̃n�[�g�摜
    [SerializeField]
    Sprite emptyHeart; // ��̃n�[�g�摜
    [SerializeField] GameObject cursorPrefab; // �J�[�\���̃v���n�u
    [SerializeField] GameObject cursorInstance;

    [SerializeField] private float homingRange = 5f; // �z�[�~���O�͈�
    [SerializeField] private float homingSpeed = 10f; // �z�[�~���O���x
    [SerializeField] private LayerMask enemyLayer; // �G�̃��C���[�}�X�N
    [SerializeField] private GameObject attackHitbox; // �U������̃I�u�W�F�N�g
    [SerializeField] GameObject BoostCol;
    [SerializeField] float attackWindow = 0.3f; // �U�����͎�t���ԁi�b�j
    [SerializeField] float attackCooldown = 0.5f;//�ߐڂ̃N�[���^�C��
    [SerializeField] float attackCooldownAfterCombo = 1.0f; // �O�i�ڂ̌�̃N�[���^�C��

    private Transform targetEnemy; // �z�[�~���O�Ώۂ̓G

    private Fademane fademane;//�V�[���؂�ւ��p
     score scoreManager;

    // Start is called before the first frame update
    void Awake()
    {
        fademane = FindObjectOfType<Fademane>();//fademane�̎擾
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        snd = gameObject.GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>(); // SpriteRenderer�̎擾
        HP = MAXHP;
        isdead = false;
        numJump = 0;
        boostsl.value = 1f;
        Boostpower = MAXboostpower;
        boosting = false;
        string currentScene = SceneManager.GetActiveScene().name;
        GameManager.Instance.SavePreviousScene(currentScene); // ���݂̃V�[����ۑ�
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
        if (transform.position.y < -240) // -10 �͍����̊�l
        {
            fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // �t�F�[�h���Ԃ͓K�X����
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
        if (takingdamage) return;// �_���[�W���͓����Ȃ�
        if (isdead) return;//���S���͓����Ȃ�
        float Inhol = 0;//velocity�ɑ�����邽�߂�speed�����Ƃ����
        if (!Input.GetKey(KeyCode.X))//�u�[�X�g�㏑���h�~
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
            anim.speed = Mathf.Abs(Inhol) > 0 ? 1.2f : 1f; // �ړ����͑����Đ�
        }
        else
        {
            anim.SetFloat("speed", 0); // �󒆂ł̓X�s�[�h�A�j���[�V�������~�߂�
            anim.speed = 1f;
        }

        // ���݂̐������x���ێ����A�������̑��x��ݒ�
        rb.velocity = new Vector2(Inhol, rb.velocity.y);

        // ���������i���E���͂��Ȃ��ꍇ�̂݁j
        if (Inhol == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * decelerationRate, rb.velocity.y);
        }

    }
    void Boost()
    {
        if (takingdamage) return;// �_���[�W���͓����Ȃ�
        if (isdead) return;//���S���͓����Ȃ�
        if (Input.GetKeyDown(KeyCode.X) && Boostpower > 0 && !ishoming)
        {
            snd.PlayOneShot(se_boost);
        }
        if (Input.GetKey(KeyCode.X) && Boostpower > 0 && !ishoming) // X�L�[�������Ă����
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
            // X�L�[�𗣂�����
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
        if (takingdamage) return;// �_���[�W���͓����Ȃ�
        if (isdead) return;//���S���͓����Ȃ�
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
        if (!prevOnGround && OnGround)//�O��󒆍���n�ʂ̎�
        {
            anim.SetBool("isJump", false);
            numJump = 0;
        }
    }
    void checkGround()
    {
        prevOnGround = OnGround;

        // �n�ʔ���i�����̃I�u�W�F�N�g�擾�j
        // 0.2f�͔��蔼�a�AgroundLayer��loopLayer�̗������`�F�b�N
        bool isOnGround = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.2f, groundLayer);
        bool isOnLoop = Physics2D.OverlapCircle(groundHitObject.transform.position, 0.2f, loopLayer);

        // groundLayer�܂���loopLayer�ɏd�Ȃ��Ă���ꍇ��OnGround��true�ɂ���
        OnGround = isOnGround || isOnLoop;
    }

    private async Task ATK()
    {
        if (takingdamage) return; // �_���[�W���͓����Ȃ�
        if (isdead) return;//���S���͓����Ȃ�

        if (Input.GetKeyDown(KeyCode.C))
        {
            // �W�����v���œG������ꍇ�A�z�[�~���O�J�n
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
                // �N�[���^�C�������ǂ������`�F�b�N
                if (isInAttackCooldown)
                {
                    return;
                }
                float currentTime = Time.time;

                // �N�[���^�C�����o�߂��Ă���΍U���J�E���g�����Z�b�g
                if (currentTime - lastAttackTime > attackCooldown)
                {
                    attackCount = 0;
                }

                // �U���J�E���g�ƍU���E�B���h�E�̃`�F�b�N
                if (attackCount == 0 || currentTime - lastAttackInputTime < attackWindow)
                {
                    attackCount++;
                    lastAttackTime = currentTime;
                    lastAttackInputTime = currentTime;

                    // �U���A�j���[�V�����̃g���K�[
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
                            attackCount = 0; // 3��ڂ̍U����̓��Z�b�g

                            // 3���ڂ̍U����ɃN�[���^�C����ݒ�
                            isInAttackCooldown = true;
                            await Task.Delay((int)(attackCooldownAfterCombo * 1000)); // �N�[���^�C����Ƀt���O�����Z�b�g
                            isInAttackCooldown = false;
                            break;
                    }
                }
                else
                {
                    // �U���J�E���g��3�����ł���Βʏ�̃N�[���^�C��
                    await Task.Delay((int)(attackCooldown * 1000));
                }
            }
        }
    }
    //�A�j���[�V��������N��������
    void enddamage()
    {
        takingdamage = false;
        rb.velocity = Vector2.zero; // �m�b�N�o�b�N�����S�ɒ�~
        // �_�ŏ���������ꍇ�ɒ�~���A�����x��1�ɖ߂�
        if (blinkTween != null && blinkTween.IsActive())
        {
            blinkTween.Kill(); // Tween�̒�~
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f); // �����x��߂�
        }
    }

    void enddead()
    {
        fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // �t�F�[�h���Ԃ͓K�X����
    }

    void Hurt()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < HP)
            {
                heartImages[i].sprite = fullHeart; // HP������ꍇ�̓t���̃n�[�g��\��
            }
            else
            {
                heartImages[i].sprite = emptyHeart; // HP�������ꍇ�͋�̃n�[�g��\��
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
            // ������΂������̌v�Z
            Vector2 hitDirection = (transform.position - col.transform.position).normalized;
            float knockbackForce = 8f;

            // ������΂�����
            rb.velocity = new Vector2(hitDirection.x * knockbackForce, rb.velocity.y + 5f);

            // �_�ŃA�j���[�V�����i�����x��0.3f�܂ŗ��Ƃ� �� 1f �ɖ߂��j
            blinkTween = sprite.DOFade(0.3f, 0.1f)
                .SetLoops(-1, LoopType.Yoyo) // �������[�v�œ����x������
                .SetEase(Ease.InOutQuad);

            // �_���[�W�A�j���[�V�����̍Đ�
            anim.SetTrigger("damage");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "goal")
        {
            FindObjectOfType<score>().SaveScore();
            fademane.ChangeSceneWithFade(1f, 0.5f, "result"); // �t�F�[�h���Ԃ͓K�X����
        }
        if (col.gameObject.tag == "fall")
        {
            fademane.ChangeSceneWithFade(1f, 0.5f, "over"); // �t�F�[�h���Ԃ͓K�X����
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

    // �߂��̓G�����o���郁�\�b�h
    Transform FindNearestEnemy()
    {
        targetEnemy = null; // �^�[�Q�b�g���Z�b�g
        float nearestDistance = Mathf.Infinity;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, homingRange, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetEnemy = enemy.transform; // �ł��߂��G���X�V
            }
        }

        // �^�[�Q�b�g���������ݒ肳�ꂽ�ꍇ�̂݃J�[�\����\��
        UpdateCursor(targetEnemy);

        return targetEnemy;
    }

    void UpdateCursor(Transform target)
    {
        // �^�[�Q�b�g�����݂��Ȃ��ꍇ�A�J�[�\�����\��
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            if (cursorInstance != null) cursorInstance.SetActive(false); // �J�[�\�����\��
            return;
        }


        // �J�[�\�������݂��Ȃ��ꍇ�A�V���ɐ���
        if (cursorInstance == null)
        {
            cursorInstance = Instantiate(cursorPrefab);
        }

        if (!ishoming)
        {
            if (cursorInstance != null) cursorInstance.SetActive(false); // �J�[�\�����\��
            return;
        }

        // �z�[�~���O���Ȃ�J�[�\����\�����ă^�[�Q�b�g�̈ʒu�Ɉړ�
        if (ishoming)
        {
            cursorInstance.SetActive(true);
            cursorInstance.transform.position = target.position; // �^�[�Q�b�g�̈ʒu�ɃJ�[�\����z�u
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

        // �z�[�~���O�I������
        EndHoming();
    }

    // �z�[�~���O�I����ɕ������鏈��
    private void EndHoming()
    {
        ishoming = false;
        anim.SetBool("Homing", false);
        attackHitbox.SetActive(false);
        rb.velocity = new Vector2(rb.velocity.x, floatUpSpeed); // ������ɕ�������


        // �J�[�\����\��
        if (cursorInstance != null) cursorInstance.SetActive(false);

        Debug.Log("�z�[�~���O�I���A�������܂���");
    }

    private void StopHoming()
    {
        StopCoroutine("HomingToEnemy"); // �z�[�~���O�R���[�`���������I��
        rb.velocity = Vector2.zero;     // ���x���~
        anim.SetBool("Homing", false); // �z�[�~���O�A�j���[�V�����I��
        attackHitbox.SetActive(false); // �U������𖳌���
        if (cursorInstance != null) cursorInstance.SetActive(false); // �J�[�\�����\��
        anim.SetTrigger("atk");        // �ʏ�U���A�j���[�V�������Đ�
    }

    private void RotToGround()
    {
        // �ʏ�̒n�ʔ���
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        RaycastHit2D loopHit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, loopLayer);

        // �v���C���[���󒆂ɂ���ꍇ
        if (!OnGround)
        {
            // �󒆂̏ꍇ�͌��̉�]�ɖ߂�
            targetRotation = Quaternion.identity;
            return; // �󒆂̏ꍇ�͉�]���X�V���Ȃ�
        }

        if (loopHit.collider != null)
        {
            // ���[�v�̒n�`�ɑ΂���p�x���v�Z
            Vector2 loopNormal = loopHit.normal;
            float loopAngle = Mathf.Atan2(loopNormal.y, loopNormal.x) * Mathf.Rad2Deg;

            // ���[�v�ɂ���ꍇ�̉�]��ݒ�
            targetRotation = Quaternion.Euler(0, 0, loopAngle - 90);
            isInLoop = true; // ���[�v���t���O��ݒ�
        }
        else if (groundHit.collider != null && !isInLoop)
        {
            // �ʏ�n�ʂ̊p�x���v�Z
            Vector2 groundNormal = groundHit.normal;
            float groundAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;

            // �ʏ�̒n�ʉ�]��ݒ�
            targetRotation = Quaternion.Euler(0, 0, groundAngle - 90);
        }
        else if (isInLoop && loopHit.collider == null)
        {
            // ���[�v���痣�ꂽ�ꍇ�A�p�x�����ɖ߂�
            isInLoop = false;
            targetRotation = Quaternion.identity; // �f�t�H���g�̉�]�ɖ߂�
        }
    }

    private void ApplyRotation()
    {
        // �p�x�̓K�p�i�X���[�Y�ɉ�]����ꍇ�͕�ԁj
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class over : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI yes;
    [SerializeField]
    TextMeshProUGUI no;
    [SerializeField]
    AudioClip se_sel;
    [SerializeField]
    AudioClip se_start;
    [SerializeField]
    AudioClip se_end;
    AudioSource snd;
    int state = 0;
    [SerializeField] GameObject trg, targetyes, targetno;

    public float up = 0.5f; // ��
    public float down = -0.5f; // ��
    // PS4�R���g���[���[�̍����o�[�̐�����
    public string verticalAxis = "Vertical";
    bool isdis;
    private Fademane fademane;

    void Start()
    {
        state = 1;
        yes.color = new Color(0.9528302f, 0.03056241f, 0.03056241f, 1f);
        no.color = new Color(255f, 255f, 255f, 1f);
        isdis = false;
        snd = gameObject.AddComponent<AudioSource>();

        fademane = FindObjectOfType<Fademane>();//fademane�̎擾
        if (fademane == null)
        {
            Debug.LogError("Fademane instance not found in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ���o�[����
        float verticalInput = Input.GetAxis(verticalAxis);

        if (verticalInput > up && !isdis) // ��L�[
        {
            yes.color = new Color(0.9528302f, 0.03056241f, 0.03056241f, 1f);
            no.color = new Color(255f, 255f, 255f, 1f);
            trg.transform.position = targetyes.transform.position; // trg��targetyes�̈ʒu�Ɉړ�
            if (state != 1)
            {
                snd.PlayOneShot(se_sel);
                state = 1;
            }
        }
        if (verticalInput < down && !isdis) // ���L�[
        {
            no.color = new Color(0.9528302f, 0.03056241f, 0.03056241f, 1f);
            yes.color = new Color(255f, 255f, 255f, 1f);
            trg.transform.position = targetno.transform.position; // trg��targetyes�̈ʒu�Ɉړ�
            if (state != 2)
            {
                snd.PlayOneShot(se_sel);
                state = 2;
            }
        }
        if (Input.GetKeyDown(KeyCode.Z) && !isdis)
        {
            if (state == 1)
            {
                StartCoroutine(str()); // �ҋ@��
                isdis = true;
            }
            else if (state == 2)
            {
                StartCoroutine(end()); // �ҋ@��
                isdis = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Application.Quit();
        }
    }

    IEnumerator str() // se����I���܂őҋ@
    {
        // ���y��炷
        snd.PlayOneShot(se_start);
        Debug.Log("Playing se_start");

        // �I���܂őҋ@
        yield return new WaitWhile(() => snd.isPlaying);
        Debug.Log("se_start finished");
        // �O�̃V�[���ɖ߂�
        GameManager.Instance.LoadPreviousScene();
    }

    IEnumerator end() // se����I���܂őҋ@
    {
        // ���y��炷
        snd.PlayOneShot(se_end);
        Debug.Log("Playing se_end");

        // �I���܂őҋ@
        yield return new WaitWhile(() => snd.isPlaying);
        Debug.Log("se_end finished");
        fademane.ChangeSceneWithFade(1f, 0.5f, "title"); // �t�F�[�h���Ԃ͓K�X����
    }
}

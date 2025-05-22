using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class titlescript : MonoBehaviour
{
    [SerializeField] AudioClip SE_dec;
    AudioSource snd;
    [SerializeField] TextMeshProUGUI textToBlink; // 点滅させるTextMeshProUGUIコンポーネント
    public float blinkSpeed = 1.0f; // 点滅の速度

    private float timer;
    private bool isBlinking = true;
    private bool canProceed = true; // ボタン連打防止用フラグ
    public float cooldownTime = 10.0f; // クールダウン時間（秒）
    private Fademane fademane;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        snd = GetComponent<AudioSource>();
        if (snd == null)
        {
            snd = gameObject.AddComponent<AudioSource>();
        }

        if (SE_dec == null)
        {
            Debug.LogError("AudioClip SE_dec is not assigned in the inspector.");
        }

        fademane = FindObjectOfType<Fademane>();
        if (fademane == null)
        {
            Debug.LogError("Fademane instance not found in the scene.");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject textObject = GameObject.Find("BlinkingText");
        if (textObject != null)
        {
            textToBlink = textObject.GetComponent<TextMeshProUGUI>();
            isBlinking = true;
        }
        else
        {
            Debug.LogError("BlinkingText object not found in the scene.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Application.Quit();
        }

        if ((Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.Z)) && canProceed)
        {
            StartCoroutine(StartTransition());
            StopBlinking();
            StartCoroutine(Cooldown());
        }

        if (isBlinking && textToBlink != null)
        {
            timer += Time.deltaTime * blinkSpeed;
            Color color = textToBlink.color;
            color.a = Mathf.PingPong(timer, 1.0f);
            textToBlink.color = color;
        }
    }

    public void StopBlinking()
    {
        isBlinking = false;
        if (textToBlink != null)
        {
            Color color = textToBlink.color;
            color.a = 1.0f;
            textToBlink.color = color;
        }
    }

    IEnumerator StartTransition()
    {
        if (SE_dec != null)
        {
            snd.PlayOneShot(SE_dec);
            yield return new WaitWhile(() => snd.isPlaying);
            fademane.ChangeSceneWithFade(1f, 0.5f, "Stage1");
        }
        else
        {
            Debug.LogError("AudioClip SE_dec is not assigned.");
            SceneManager.LoadScene("Stage1");
        }
    }

    IEnumerator Cooldown()
    {
        canProceed = false;
        yield return new WaitForSeconds(cooldownTime);
        canProceed = true;
    }
}

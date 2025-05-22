using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fademane : MonoBehaviour
{
    private CanvasGroup fadeCanvas;

    private static float expectedfadetime;
    //シーン遷移にかかると予想される時間
    public static float expectedFadetime
    {
        get { return expectedfadetime; }
    }

    // Use this for initialization
    void Awake()
    {
        var prefab = (GameObject)Resources.Load("FadeCanvas");

        if (prefab == null)
        {
            Debug.LogError("FadeCanvasがResourcesフォルダ内に見つかりませんでした。");
            return;
        }

        var instance = Instantiate(prefab);

        fadeCanvas = instance.GetComponent<CanvasGroup>();
        if (fadeCanvas == null)
        {
            Debug.LogError("CanvasGroupがFadeCanvasに見つかりませんでした。");
            return;
        }

        // フェード画像のアルファ値を0に設定（最初は透明にする）
        fadeCanvas.alpha = 0f;


        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    private Sequence sequence;

    public void ChangeSceneWithFade(float fadeiotime, float fadewaittime, string nextscenename)
    {
        var operation = SceneManager.LoadSceneAsync(nextscenename); //次シーンの読み込み開始
        operation.allowSceneActivation = false;//シーンの変更をブロック

        expectedfadetime = fadeiotime + fadewaittime;

        sequence = DOTween.Sequence();

        sequence
                //.OnStart(InputManager.Instance.StopInput)      //入力停止
                .Append(fadeCanvas.DOFade(1f, fadeiotime))     //フェードアウト
                .AppendCallback(() =>
                {
                    sequence.Pause();                          //シーンが変わるまで停止
                    operation.allowSceneActivation = true;     //シーンの変更を許可
                })
                .AppendInterval(fadewaittime)                  //暗転中に停止
                .Append(fadeCanvas.DOFade(0f, fadeiotime));     //フェードイン
                                                                //.OnComplete(InputManager.Instance.StartInput); //入力再開

        sequence.Play();
    }

    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        sequence.Play();
    }
}

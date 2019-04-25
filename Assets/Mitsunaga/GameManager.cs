using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;

public class GameManager : SingletonMBGameManager<GameManager>
{
    // GameManager
    // シーン遷移やフラグ管理など、シーンをまたぐ情報の管理を行う
    // どのシーンにも存在できるように、特定のオブジェクト、スクリプトがないと動かないという状況は避ける

    [SerializeField, Header("デバッグ用")]
    bool isDebug = false;
    [SerializeField]
    public Text bigText;

    [SerializeField, Header("シーン遷移用")]
    FadeSystem fadeSystem;
    [SerializeField]
    float fadeTime;

    [Header("ここから下、確認用")]
    // プレイヤーの情報
    public Vector3      playerPosition;     // プレイヤーのポジション
    public Vector3      cameraPosition;     // カメラのポジション
    public Vector3      cursorPos;          // カーソルの位置
    public bool         cursorFlg;          // カーソルのフラグ(0ならブラックホール、1ならホワイトホール)
    // フラグ管理
    public BoolReactiveProperty isClear     = new BoolReactiveProperty(false);      // クリア
    public BoolReactiveProperty isGameOver  = new BoolReactiveProperty(false);      // ゲームオーバー
    public BoolReactiveProperty isPause     = new BoolReactiveProperty(true);       // 一時停止

    override protected void Awake()
    {
        // 親クラスのAwakeをはじめに呼び出す
        base.Awake();

        // シーンが変わっても破棄されないようにする
        DontDestroyOnLoad(this.gameObject);

        // デバッグ中以外なら読み込み後タイトルシーンに遷移
        if (!isDebug)
        {
            FadeOut("01 Title");
        }
        else
        {
            FadeIn();
        }
    }

    // シーン遷移
    // フェードアウト後にシーンを切り替え、フェードインする
    // それぞれフラグを元に戻しておく
    void FadeOut(string SceneName)
    {
        isPause.Value = true;
        IObservable<bool> obsOut = Observable.FromCoroutine<bool>(observer => fadeSystem.FadeOutCoroutine(observer, fadeTime));
        obsOut.Subscribe(onCompleted =>
        {
            SceneManager.LoadScene(SceneName);
            FadeIn();
        })
        .AddTo(this.gameObject);
    }
    void FadeIn()
    {
        isGameOver.Value    = false;
        isClear.Value       = false;

        StartCoroutine(fadeSystem.FadeInCoroutine(fadeTime));
    }
}

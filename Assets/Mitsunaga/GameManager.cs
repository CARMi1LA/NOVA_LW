using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

public class GameManager : SingletonMBGameManager<GameManager>
{
    // GameManager
    // シーン遷移やフラグ管理など、シーンをまたぐ情報の管理を行う
    // どのシーンにも存在できるように、特定のオブジェクト、スクリプトがないと動かないという状況は避ける

    [SerializeField, Header("シーン遷移の速さ")]
    float fadeTime;

    [Header("ここから下、確認用")]

    // ゲームで必要な情報
    public float        fieldRange = 250.0f;            // フィールドの大きさ
    public Transform    playerTransform;                // プレイヤーのトランスフォーム
    public Transform    bossTransform;                  // ボスのトランスフォーム
    public Vector3      cameraPosition = Vector3.zero;  // カメラのポジション
    public Vector3      cursorPos = Vector3.zero;       // カーソルの位置
    public bool         cursorFlg = true;               // カーソルのフラグ(0ならブラックホール、1ならホワイトホール)
    public int          playerLevel = 1;                // プレイヤーのレベル(成長によってレベルアップ)

    // フラグ管理
    public BoolReactiveProperty isClear     = new BoolReactiveProperty(false);      // クリア
    public BoolReactiveProperty isGameOver  = new BoolReactiveProperty(false);      // ゲームオーバー
    public BoolReactiveProperty isPause     = new BoolReactiveProperty(true);       // 一時停止
    public BoolReactiveProperty isCoreMode  = new BoolReactiveProperty(false);      // コアモード
    
    // シーン遷移までの待ち時間
    const float WAITTIME_CLEAR      = 15.0f;
    const float WAITTIME_GAMEOVER   = 2.0f;
    const float WAITTIME_COREMODE   = 1.0f;
    // フィールドの大きさ
    const float FIELDRANGE_NORMAL   = 250.0f;
    const float FIELDRANGE_CORE     = 150.0f;

    // GetComponent保存用
    FadeSystem fadeSystem;
    Rigidbody playerRigidbody;

    protected override void Awake()
    {
        // 親クラスのAwakeをはじめに呼び出す
        base.Awake();

        // シーンが変わっても破棄されないようにする
        DontDestroyOnLoad(this.gameObject);

        // 各情報を取得
        fadeSystem = this.GetComponent<FadeSystem>();

        Debug.Log("Init Completed");
        // クリア時の処理
        isClear
            .Where(x => x)
            .Subscribe(_ =>
            {
                // 停止
                isPause.Value = true;
                playerRigidbody = playerTransform.gameObject.GetComponent<Rigidbody>();
                playerRigidbody.isKinematic = true;
                Debug.Log(playerRigidbody.isKinematic.ToString());
                // 一定時間待ち、タイトルに戻る
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_CLEAR))
                .Subscribe(c =>
                {
                    FadeOut("01 Title");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);

        Debug.Log("isClear Completed");

        // ゲームオーバー時の処理
        isGameOver
            .Where(x => x)
            .Where(x => !isClear.Value)
            .Subscribe(_ =>
            {
                // 停止
                isPause.Value = true;
                playerRigidbody = playerTransform.gameObject.GetComponent<Rigidbody>();
                playerRigidbody.isKinematic = true;

                // 一定時間待ち、ステージセレクトに戻る
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_GAMEOVER))
                .Subscribe(c =>
                {
                    FadeOut("02 StageSelect");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(gameObject);

        Debug.Log("isGameOver Completed");

        // コアモード開始時の処理
        isCoreMode
            .Where(x => x)
            .Subscribe(_ =>
            {
                // 停止
                isPause.Value = true;
                playerRigidbody = playerTransform.gameObject.GetComponent<Rigidbody>();
                playerRigidbody.isKinematic = true;

                // 一定時間待ち、コアモードを開始する
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_COREMODE))
                .Subscribe(c =>
                {
                    FadeOut("04 CoreMode");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);

        Debug.Log("isCoreMode Completed");

        // クリックでポーズを解除する
        // 各フラグが一致する場合のみ実行
        this.UpdateAsObservable()
            .Where(x => Input.GetMouseButtonDown(0))
            .Where(x => isPause.Value)
            .Where(x => !isGameOver.Value)
            .Where(x => !isClear.Value)
            .Subscribe(_ =>
            {
                GameManager.Instance.isPause.Value = false;
            })
            .AddTo(this.gameObject);

        Debug.Log("isPause Completed");

        // シーンを開始
        FadeIn("GameStart");
    }

    // シーン遷移
    // フェードアウト後にシーンを切り替え、フェードインする
    public void FadeOut(string SceneName)
    {
        isPause.Value = true;
        IObservable<bool> obsOut = Observable.FromCoroutine<bool>(observer => fadeSystem.FadeOutCoroutine(observer, fadeTime));
        obsOut.Subscribe(_ =>
        {
            SceneManager.LoadScene(SceneName);
            FadeIn(SceneName);
        })
        .AddTo(this.gameObject);
    }
    void FadeIn(string SceneName)
    {
        Debug.Log("Start : " + SceneName);

        isGameOver.Value    = false;
        isClear.Value       = false;

        if (SceneName == "04 CoreMode")
        {
            fieldRange = FIELDRANGE_CORE;
        }
        else
        {
            isCoreMode.Value = false;
            fieldRange = FIELDRANGE_NORMAL;
        }

        StartCoroutine(fadeSystem.FadeInCoroutine(fadeTime));
    }
}
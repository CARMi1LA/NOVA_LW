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

    [SerializeField, Header("デバッグ用フラグ")]
    bool isDebug = false;
    [SerializeField,Header("各メッセージ表示用のでかいテキスト")]
    public Text bigText;

    FadeSystem fadeSystem;
    [SerializeField, Header("シーン遷移の速さ")]
    float fadeTime;

    [Header("ここから下、確認用")]
    // プレイヤーの情報
    public Transform    playerTransform;    // プレイヤーのトランスフォーム
    public Transform    bossTransform;      // ボスのトランスフォーム
    public Vector3      cameraPosition;     // カメラのポジション
    public Vector3      cursorPos;          // カーソルの位置
    public bool         cursorFlg;          // カーソルのフラグ(0ならブラックホール、1ならホワイトホール)

    public int          playerLevel = 1;    // プレイヤーのレベル(成長によってレベルアップ)
    // フラグ管理
    public BoolReactiveProperty isClear     = new BoolReactiveProperty(false);      // クリア
    public BoolReactiveProperty isGameOver  = new BoolReactiveProperty(false);      // ゲームオーバー
    public BoolReactiveProperty isPause     = new BoolReactiveProperty(true);       // 一時停止
    public BoolReactiveProperty isCoreMode  = new BoolReactiveProperty(false);      // コアモード
    
    // シーン遷移までの待ち時間
    const float WAITTIME_CLEAR = 4.0f;
    const float WAITTIME_GAMEOVER = 2.0f;
    const float WAITTIME_COREMODE = 1.0f;

    override protected void Awake()
    {
        // 親クラスのAwakeをはじめに呼び出す
        base.Awake();

        // シーンが変わっても破棄されないようにする
        DontDestroyOnLoad(this.gameObject);

        // 各情報を取得
        fadeSystem = this.GetComponent<FadeSystem>();

        // クリア時の処理
        isClear
            .Where(x => x)
            .Subscribe(_ =>
            {
                isPause.Value = true;

                Debug.Log("IsClear : " + _.ToString());
                bigText.text = "CLEAR!";

                // 一定時間待ち、タイトルに戻る
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_CLEAR))
                .Subscribe(c =>
                {
                    FadeOut("01 Title");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);

        // ゲームオーバー時の処理
        isGameOver
            .Where(x => x)
            .Subscribe(_ =>
            {
                isPause.Value = true;

                Debug.Log("IsGameOver : " + _.ToString());
                bigText.text = "GAME OVER";

                // 一定時間待ち、ステージセレクトに戻る
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_GAMEOVER))
                .Subscribe(c =>
                {
                    FadeOut("02 StageSelect");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(gameObject);

        // コアモード開始時の処理
        isCoreMode
            .Where(x => x)
            .Subscribe(_ =>
            {
                isPause.Value = true;

                // 一定時間待ち、コアモードを開始する
                Observable.Timer(TimeSpan.FromSeconds(WAITTIME_COREMODE))
                .Subscribe(c =>
                {
                    FadeOut("04 CoreMode");
                })
                .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);

        // デバッグ中以外なら読み込み後タイトルシーンに遷移
        if (!isDebug)
        {
            FadeOut("01 Title");
        }
        else
        {
            FadeIn("DebugMode");
        }
    }

    // シーン遷移
    // フェードアウト後にシーンを切り替え、フェードインする
    // それぞれフラグを元に戻しておく
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

        if (SceneName == "03 Stage01")
        {
            bigText.text = "Click Start";
            isCoreMode.Value = false;
        }
        else if(SceneName == "04 CoreMode")
        {
            bigText.text = "Destroy Core";
        }
        else
        {
            bigText.text = "";
            isCoreMode.Value = false;
        }

        StartCoroutine(fadeSystem.FadeInCoroutine(fadeTime));
    }
}

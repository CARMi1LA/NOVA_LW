using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UniRx.Toolkit;
using Cinemachine;

using UnityEngine.VFX;
//using UnityEditor.VFX;
//using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Experimental.VFX;

public class PlayerSystem : _StarParam
{
    // プレイヤーとカメラのコントロールを行う

    [SerializeField, Header("メインシーンか否か")]
    bool isMainScene = true;
    [SerializeField, Header("ボスとのライン関連")]
    LineRenderer linePtB;

    // 星の移動関連 移動は敵と統合して _StarParam に移す予定
    [SerializeField, Header("星の加速度、加速度への追従度、軌跡パーティクル")]
    float moveSpeed = 10.0f;
    [SerializeField]
    float moveSpeedMul = 2.0f;
    [SerializeField]
    VisualEffect VFXPath;

    // 衝突関連
    [SerializeField, Header("星の衝突、合体時の待ち時間、パーティクル")]
    float hitStopTime = 0.2f;
    [SerializeField]
    float waitCount = 4.5f;
    [SerializeField]
    GameObject[] hitPS;

    // カメラ関連 これもいずれ独立させる
    [SerializeField, Header("シネマシーンのカメラ")]
    CinemachineVirtualCamera vCam;
    const float CDISTANCE = 100.0f; // カメラの引き
    const float CDISTANCE_SELECTSCENE = 50.0f;

    // 音楽関連
    AudioSource collisionAudioSource;

    new void Awake()
    {
        // _StarParamのAwakeを最初に起動
        base.Awake();
        // SEを取得
        collisionAudioSource = GetComponent<AudioSource>();
        // カメラの初期化
        SetCamera();
    }

    void Start()
    {
        if (isMainScene)
        {
            // ラインレンダラーの情報を指定
            linePtB.startWidth = 0.1f;  // 開始点の幅
            linePtB.endWidth = 0.1f;    // 終点の幅
            linePtB.positionCount = 2;  // 頂点の数
        }

        // プレイヤー情報をGameManagerに送信
        GameManager.Instance.playerTransform = this.transform;
        GameManager.Instance.cameraPosition = vCam.gameObject.transform.position; // カメラ

        // アップデート
        // isPauseがfalseの場合のみ実行
        this.UpdateAsObservable()
            .Where(c => !GameManager.Instance.isPause.Value)
            .Subscribe(c =>
            {
                // 移動処理
                // 移動速度と追従度を渡す
                SetStarMove(moveSpeed, moveSpeedMul);
                VFXPath.SetVector3("PlayerPosition", this.transform.position);
                VFXPath.SetFloat("PlayerSize", (GetStarSize() + 1.0f) / 2);

                // プレイヤー情報をGameManagerに送信
                GameManager.Instance.playerTransform = this.transform;
                GameManager.Instance.cameraPosition = vCam.gameObject.transform.position; // カメラ

                if (isMainScene)
                {
                    // ボスとの間に線を引く
                    linePtB.SetPosition(0, transform.position);     // 開始点の座標
                    linePtB.SetPosition(1, GameManager.Instance.bossTransform.position); // 終点の座標
                }
            })
            .AddTo(this.gameObject);

        // クリックでポーズを解除する
        // 各フラグが一致する場合のみ実行
        this.UpdateAsObservable()
            .Where(c => Input.GetMouseButtonDown(0))
            .Where(c => GameManager.Instance.isPause.Value)
            .Where(c => !GameManager.Instance.isGameOver.Value)
            .Where(c => !GameManager.Instance.isClear.Value)
            .Subscribe(_ =>
            {
                GameManager.Instance.bigText.text = "";
                GameManager.Instance.isPause.Value = false;
            })
            .AddTo(this.gameObject);

        // 当たり判定
        // 衝突したオブジェクトの_StarParamをtry,catchを用いて強引に取得する
        this.OnCollisionEnterAsObservable()
            .Subscribe(c =>
            {
                float enemySize = -1.0f;

                try
                {
                    enemySize = c.gameObject.GetComponent<_StarParam>().GetStarSize();

                    collisionAudioSource.Play();    // 衝突の音を出す

                    // 当たった星のサイズが
                    if (enemySize <= (GetStarSize() / 4))
                    {
                        // 1. 自分よりも圧倒的に小さければそのまま吸収

                        // ボスを倒すとゲームクリア
                        if (c.gameObject.GetComponent<_StarParam>().starID == 2)
                        {
                            GameManager.Instance.isClear.Value = true;
                        }
                        else
                        {
                            // 小さい星では成長しない
                            c.gameObject.SetActive(false);
                        }
                    }
                    else if (enemySize <= (GetStarSize() * 1.1f))
                    {
                        // 2. 自分と同じくらいならばお互いを破壊して合体

                        // パーティクル再生
                        foreach (GameObject ps in hitPS)
                        {
                            Instantiate(ps).transform.position = transform.position;
                        }

                        // ボスを倒すとゲームクリア
                        if (c.gameObject.GetComponent<_StarParam>().starID == 2)
                        {
                            GameManager.Instance.isClear.Value = true;
                        }
                        else
                        {
                            // ぶつかったら、砕けて待ち時間のカウントを進める
                            StartCoroutine(WaitCoroutine(waitCount, c.transform.localScale.x / 2));
                        }

                        // 相手のオブジェクトを非表示にする
                        c.gameObject.SetActive(false);
                    }
                    else
                    {
                        // 3. 自分より大きければ自分が破壊される　ゲームオーバー

                        GameManager.Instance.isGameOver.Value = true;
                        this.gameObject.SetActive(false);
                    }
                }
                catch
                {
                    // コンポーネントを持っていない場合例外が発生するためデバッグログで流す
                    Debug.Log("_StarParam is Null");
                }
            })
            .AddTo(this.gameObject);
    }

    // カメラの処理
    void SetCamera()
    {
        if (isMainScene)
        {
            // カメラ初期位置と星の半径を足した距離分、カメラを離す
            vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance
                = CDISTANCE + (transform.localScale.x / 1.5f);
        }
        else
        {
            // カメラ初期位置と星の半径を足した距離分、カメラを離す
            vCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance
                = CDISTANCE_SELECTSCENE + (transform.localScale.x / 1.5f);
        }
    }

    // 衝突後の待ち時間、星の再構成を管理するコルーチン
    // waitCount：待ち時間(単位：秒)
    // nextSize ：設定する星のサイズ
    IEnumerator WaitCoroutine(float waitCount, float nextSize)
    {

        float count = 0.0f;                     // 待ち時間を計測する変数
        float size = transform.localScale.x;    // プレイヤーのサイズを保存する

        // ヒットストップを最初に起動
        StartCoroutine(HitStopCoroutine(hitStopTime));

        starRig.isKinematic = true; // プレイヤーを移動不能に
        SetStarSize(0.0f);          // 星のサイズを0に

        // 待ち時間のカウント
        while (count < waitCount - 0.5f)
        {
            count += Time.deltaTime;
            yield return null;
        }

        // 待ち時間が終わる0.5秒前に、星のサイズを適用
        SetStarSize(size + nextSize);

        // 待ち時間のカウント
        while (count < waitCount)
        {
            count += Time.deltaTime;
            yield return null;
        }

        starRig.isKinematic = false;    // プレイヤーを移動可能に
        SetCamera();                    // カメラをセットする
    }

    // 衝突時のヒットストップを管理するコルーチン
    // stopTime：待ち時間(単位：秒)
    IEnumerator HitStopCoroutine(float stopTime)
    {
        float count = 0.0f;

        // Time.TimeScale … 時間の進む速さを変更する(通常 1.0f)
        Time.timeScale = 0.1f;

        // 待ち時間のカウント
        while (count < stopTime)
        {
            // Time.unscaledDeltaTime … タイムスケールの影響を受けないDeltaTime
            count += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1.0f;
    }
}

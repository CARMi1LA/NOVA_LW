using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UniRx.Toolkit;
using Cinemachine;

using UnityEngine.Experimental.VFX;

public class PlayerSystem : _StarParam
{
    // プレイヤーとカメラのコントロールを行う

    [SerializeField, Header("メインシーンか否か")]
    bool isMainScene = true;

    // 星の移動関連 移動は敵と統合して _StarParam に移す予定
    [SerializeField, Header("星の加速度、加速度への追従度、軌跡パーティクル")]
    float moveSpeed = 15.0f;
    float startSpeed;
    [SerializeField]
    float moveSpeedMul = 2.0f;
    [SerializeField]
    VisualEffect VFXPath;

    // 衝突関連
    [SerializeField, Header("星の衝突、合体時の待ち時間、パーティクル")]
    float hitStopTime = 0.2f;
    [SerializeField]
    public float waitCount = 3.0f;

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

        // プレイヤー情報をGameManagerに送信
        GameManager.Instance.playerTransform = this.transform;
        GameManager.Instance.cameraPosition = vCam.gameObject.transform.position; // カメラ
    }

    void Start()
    {
        startSpeed = moveSpeed;
        // プレイヤー情報をGameManagerに送信
        GameManager.Instance.playerTransform = this.transform;
        GameManager.Instance.cameraPosition = vCam.gameObject.transform.position; // カメラ

        // アップデート
        // isPauseがfalseの場合のみ実行
        this.UpdateAsObservable()
            .Where(c => !GameManager.Instance.isPause.Value)
            .Subscribe(c =>
            {
                // プレイヤー情報をGameManagerに送信
                GameManager.Instance.playerTransform = this.transform;                          // トランスフォーム
                GameManager.Instance.cameraPosition = vCam.gameObject.transform.position;       // カメラ
                GameManager.Instance.playerLevel = Mathf.Clamp((int)GetStarSize() / 10, 1, 5);  // レベル

                // レベルに応じて速度上昇
                moveSpeed = startSpeed * (1 + GameManager.Instance.playerLevel * 0.1f);

                // 移動処理
                SetStarMove(moveSpeed, moveSpeedMul);
                // 軌道エフェクト
                VFXPath.SetVector3("PlayerPosition", this.transform.position);
                VFXPath.SetFloat("PlayerSize", (GetStarSize() + 1.0f) / 2);

                // フィールド外に出た場合、ゲームオーバー
                if (GameManager.Instance.isCoreMode.Value &&
                    Vector3.Distance(this.transform.position, GameManager.Instance.bossTransform.position) > GameManager.Instance.fieldRange)
                {
                    playDeathFX.OnNext(waitCount);
                    GameManager.Instance.isGameOver.Value = true;
                }
            })
            .AddTo(this.gameObject);

        // 当たり判定
        // 衝突したオブジェクトの_StarParamをtry,catchを用いて強引に取得する
        this.OnCollisionEnterAsObservable()
            .Subscribe(c =>
            {
                _StarParam enemyParam;         // 衝突したオブジェクトの情報を取得する
                collisionAudioSource.Play();    // 衝突の音を出す

                // モードごとの処理
                if (GameManager.Instance.isCoreMode.Value)
                {
                    // コアモード
                    try
                    {
                        // 衝突したオブジェクトの情報を取得する
                        enemyParam = c.gameObject.GetComponent<_StarParam>();
                        Debug.Log("collision Boss!!"  + enemyParam.starID.ToString());

                        // ボスと衝突した場合、ゲームクリア
                        if (enemyParam != null)
                        {
                            GameManager.Instance.isClear.Value = true;
                            playCollisionFX.OnNext(waitCount);
                            enemyParam.playDeathFX.OnNext(0.5f);
                        }
                    }
                    catch
                    {
                        // ボス以外のオブジェクトと衝突した場合、ゲームオーバー
                        try
                        {
                            FragmentSystem fragParam = c.gameObject.GetComponent<FragmentSystem>();

                            if(fragParam != null)
                            {
                                playDeathFX.OnNext(waitCount);
                                GameManager.Instance.isGameOver.Value = true;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    // 通常モード
                    try
                    {
                        enemyParam = c.gameObject.GetComponent<_StarParam>();

                        // 当たった星のサイズを比べる
                        if (enemyParam.GetStarSize() <= (GetStarSize() * 1.1f))
                        {
                            // 2. 自分より小さければお互いを破壊し再構成　成長

                            if (enemyParam.starID == 2)
                            {
                                // ボスを破壊したらコアモードへ
                                GameManager.Instance.isCoreMode.Value = true;
                            }
                            else
                            {
                                // 砕けて待ち、成長
                                StartCoroutine(WaitCoroutine(waitCount, c.transform.localScale.x * 0.5f));
                            }

                            playCollisionFX.OnNext(waitCount);
                            enemyParam.playDeathFX.OnNext(0.5f);
                        }
                        else
                        {
                            // 3. 自分より大きければ自分が破壊される　ゲームオーバー

                            playDeathFX.OnNext(waitCount);
                            GameManager.Instance.isGameOver.Value = true;
                        }
                    }
                    catch
                    {
                        // コンポーネントを持っていない場合例外が発生するためデバッグログで流す
                        Debug.Log("Collision Error.");
                    }
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
                = CDISTANCE + (transform.localScale.x * 2.5f);
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

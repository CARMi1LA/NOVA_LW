using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;
using UniRx.Triggers;

public class EnemySystem : _StarParam
{
    // パラメータ
    //[SerializeField] PlanetSpawner spawner; // 惑星が持つスクリプト 30秒経つと消える

    [SerializeField,Header("AI番号")]
    int AInum;

    // 移動のパラメータ
    // AI
    Vector3 moveDir = Vector3.zero;                             // 移動方向
    float moveSpeed;                                            // 移動速度
    float moveSpeedMul = 1.0f;                                  // 移動速度への追従度
    float moveSpace = 150.0f;                                   // 移動可能距離(プレイヤーからの距離)

    bool isLookPlayer = false;                                  // プレイヤーを追従するか否か

    // マウスカーソルに対する加速関連
    float cursorSpeed = 20.0f;
    float cursorSpeedMul = 1.0f;
    float cursorSpace = 30.0f;

    // 死亡時と衝突時の待ち時間
    public const float DEATH_COUNT = 0.5f;
    public const float COLLISION_COUNT = 1.0f;

    Collider enemyCol;

    public Subject<bool> coreEscape = new Subject<bool>();

    public Subject<bool> EnemyAISubject = new Subject<bool>();

    new void Awake()
    {
        // _StarParamのAwakeを最初に起動
        base.Awake();

        enemyCol = GetComponent<Collider>();
        enemyCol.isTrigger = false;

        // ボス星の場合は、GameManagerに情報を送る
        if (starID == 2)
        {
            GameManager.Instance.bossTransform = this.transform;
        }
    }
    void Start()
    {
        // 星をアクティブにする
        this.gameObject.SetActive(true);

        // 移動速度、AIナンバーをランダムに取得する
        moveSpeed = UnityEngine.Random.Range(5.0f, 15.0f);
        AInum = UnityEngine.Random.Range(0, 3);

        // 簡単なAIの挙動(プレイヤーの方向を向くか、ランダムな方向を向くか)
        switch (AInum)
        {
            case 0: // ランダム方向にまっすぐ進む
                moveDir.x = UnityEngine.Random.Range(-1.0f, 1.0f);
                moveDir.z = UnityEngine.Random.Range(-1.0f, 1.0f);
                break;
            case 1: // プレイヤーを追いかける
                moveSpace *= 0.5f;
                isLookPlayer = true;
                break;
            case 2: // プレイヤーから逃げる
                moveSpace *= 0.5f;
                isLookPlayer = true;
                moveSpeed = -moveSpeed;
                break;
            default:
                // ランダム方向にまっすぐ進む
                moveDir.x = UnityEngine.Random.Range(-1.0f, 1.0f);
                moveDir.z = UnityEngine.Random.Range(-1.0f, 1.0f);
                break;
        }

        this.UpdateAsObservable()
            .Where(_ => starID != 2)
            .Where(_ => !GameManager.Instance.isPause.Value)
            .Subscribe(c =>
            {
                if (Vector3.Distance(this.transform.position, GameManager.Instance.playerTransform.position) <= moveSpace)
                {
                    isMoving.Value = true;

                    // プレイヤーの方向を向くAIか否か
                    if (isLookPlayer)
                    {
                        moveDir = (GameManager.Instance.playerTransform.position - this.transform.position).normalized;
                    }
                    // 速度と方向を計算して、力を加える
                    starRig.AddForce(moveSpeedMul * ((moveDir * moveSpeed) - starRig.velocity));
                }
            })
            .AddTo(this.gameObject);

        // コアモードのボスの挙動
        this.UpdateAsObservable()
            .Where(_ => starID == 2)
            .Where(_ => !GameManager.Instance.isPause.Value)
            .Where(_ => GameManager.Instance.isCoreMode.Value)
            .Subscribe(_ =>
            {
                moveSpeed = 15.0f;
                moveSpeedMul = 3.0f;

                isMoving.Value = true;
                coreEscape.OnNext(true);

                starRig.AddForce(moveSpeedMul * ((moveDir * moveSpeed) - starRig.velocity));
            })
            .AddTo(this.gameObject);

        // 当たり判定
        this.OnCollisionEnterAsObservable()
            .Subscribe(_=>
            {
                // 自分よりも大きい星にぶつかった場合、消滅する
                if (transform.localScale.x < _.transform.localScale.x / 4)
                {
                    // 死亡時のエフェクト再生
                    this.playDeathFX.OnNext(DEATH_COUNT);
                }
                else
                {
                    // 衝突時のエフェクト再生
                    this.playCollisionFX.OnNext(COLLISION_COUNT);
                }
            })
            .AddTo(this.gameObject);
    }

    // AIで方向を取得する
    public void SetMoveDirection(Vector3 setDirection)
    {
        moveDir = setDirection;
    }

    // K A K I H A N A   Z O N E

    // 惑星スポーンの座標設定
    public void PlanetSpawn(Vector3 pos)
    {
        transform.position = pos;
    }

    // 惑星スポーンの設定（オーバーロード、スケール値追加）
    public void PlanetSpawn(Vector3 pos, float scale)
    {
        transform.position = pos;
        SetStarSize(scale);
    }

    // 消滅情報をスポーンクラスに送る
    public void Stop()
    {
        try
        {
            // PlanetSpawner.Instance.PlanetDestroy();
        }
        catch
        {

        }
    }
}

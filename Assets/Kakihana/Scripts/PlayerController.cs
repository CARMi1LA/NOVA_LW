using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class PlayerController : MonoBehaviour
{
    public IntReactiveProperty hp;  // プレイヤーのHP
    public float playerSpeed;       // プレイヤーの速さ
    public int playerShrinkTime;    // プレイヤーが最終サイズになるまでの時間
    public Vector3 movePos;         // 移動量
    public Vector3 startScale;      // 開始時の初期サイズ
    public Vector3 endScale = new Vector3(1.0f, 1.0f, 1.0f);    // 最終サイズ

    public int enemyDamage;         // 敵から受けるダメージ
    public int pulseDamage;         // パルスのダメージ

    public BoolReactiveProperty inPulse;

    public GameObject destroyEffect;// 消滅エフェクト
    // Start is called before the first frame update
    void Start()
    {
        // プレイヤー収縮用カウントの初期化
        float time = 0.0f;
        // プレイヤーの初期サイズの設定
        this.transform.localScale = startScale;

        // 矢印キーが押されたら移動量を設定する
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // プレイヤー収縮用の時間を計測
                time += Time.deltaTime;
                // パルスの最大収縮に合わせてプレイヤーのサイズも収縮
                this.transform.localScale = Vector3.Lerp(startScale, endScale, time / playerShrinkTime);
                // 右矢印キーが押されたら右に移動
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    movePos.x = 1.0f;
                }
                // 左矢印キーが押されたら左に移動
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    movePos.x = -1.0f;
                }
                // 上矢印キーが押されたら上に移動
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    movePos.z = 1.0f;
                }
                // 下矢印キーが押されたら下に移動
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    movePos.z = -1.0f;
                }

                // どのキーも押されていなければ移動しないようにする
                if (Input.GetKey(KeyCode.RightArrow) == false && Input.GetKey(KeyCode.LeftArrow) == false)
                {
                    movePos.x = 0.0f;
                }
                if (Input.GetKey(KeyCode.UpArrow) == false && Input.GetKey(KeyCode.DownArrow) == false)
                {
                    movePos.z = 0.0f;
                }
            }).AddTo(this.gameObject);

        // パルス外にいる場合、１秒毎にダメージを受ける
        this.UpdateAsObservable()
            .Where(_ => inPulse.Value == true)
            .Sample(TimeSpan.FromSeconds(1.0f))
            .Subscribe(_ => 
            {
                hp.Value -= 1;
            }).AddTo(this.gameObject);

        // HPが0になったらゲームオーバー
        hp.Where(_ => hp.Value <= 0)
            .Subscribe(_ => 
            {
                GameMaster.Instance.isGameOver.Value = true;
                Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
                gameObject.SetActive(false);
            }).AddTo(this.gameObject);

        // 移動処理
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                this.transform.position += movePos * playerSpeed * Time.deltaTime;
            }).AddTo(this.gameObject);

        // 当たり判定処理（敵）
        this.OnTriggerEnterAsObservable()
            .Where(c => c.gameObject.tag == "Enemy")
            .Subscribe(c => 
            {
                // 敵情報を取得
                EnemyController enemyInfo;
                enemyInfo = c.gameObject.GetComponent<EnemyController>();
                // 自分のサイズが敵のサイズより大きければスコア獲得
                if (this.transform.localScale.x >= c.transform.localScale.x)
                {
                    // スコアを加算
                    GameMaster.Instance.AddScore.OnNext(enemyInfo.score);
                    // 敵クラス側で消滅処理を実行
                    enemyInfo.EnemyDestroy();
                }
                // 敵のサイズの方が大きい場合はゲームオーバー
                else
                {
                    hp.Value -= 3;
                }
            }).AddTo(this.gameObject);

        // 当たり判定（ボス）
        this.OnTriggerEnterAsObservable()
        .Where(c => c.gameObject.tag == "Boss")
        .Subscribe(c =>
        {
            // ボスの情報を取得
            BossManager boss;
            boss = c.gameObject.GetComponent<BossManager>();
            // スコアを加算
            GameMaster.Instance.AddScore.OnNext(boss.score);
            // ボスクラス側で消滅処理を実行
            boss.EnemyDestroy();
        }).AddTo(this.gameObject);

        // 当たり判定（パルス内）
        this.OnTriggerEnterAsObservable()
        .Where(c => c.gameObject.tag == "Pulse")
        .Subscribe(c =>
        {
            inPulse.Value = false;
        }).AddTo(this.gameObject);

        // 当たり判定（パルス外）
        this.OnTriggerExitAsObservable()
            .Where(c => c.gameObject.tag == "Pulse")
            .Subscribe(c =>
            {
                // プレイヤーがエリア外であればダメージを受ける
                inPulse.Value = true;
            }).AddTo(this.gameObject);
    }
}

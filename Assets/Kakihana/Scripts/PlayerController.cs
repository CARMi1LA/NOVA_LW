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
        float time = 0.0f;
        this.transform.localScale = startScale;
        // 矢印キーが押されたら移動量を設定する
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                time += Time.deltaTime;
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

        inPulse
            .Where(_ => inPulse.Value == true)
            .Sample(TimeSpan.FromSeconds(1.0f))
            .Subscribe(_ => 
            {
                hp.Value -= 1;
            }).AddTo(this.gameObject);

        hp.Where(_ => hp.Value <= 0)
            .Subscribe(_ => 
            {
                GameMaster.Instance.isGameOver.Value = true;
            }).AddTo(this.gameObject);

        // 移動処理
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                this.transform.position += movePos * playerSpeed * Time.deltaTime;
            }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Where(c => c.gameObject.tag == "Enemy")
            .Subscribe(c => 
            {
                EnemyController enemyInfo;
                enemyInfo = c.gameObject.GetComponent<EnemyController>();
                if (this.transform.localScale.x >= c.transform.localScale.x)
                {
                    GameMaster.Instance.AddScore.OnNext(enemyInfo.score);
                    enemyInfo.EnemyDestroy();
                }
                else
                {
                    hp.Value -= 3;
                }
            }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
        .Where(c => c.gameObject.tag == "Boss")
        .Subscribe(c =>
        {
            BossManager boss;
            boss = c.gameObject.GetComponent<BossManager>();
            GameMaster.Instance.AddScore.OnNext(boss.score);
            boss.EnemyDestroy();
        }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
        .Where(c => c.gameObject.tag == "Pulse")
        .Subscribe(c =>
        {
            inPulse.Value = false;
        }).AddTo(this.gameObject);

        // プレイヤーがエリア外であればダメージを受ける
        this.OnTriggerExitAsObservable()
            .Where(c => c.gameObject.tag == "Pulse")
            .Subscribe(c =>
            {
                inPulse.Value = true;
            }).AddTo(this.gameObject);
    }
}

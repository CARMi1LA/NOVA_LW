using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class BossManager : MonoBehaviour
{
    public float bossSpeed;                 // 移動スピード
    public float maxDistance;               // アクティブ状態になるまでの距離
    public int score;                       // 獲得スコア

    public Vector3 moveForword;             // 移動方向
    public FloatReactiveProperty distance;  // プレイヤー間の距離

    public GameObject destroyEffect;        // 消滅エフェクト
    // Start is called before the first frame update
    void Start()
    {
        // ボスフラグがTrueになれば呼び出される
        GameMaster.Instance.bossFlg
            .Where(_ => GameMaster.Instance.bossFlg.Value == true)
            .Subscribe(_ => 
            {
                // このオブジェクトを表示する
                this.gameObject.SetActive(true);
                // 移動ベクトルの設定
                moveForword = (GameMaster.Instance.playerPos.position - this.transform.position).normalized;
                this.transform.forward = -moveForword;
            }).AddTo(this.gameObject);

        // プレイヤーとの距離が一定以上であれば動作
        this.UpdateAsObservable()
            .Where(_ => GameMaster.Instance.bossFlg.Value == true)
            .Where(_ => distance.Value <= Mathf.Pow(maxDistance,2))
            .Subscribe(_ => 
            {
                // ジグザグに移動する
                this.transform.eulerAngles = new Vector3(0, Mathf.Sin(Time.time), 0);
                this.transform.position += this.transform.forward * bossSpeed * Time.deltaTime;
            }).AddTo(this.gameObject);

        // 一定時間ごとにプレイヤーとの距離を取得する
            this.UpdateAsObservable()
            .Sample(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                distance.Value = (this.transform.position - GameMaster.Instance.playerPos.position).sqrMagnitude;
            }).AddTo(this.gameObject);
    }

    // 消滅メソッド
    public void EnemyDestroy()
    {
        // 消滅エフェクトを生成
        Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
        // マスタークラスにゲームクリア情報を送信
        GameMaster.Instance.isClear.Value = true;
        // 自身のオブジェクトを削除する
        Destroy(this.gameObject);
    }
}

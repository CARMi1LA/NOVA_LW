using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class PlayerCollision : MonoBehaviour
{
    // 衝突時、死亡時のエフェクト再生  プレイヤー

    [SerializeField]
    VisualEffect VFXCollision;

    void Start()
    {
        // 初期設定
        float VCSpawnRate = VFXCollision.GetFloat("SpawnRate");
        VFXCollision.gameObject.SetActive(false);
        _StarParam sp = this.GetComponent<_StarParam>();

        // 衝突時の処理
        sp.playCollisionFX
            .Subscribe(count =>
            {
                // 位置、サイズなどを指定して、VFXを起動する
                VFXCollision.gameObject.transform.position = this.transform.position;                   // 位置調整
                VFXCollision.SetFloat("SpawnRate", VCSpawnRate);                                        // 生成レートを戻す
                VFXCollision.SetFloat("PlayerSize", GameManager.Instance.playerTransform.localScale.x); // サイズ調整
                VFXCollision.SetFloat("LoopTime", count);                                               // 速度調整
                VFXCollision.gameObject.SetActive(false);                                               // 停止
                VFXCollision.gameObject.SetActive(true);                                                // 再起動

                // VFXを一定時間後に停止させる
                Observable.Timer(TimeSpan.FromSeconds(count))
                     .Subscribe(_ =>
                     {
                         VFXCollision.SetFloat("SpawnRate", 0);
                     })
                    .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);

        // 死亡時の処理
        sp.playDeathFX
            .Subscribe(count =>
            {
                // 死亡処理を実行
                // 最初にオブジェクトのコライダーを取得、当たり判定を消す
                Collider col = this.gameObject.GetComponent<Collider>();
                col.isTrigger = true;
                // 星を小さくする
                GetComponent<_StarParam>().SetStarSize(0.0f);

                // 位置、サイズなどを指定して、VFXを起動する
                VFXCollision.gameObject.transform.position = this.transform.position;                   // 位置調整
                VFXCollision.SetFloat("SpawnRate", VCSpawnRate);                                        // 生成レートを戻す
                VFXCollision.SetFloat("PlayerSize", GameManager.Instance.playerTransform.localScale.x); // サイズ調整
                VFXCollision.SetFloat("LoopTime", count);                                               // 速度調整
                VFXCollision.gameObject.SetActive(false);                                               // 停止
                VFXCollision.gameObject.SetActive(true);                                                // 再起動
            })
            .AddTo(this.gameObject);
    }
}

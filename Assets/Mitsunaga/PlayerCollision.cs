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

public class PlayerCollision : MonoBehaviour
{
    // 衝突時、死亡時のエフェクト再生  プレイヤー

    [SerializeField]
    VisualEffect VFXCollision;
    [SerializeField]
    ParticleSystem PSCollision;

    float VCSpawnRate;

    void Start()
    {
        VCSpawnRate = VFXCollision.GetFloat("SpawnRate");
        VFXCollision.gameObject.SetActive(false);

        _StarParam sp = this.GetComponent<_StarParam>();

        // 衝突時のエフェクト
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
                // PSを起動する
                //PSCollision.Play();

                // VFXを一定時間後に停止させる
                Observable.Timer(TimeSpan.FromSeconds(count))
                     .Subscribe(_ =>
                     {
                         VFXCollision.SetFloat("SpawnRate", 0);
                         // PSCollision.Stop();
                     })
                    .AddTo(this.gameObject);
            })
            .AddTo(this.gameObject);
        // 死亡時のエフェクト
        sp.playDeathFX
            .Subscribe(count =>
            {
                // 死亡処理を実行
                sp.StarDeath(count);

                // 位置、サイズなどを指定して、VFXを起動する
                VFXCollision.gameObject.transform.position = this.transform.position;                   // 位置調整
                VFXCollision.SetFloat("SpawnRate", VCSpawnRate);                                        // 生成レートを戻す
                VFXCollision.SetFloat("PlayerSize", GameManager.Instance.playerTransform.localScale.x); // サイズ調整
                VFXCollision.SetFloat("LoopTime", count);                                               // 速度調整
                VFXCollision.gameObject.SetActive(false);                                               // 停止
                VFXCollision.gameObject.SetActive(true);                                                // 再起動
                // PSを起動する
                //PSCollision.Play();

                // VFXを一定時間後に停止させる
                Invoke("VCStop", count);
            })
            .AddTo(this.gameObject);
    }
}

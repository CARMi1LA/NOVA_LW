using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class EnemyCollision : MonoBehaviour
{
    // 衝突時、死亡時のエフェクト再生

    [SerializeField,Header("VFXのプレハブ")]
    VisualEffect VFXCollision;

    void Start()
    {
        // 初期設定
        GameObject vc;
        _StarParam sp = this.GetComponent<_StarParam>();

        // 衝突時の処理
        sp.playCollisionFX
            .Subscribe(count =>
            {
                // VFXを作成
                vc = Instantiate(VFXCollision.gameObject);
                vc.gameObject.transform.position = this.transform.position;
                vc.GetComponent<VFXCollisionEnemy>().deathCount = count * 2.0f;
                vc.gameObject.SetActive(false);
                vc.gameObject.SetActive(true);
            })
            .AddTo(this.gameObject);

        // 死亡時の処理
        this.GetComponent<_StarParam>().playDeathFX
            .Subscribe(count =>
            {
                // 死亡処理を起動
                sp.StarDeath(count);

                // VFXを生成
                vc = Instantiate(VFXCollision.gameObject);
                vc.gameObject.transform.position = this.transform.position;
                vc.GetComponent<VFXCollisionEnemy>().deathCount = count * 2.0f;
                vc.gameObject.SetActive(false);
                vc.gameObject.SetActive(true);
            })
            .AddTo(this.gameObject);
    }
}

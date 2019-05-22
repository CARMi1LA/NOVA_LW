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

public class EnemyCollision : MonoBehaviour
{
    // 衝突時、死亡時のエフェクト再生

    [SerializeField,Header("VFXのプレハブ")]
    VisualEffect VFXCollision;
    [SerializeField]
    ParticleSystem PSCollision;

    void Start()
    {
        GameObject vc;

        _StarParam sp = this.GetComponent<_StarParam>();

        sp.playCollisionFX
            .Subscribe(count =>
            {
                // VFXを作成
                vc = Instantiate(VFXCollision.gameObject);
                vc.gameObject.transform.position = this.transform.position;
                vc.gameObject.SetActive(false);
                vc.gameObject.SetActive(true);
                // 一定時間後、VFXを削除
                Destroy(vc, count * 2);
            })
            .AddTo(this.gameObject);

        this.GetComponent<_StarParam>().playDeathFX
            .Subscribe(count =>
            {
                // 死亡処理を起動
                sp.StarDeath(count);
                // VFXを生成
                vc = Instantiate(VFXCollision.gameObject);
                vc.gameObject.transform.position = this.transform.position;
                vc.gameObject.SetActive(false);
                vc.gameObject.SetActive(true);
                // 一定時間後VFXを削除
                Destroy(vc, count * 2);
            })
            .AddTo(this.gameObject);
    }
}

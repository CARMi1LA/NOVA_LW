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

        this.GetComponent<_StarParam>().playCollisionFX
            .Subscribe(count =>
            {
                vc = Instantiate(VFXCollision.gameObject);
                vc.gameObject.transform.position = this.transform.position;
                vc.gameObject.SetActive(false);
                vc.gameObject.SetActive(true);

                Destroy(vc, count * 2);
            })
            .AddTo(this.gameObject);

        this.GetComponent<_StarParam>().playDeathFX
            .Subscribe(count =>
            {

            })
            .AddTo(this.gameObject);
    }
}

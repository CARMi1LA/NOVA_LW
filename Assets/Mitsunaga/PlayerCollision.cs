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

        // 衝突時のエフェクト
        this.GetComponent<_StarParam>().playCollisionFX
            .Subscribe(count =>
            {
                // 位置、サイズなどを指定して、VFXを起動する
                VFXCollision.gameObject.transform.position = this.transform.position;
                VFXCollision.SetFloat("SpawnRate", VCSpawnRate);
                VFXCollision.SetFloat("PlayerSize", GameManager.Instance.playerTransform.localScale.x);
                VFXCollision.SetFloat("LoopTime", count);
                VFXCollision.gameObject.SetActive(false);
                VFXCollision.gameObject.SetActive(true);
                // PSを起動する
                //PSCollision.Play();

                // VFXを一定時間後に停止させる
                Invoke("VCStop", count);
            })
            .AddTo(this.gameObject);
        // 死亡時のエフェクト
        this.GetComponent<_StarParam>().playDeathFX
            .Subscribe(count =>
            {

            })
            .AddTo(this.gameObject);
    }

    void VCStop()
    {
        VFXCollision.SetFloat("SpawnRate", 0);
    }
}

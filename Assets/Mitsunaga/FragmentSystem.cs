using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class FragmentSystem : MonoBehaviour
{
    // 星のかけらのシステム
    // ランダムに回転する

    Vector3 randomRotate;

    [SerializeField, Header("回転速度")]
    float RotationSpeed = 10;
    [SerializeField, Header("消滅時のエフェクト")]
    VisualEffect vfxCollision;

    void Start()
    {
        // ランダムな角度
        randomRotate = new Vector3(Random.Range(-5.0f, 5.0f),
                                   Random.Range(-5.0f, 5.0f),
                                   Random.Range(-5.0f, 5.0f));

        // くるくるまわす
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // 常に少しずつ回転している
                this.transform.eulerAngles += (randomRotate * RotationSpeed * Time.deltaTime);

                // フィールドの外に出たら破壊される
                if (Vector3.Distance(this.transform.position,GameManager.Instance.bossTransform.position) >= CoreModeManager.fieldRange / 2)
                {
                    GameObject fx = Instantiate(vfxCollision.gameObject);
                    fx.transform.position = this.transform.position;
                    Destroy(this.gameObject);
                }
            })
            .AddTo(this.gameObject);
    }

    void Update()
    {
        
    }
}

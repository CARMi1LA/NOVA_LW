using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class FragmentSystem : MonoBehaviour
{
    // 星のかけらのシステム
    // ランダムに回転する

    Vector3 randomRotate;

    [SerializeField, Header("回転速度")]
    float RotationSpeed = 10;

    void Start()
    {
        // ランダムな角度
        randomRotate = new Vector3(Random.Range(-1.0f, 1.0f),
                                   Random.Range(-1.0f, 1.0f),
                                   Random.Range(-1.0f, 1.0f));

        // くるくるまわす
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                this.transform.eulerAngles += (randomRotate * RotationSpeed * Time.deltaTime);
            })
            .AddTo(this.gameObject);
    }

    void Update()
    {
        
    }
}

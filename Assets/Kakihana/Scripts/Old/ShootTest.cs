using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class ShootTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                this.transform.Rotate(new Vector3(0, 180, 0) * Time.deltaTime);
                //float speed = 0.0f;
                //speed = 1 * Time.deltaTime;
                //this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 180, 0), speed);
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Delay(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => 
            {
                new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
                Debug.Log("Test生成");
            }).AddTo(this.gameObject);
    }

}

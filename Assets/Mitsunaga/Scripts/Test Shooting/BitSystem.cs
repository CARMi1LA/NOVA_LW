using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BitSystem : MonoBehaviour
{
    [SerializeField]
    GameObject bullet = null;

    float shotInt = 0.2f;

    void Start()
    {
        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Sample(TimeSpan.FromSeconds(shotInt))
            .Subscribe(_ =>
            {
                Debug.Log(Vector3.Angle(this.transform.position, GameManager.Instance.cursorPos));

                GameObject bul = Instantiate(bullet);
                bul.transform.position = this.transform.position;

                float deg = GetAngleXZ(this.transform.position, GameManager.Instance.cursorPos);

                bul.transform.eulerAngles = new Vector3(90, 0, deg - 90.0f);
            })
            .AddTo(this.gameObject);
    }

    // 2点間の角度(Degree)を取得する
    // start  : 原点
    // target : 標的
    float GetAngleXZ(Vector3 start, Vector3 target)
    {
        Vector3 dist = target - start;              // 2点の距離を取得
        float radian = Mathf.Atan2(dist.z, dist.x); // 距離のX,Z値から角度(Radian)を取得
        float degree = radian * Mathf.Rad2Deg;      // 角度(Radian)から角度(degree)に変換

        return degree;
    }
}

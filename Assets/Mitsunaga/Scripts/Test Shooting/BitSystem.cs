using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BitSystem : MonoBehaviour
{
    [SerializeField]
    int bitID = 0;
    [SerializeField]
    float bitRange = 10.0f;
    [SerializeField]
    GameObject bullet = null;

    float rotSpeed = 8;

    float shotInt = 0.01f;

    Vector3 targetPosition = Vector3.zero;

    void Start()
    {
        Vector3 playerPos = Vector3.zero;
        float time = 0.0f;

        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Subscribe(_ =>
            {
                playerPos = GameManager.Instance.playerTransform.position;

                time += Time.deltaTime;
                targetPosition = GetTargetPosition(time);

                /*this.transform.position = new Vector3(
                    Mathf.Lerp(this.transform.position.x, playerPos.x + (targetPosition.x * bitRange), 0.1f),
                    0.0f,
                    Mathf.Lerp(this.transform.position.z, playerPos.z + (targetPosition.z * bitRange), 0.1f)
                    );*/

                this.transform.position = playerPos + targetPosition * bitRange;
            })
            .AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Sample(TimeSpan.FromSeconds(shotInt))
            .Subscribe(_ =>
            {
                GameObject bul = Instantiate(bullet);
                bul.transform.position = this.transform.position;


                float deg;
                if (GameManager.Instance.cursorFlg)
                {
                    deg = GetAngleXZ(GameManager.Instance.playerTransform.position, this.transform.position);
                }
                else
                {
                    deg = GetAngleXZ(GameManager.Instance.playerTransform.position, GameManager.Instance.cursorPos);
                }

                bul.transform.eulerAngles = new Vector3(90, 0, deg - 90.0f);
            })
            .AddTo(this.gameObject);
    }

    Vector3 GetTargetPosition(float time)
    {
        Vector3 tp = Vector3.zero;
        float deg = 0;

        if (GameManager.Instance.cursorFlg)
        {
            if (bitID == 1)
            {
                tp = new Vector3(Mathf.Cos(time * rotSpeed), 0.0f, Mathf.Sin(time * rotSpeed));
            }
            else
            {
                tp = new Vector3(Mathf.Cos((time * rotSpeed) + (Mathf.Deg2Rad * 180.0f)), 0.0f, Mathf.Sin((time * rotSpeed) + (Mathf.Deg2Rad * 180.0f)));
            }
        }
        else
        {
            deg = GetAngleXZ(GameManager.Instance.playerTransform.position, GameManager.Instance.cursorPos);

            tp = new Vector3(Mathf.Cos(Mathf.Deg2Rad * deg), 0.0f, Mathf.Sin(Mathf.Deg2Rad * deg));
        }

        return tp;
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

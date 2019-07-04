using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField, Header("最大回転値")]
    float rotSpeed = 90;


    void Start()
    {
        Vector3 rot = new Vector3(
            rotSpeed,
            0,
            0
            );

        Debug.Log(rot);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                transform.Rotate(rot * Time.deltaTime);
                if(transform.eulerAngles.x < -180)
                {
                    transform.eulerAngles += new Vector3(360, 0, 0);
                }
                else if (transform.eulerAngles.x > 180)
                {
                    transform.eulerAngles += new Vector3(-360, 0, 0);
                }
            })
            .AddTo(this.gameObject);
    }
}

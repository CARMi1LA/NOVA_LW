using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField, Header("ステージ1の当たり判定")]
    Collider colliderS01;
    [SerializeField, Header("タイトルの当たり判定")]
    Collider colliderTitle;

    void Start()
    {
        colliderS01.OnTriggerEnterAsObservable()
            .Where(c => c.GetComponent<_StarParam>().starID == 1)
            .Subscribe(c =>
            {
                GameManager.Instance.FadeOut("03 Stage01");
            })
            .AddTo(this.gameObject);

        colliderTitle.OnTriggerEnterAsObservable()
            .Where(c => c.GetComponent<_StarParam>().starID == 1)
            .Subscribe(c =>
            {
                GameManager.Instance.FadeOut("01 Title");
            })
            .AddTo(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;


public class LineRendererP2B : MonoBehaviour
{
    // プレイヤーとボスをラインレンダラーでつなぐスクリプト

    Transform[] targetObject = new Transform[2];

    LineRenderer line;
    [SerializeField, Header("ラインのマテリアル 左右,上下の順")]
    Material[] lineMat = new Material[2];

    const float LINE_WIDTH = 0.5f;  // ラインの幅

    void Start()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;

        line.startWidth = LINE_WIDTH;    // 始点の幅
        line.endWidth = LINE_WIDTH;      // 終点の幅

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                targetObject[0] = GameManager.Instance.playerTransform;
                targetObject[1] = GameManager.Instance.bossTransform;

                for (int i = 0; i < line.positionCount; ++i)
                {
                    line.SetPosition(i, targetObject[i].position);
                }
            })
            .AddTo(this.gameObject);
    }
}

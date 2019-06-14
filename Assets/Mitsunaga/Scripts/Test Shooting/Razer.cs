using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Razer : MonoBehaviour
{
    // レインレンダラーを用いてレーザーを表示するスクリプト
    // カーソルと接触した場合、カーソルが左右どちらにあるかを取得
    // カーソルとの近さに応じて進行方向を変化させる

    LineRenderer line;

    struct point
    {
        public Vector3 moveVector; // 進行方向
    }

    bool isDeath = false;       // 死亡したか否か
    bool isCollision = false;   // 衝突しているか否か
    float speed = 2;            // 進む速度
    float range = 10;           // 長さ

    const float LINE_WIDTH = 0.5f;  // ラインの幅

    point[] points = new point[3];

    void Start()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 3;         // 頂点数
        line.startWidth = LINE_WIDTH;   // 始点の幅
        line.endWidth   = LINE_WIDTH;   // 終点の幅

        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Subscribe(_ =>
            {
                for(int i = 0; i < points.Length; ++i)
                {
                    line.SetPosition(i, line.GetPosition(i) + points[i].moveVector * speed * Time.deltaTime);
                }
            })
            .AddTo(this.gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BossCoreMode : MonoBehaviour
{
    // ボスのコアモードのAI

    [SerializeField, Header("探索範囲")]
    float searchRange = 20;
    float searchInterval = 0.5f;    // 探索間隔

    Vector3 moveDir = Vector3.zero; // 移動角度

    float[] searchAngles = new float[] { 0, 45, -45, 90, -90, 135, -135, 180 };

    void Start()
    {
        EnemySystem bossSystem = GetComponent<EnemySystem>();

        // 自分を中心に8方向を一定間隔で探索し、ルートを出す
        bossSystem.coreEscape
            .Where(_ => GameManager.Instance.isCoreMode.Value)
            .Subscribe(_ =>
            {
                Debug.Log("");

                int rayNumber = 0;

                Vector3 searchDir = new Vector3(
                    0.0f,
                    Vector3.Angle(this.transform.position, GameManager.Instance.playerTransform.position) * -1,
                    0.0f
                );

                while (rayNumber < 8)
                {
                    Vector3 addDir = new Vector3(0.0f,searchAngles[rayNumber], 0.0f);

                    Debug.DrawRay(
                        transform.position + (transform.localScale.x + 0.1f) * (searchDir + addDir).normalized,
                        (searchDir + addDir).normalized, 
                        new Color(1, 1, 0, 1), 
                        searchRange
                    );

                    // Physics.Raycast(原点、角度、長さ)
                    // 衝突すればtrue、しなければfalse
                    if (Physics.Raycast(transform.position, searchDir + addDir , searchRange))
                    {
                        rayNumber++;
                    }
                    else
                    {
                        moveDir = addDir;
                        break;
                    }
                    moveDir = Vector3.zero;
                }

                transform.eulerAngles += moveDir;
                // bossSystem.SetMoveDirection(moveDir);

            })
            .AddTo(this.gameObject);
    }
}

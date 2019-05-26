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
            .Sample(TimeSpan.FromSeconds(searchInterval))
            .Subscribe(_ =>
            {
                int rayNumber = 0;

                Vector3 dirPtB = (GameManager.Instance.playerTransform.position - this.transform.position).normalized;
                float dirY = Mathf.Rad2Deg * Mathf.Atan2(dirPtB.z, dirPtB.x);

                Vector3 playerDir = new Vector3(
                    0.0f,
                    dirY + 180.0f,
                    0.0f
                );

                while (rayNumber < 8)
                {
                    Vector3 addDir = new Vector3(0.0f,searchAngles[rayNumber], 0.0f);
                    Vector3 searchDir = playerDir + addDir;

                    RaycastHit hit;

                    // Physics.Raycast(原点、角度、長さ)
                    // 衝突すればtrue、しなければfalse
                    if (Physics.Raycast(transform.position + transform.localScale.x * searchDir,searchDir ,out hit, searchRange)
                    )
                    {
                        rayNumber++;
                    }
                    else
                    {
                        Debug.Log("RayNumber : " + rayNumber.ToString());
                        moveDir = new Vector3(Mathf.Cos(searchDir.y), 0.0f, Mathf.Sin(searchDir.y));
                        break;
                    }
                }
                Debug.Log("moveDir : " + moveDir.ToString());
                bossSystem.SetMoveDirection(this.transform.eulerAngles + moveDir);

            })
            .AddTo(this.gameObject);
    }
}

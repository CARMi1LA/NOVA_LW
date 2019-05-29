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
    float searchRange = 10;
    float searchInterval = 0.5f;    // 探索間隔

    Vector3 moveDir = Vector3.zero; // 移動角度

    float[] searchAngles = new float[] { 0, 45, -45, 90, -90, 135, -135, 180 };

    [SerializeField]
    Transform debugObj;

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

                Vector3 playerDir = (GameManager.Instance.playerTransform.position - this.transform.position);
                playerDir *= -1;

                moveDir = playerDir.normalized;

                while (rayNumber < 8)
                {
                    Vector3 addDir = new Vector3(Mathf.Cos(searchAngles[rayNumber]), 0.0f, Mathf.Sin(searchAngles[rayNumber]));
                    Vector3 searchDir = (playerDir + addDir).normalized;

                    RaycastHit hit;

                    debugObj.position = transform.position + (transform.localScale.x * 0.55f * searchDir) + (searchRange * searchDir);
                    Debug.Log(Vector3.Distance(new Vector3(0, 0, 0), transform.position + (transform.localScale.x * 0.55f * searchDir) + (searchRange * searchDir)));
                    
                    // Physics.SphereCast(原点、球の大きさ、角度、当たった情報、長さ)
                    // 衝突すればTrue、しなければFalse
                    // 球がステージの外にある場合もTrue
                    if (
                        Physics.SphereCast(transform.position + (transform.localScale.x * 0.55f * searchDir),
                                           transform.localScale.x, searchDir,
                                           out hit, searchRange) ||
                        Vector3.Distance(new Vector3(0, 0, 0), transform.position + (transform.localScale.x * 0.55f * searchDir) + (searchRange * searchDir))
                        > CoreModeManager.fieldRange / 2
                       )
                    {
                        Debug.Log("Collision!!");
                        rayNumber++;
                    }
                    else
                    {
                        Debug.Log("RayNumber : " + rayNumber.ToString());
                        moveDir = searchDir;
                        break;
                    }
                }
                Debug.Log("moveDir : " + moveDir.ToString());
                bossSystem.SetMoveDirection(moveDir);

            })
            .AddTo(this.gameObject);
    }
}

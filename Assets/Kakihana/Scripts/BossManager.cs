using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class BossManager : MonoBehaviour
{
    public float bossSpeed;                 // 移動スピード
    public float maxDistance;               // アクティブ状態になるまでの距離
    public int score;

    public Vector3 moveForword;             // 移動方向
    public FloatReactiveProperty distance;  // プレイヤー間の距離

    public GameObject destroyEffect;// 消滅エフェクト
    // Start is called before the first frame update
    void Start()
    {
        GameMaster.Instance.bossFlg
            .Where(_ => GameMaster.Instance.bossFlg.Value == true)
            .Subscribe(_ => 
            {
                this.gameObject.SetActive(true);
                moveForword = (GameMaster.Instance.playerPos.position - this.transform.position).normalized;
                this.transform.forward = -moveForword;
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(_ => GameMaster.Instance.bossFlg.Value == true)
            .Where(_ => distance.Value >= Mathf.Pow(maxDistance,2))
            .Subscribe(_ => 
            {
                this.transform.eulerAngles = new Vector3(0, Mathf.Sin(Time.time), 0);
                this.transform.position += this.transform.forward * bossSpeed * Time.deltaTime;
            }).AddTo(this.gameObject);

            this.UpdateAsObservable()
            .Sample(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                distance.Value = (this.transform.position - GameMaster.Instance.playerPos.position).sqrMagnitude;
            }).AddTo(this.gameObject);
    }

    public void EnemyDestroy()
    {
        Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class EnemyController : MonoBehaviour
{
    // 敵のスクリプト

    public Vector3 rot;                             // 回転量
    public int score;                               // 取得スコア
    [SerializeField] private float rotSpeed;        // 回転スピード
    [SerializeField] GameObject destroyEffect;      // 消滅エフェクト

    // Start is called before the first frame update
    void Start()
    {
        // 回転をランダムに設定
        rot = new Vector3(
            Random.Range(-5.0f, 5.0f),
            Random.Range(-5.0f, 5.0f),
            Random.Range(-5.0f, 5.0f)
            );

        // 回転量を元に回転させる
        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                this.transform.localEulerAngles += (rot * rotSpeed * Time.deltaTime);
            }).AddTo(this.gameObject);

        // パルスに入ったら消滅する
        this.OnTriggerExitAsObservable()
            .Where(c => c.gameObject.tag == "Pulse")
            .Subscribe(c => 
            {
                PluseDestroy();
            }).AddTo(this.gameObject);
    }

    // プレイヤーと接触したときの消滅処理
    public void EnemyDestroy()
    {
        // エフェクト再生
        Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
        // このオブジェクトを消滅させる
        Destroy(this.gameObject);
    }

    // パルス内消滅処理
    public void PluseDestroy()
    {
        // エフェクト再生
        Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
        // このオブジェクトを消滅させる
        Destroy(this.gameObject);
    }
}

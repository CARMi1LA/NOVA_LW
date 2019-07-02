using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletManager : MonoBehaviour
{
    /* 
      弾が持つスクリプト
      生成時のステータスを元に実行される
      敵やプレイヤーに衝突したらこのスクリプトのステータスを元にダメージ処理を行う
    */

    // 誰が攻撃したか
    public enum ShootChara
    {
        Player = 0,
        Enemy
    }
    // 生成したキャラクター情報
    [SerializeField] public ShootChara shootChara;
    [SerializeField] public float shootSpeed;            // 発射スピード
    [SerializeField] private float rangeLimit;           // 最大距離
    [SerializeField] public int damageAtk;               // 攻撃力
    [SerializeField] public int listIndex;               // 消滅時にスポナーに消滅を知らせるために必要

    [SerializeField] public Transform shootOriginTrans; // 発射元の座標
    [SerializeField] public BoolReactiveProperty isDestroy; // 現在非表示かどうか

    public BulletManager(int atk, float speed, Transform origin, ShootChara chara)
    {
        // 攻撃力の設定
        damageAtk = atk;
        // スピードを設定する
        shootSpeed = speed;
        // 発射元座標の設定
        shootOriginTrans = origin;
        // 発射元キャラクターの設定
        shootChara = chara;
    }
    // Start is called before the first frame update
    void Start()
    {
        // 弾を発射する
        this.GetComponent<Rigidbody>().AddForce(transform.up * shootSpeed, ForceMode.Impulse);

        // 最大距離を超えたら消滅
        this.UpdateAsObservable()
            //.Where(_ => !GameManager.Instance.isPause.Value)
            .Where(_ => Vector3.Distance(this.transform.position, shootOriginTrans.position) >= rangeLimit)
            .Subscribe(_ => 
            {
                BulletDestroy();
            }).AddTo(this.gameObject);
    }

    // 消滅メソッド
    public void BulletDestroy()
    {
        isDestroy.Value = true;
        BulletSpawner.Instance.BulletRemove(listIndex);
        // このオブジェクトを非表示にする
        this.gameObject.SetActive(false);
    }

    // 弾生成時のメソッド
    public void BulletCreate(int atk, float speed, Transform origin, ShootChara chara,int index)
    {
        // このオブジェクトを表示する
        this.gameObject.SetActive(true);
        // 攻撃力の設定
        damageAtk = atk;
        // スピードを設定する
        shootSpeed = speed;
        // 発射元座標の設定
        shootOriginTrans = origin;
        // 発射元キャラクターの設定
        shootChara = chara;
        listIndex = index;
        // 現在の座標の初期化
        this.transform.position = origin.position;
    }

}

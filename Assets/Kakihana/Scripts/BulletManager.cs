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
        Enemy,
        None
    }

    // 弾の生成状況
    public enum BulletState
    {
        Active = 0,     // 生成済み
        Destroy,        // 消滅
        Pool            // 生成待機状態
    }
    // 生成したキャラクター情報
    [SerializeField] public ShootChara shootChara;       // 生成元のキャラクター
    [SerializeField] private BulletState bulletState;    // 弾の生成状況
    [SerializeField] public float shootSpeed;            // 発射スピード
    [SerializeField] private float rangeLimit;           // 最大距離
    [SerializeField] public int damageAtk;               // 攻撃力
    [SerializeField] public int listIndex;               // 消滅時にスポナーに消滅を知らせるために必要

    [SerializeField] public Transform shootOriginTrans; // 発射元の座標

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
        this.GetComponent<Rigidbody>().AddForce(shootOriginTrans.forward * shootSpeed,ForceMode.Impulse);
        this.transform.rotation = Quaternion.LookRotation(this.transform.forward,shootOriginTrans.forward);
        //this.UpdateAsObservable()
        //    .Subscribe(_ =>
        //    {

        //    }).AddTo(this.gameObject);
        // 最大距離を超えたら消滅
        this.UpdateAsObservable()
            .Where(_ => bulletState == BulletState.Active)
            .Where(_ => Vector3.Distance(this.transform.position, shootOriginTrans.position) >= rangeLimit)
            .Subscribe(_ => 
            {
                bulletState = BulletState.Destroy;
                BulletDestroy();
            }).AddTo(this.gameObject);
    }

    // 消滅メソッド
    public void BulletDestroy()
    {
        if (bulletState == BulletState.Destroy)
        {
            Debug.Log("削除");
            // 現在の生成数を減らす
            BulletSpawner.Instance.bulletCount--;
            // 弾のスポナーに削除情報を送る
            BulletSpawner.Instance.BulletRemove(this);
            // このオブジェクトを非表示にする
            this.gameObject.SetActive(false);
            // 待機モードに移行
            bulletState = BulletState.Pool;
        }

    }

    // 弾生成時のメソッド
    public void BulletCreate(int atk, float speed, Transform origin, ShootChara chara,int index)
    {
        Debug.Log("弾生成");
        // このオブジェクトを表示する
        this.gameObject.SetActive(true);
        // スポナーの生成数を増やす
        BulletSpawner.Instance.bulletCount++;
        // ステートの初期化
        bulletState = BulletState.Active;
        // 攻撃力の設定
        damageAtk = atk;
        // スピードを設定する
        shootSpeed = speed;
        // 発射元座標の設定
        shootOriginTrans = origin;
        // 発射元キャラクターの設定
        shootChara = chara;
        // 自分の要素番号の設定
        listIndex = index;
        // 現在の座標の初期化
        this.transform.position = origin.position;
        //

    }

}

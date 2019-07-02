using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletSpawner : BMSingleton<BulletSpawner>
{
    // 弾生成管理クラス

    [SerializeField] private int bulletValueMax;            // 弾の最大生成数
    [SerializeField] private int bulletCount;               // 弾の生成数
    [SerializeField] private BulletManager[] bulletObj;     // 弾のプレハブ
    [SerializeField] private BulletPool bulletPool;         // 弾のオブジェクトプールクラス

    [SerializeField] private Transform bulletPoolTrans;     // オブジェクトプールを格納するための変数

    // 弾のデータを格納するリスト
    [SerializeField] public ReactiveCollection<BulletManager> bulletDataList = new ReactiveCollection<BulletManager>();

    // Start is called before the first frame update
    void Start()
    {
        // プールの初期化
        bulletPool = new BulletPool(bulletObj[0], bulletPoolTrans);
        // 弾生成処理、他スクリプトで弾の生成が呼び出されたときのみ動作
        bulletDataList.ObserveAdd()
        .Subscribe(_ =>
        {
            Debug.Log("kenti2");
            // プールの生成
            var bullet = bulletPool.Rent();
            // 弾の生成
            bullet.BulletCreate(_.Value.damageAtk, _.Value.shootSpeed, _.Value.shootOriginTrans, _.Value.shootChara,_.Index);
            // 生成をカウントする
            bulletCount++;
            // プールを監視し、消滅したらプールを返却する、リストの削除も行う
            bullet.OnDisableAsObservable().Subscribe(x =>
            {
                bulletPool.Return(bullet);
                //if (bulletDataList[_.Index].isDestroy.Value == true)
                //{
                //    bulletDataList.Remove(bulletDataList[_.Index]);

                //}
            }).AddTo(this.gameObject);
        }).AddTo(this.gameObject);

        bulletDataList.ObserveRemove()
            .Subscribe(_ => 
            {

            }).AddTo(this.gameObject);
    }

    // 弾消滅時の処理
    public void BulletRemove(int index)
    {
        //if (bulletDataList[index].)
        //{

        //}
        // 消滅分を現在の生成数から削除
        bulletCount--;
        // リストからデータを削除する
        bulletDataList.Remove(bulletDataList[index]);
    }
}

// 弾生成パラメータを格納するクラス
public class BulletData
{
    public int initAtk;         // 攻撃力
    public float initSpeed;     // 速度
    public Transform initTrans; // 発射元の座標

    public BulletManager.ShootChara initShootChara; // 誰が発射したか
    // パラメータの設定
    public BulletData(int atk, float speed, Transform trans, BulletManager.ShootChara chara)
    {
        initAtk = atk;
        initSpeed = speed;
        initTrans = trans;
        initShootChara = chara;
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletSpawner : BSSingleton<BulletSpawner>
{
    // 弾生成管理クラス

    [SerializeField] private int bulletValueMax;            // 弾の最大生成数
    [SerializeField] public int bulletCount;                // 弾の生成数
    [SerializeField] private BulletManager[] bulletObj;     // 弾のプレハブ
    [SerializeField] private BulletPool bulletPool;         // 弾のオブジェクトプールクラス

    [SerializeField] private Transform bulletPoolTrans;     // オブジェクトプールを格納するための変数

    // 生成予定の弾のデータを格納するリスト
    [SerializeField] public ReactiveCollection<BulletData> bulletDataList = new ReactiveCollection<BulletData>();

    // 生成した弾を格納するリスト
    [SerializeField] public ReactiveCollection<BulletManager> bulletList = new ReactiveCollection<BulletManager>();
    // Start is called before the first frame update
    void Start()
    {
        // プールの初期化
        bulletPool = new BulletPool(bulletObj[0], bulletPoolTrans);

        // 弾生成処理、生成予定のデータリストに情報が追加された時に動作
        bulletDataList.ObserveAdd()
        .Where(_ => bulletCount <= bulletValueMax)
        .Subscribe(_ =>
        {
            // プールの生成
            var bullet = bulletPool.Rent();
            Debug.Log("弾スポーン");
            // 弾の生成
            bullet.BulletCreate(_.Value.initAtk, _.Value.initSpeed, _.Value.initTrans, _.Value.initShootChara,_.Index);
            // 生成済みリスト情報を追加
            bulletList.Add(bullet);
            // 弾生成用データリストは不要になるので破棄する。
            bulletDataList.Remove(_.Value);
        }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(_ => bulletDataList.Count >= 100).Subscribe(_ => 
            {
                bulletDataList.Clear();
            }).AddTo(this.gameObject);
    }

    // プールの返却と出現済みリストから弾の情報を削除するメソッド
    public void BulletRemove(BulletManager bm)
    {
        bulletPool.Return(bm);
        bulletList.Remove(bm);
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
        BulletSpawner.Instance.bulletDataList.Add(this);
    }
}
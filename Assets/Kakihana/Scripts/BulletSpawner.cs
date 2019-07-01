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
    [SerializeField] private BulletManager[] bulletObj;     // 
    [SerializeField] private BulletPool bulletPool;

    [SerializeField] private Transform bulletPoolTrans;

    [SerializeField] public ReactiveCollection<BulletData> bulletDataList = new ReactiveCollection<BulletData>();

    public delegate void BulletInit(float speed,int atk,Transform origin,BulletManager.ShootChara chara);

    // Start is called before the first frame update
    void Start()
    {
        bulletPool = new BulletPool(bulletObj[0], bulletPoolTrans);
        bulletDataList.ObserveAdd()
        .Subscribe(_ =>
        {
            Debug.Log("kenti2");
            var bullet = bulletPool.Rent();
            bulletCount++;
            bullet.BulletCreate(_.Value.initAtk, _.Value.initSpeed, _.Value.initTrans, _.Value.initShootChara,_.Index);

            bullet.OnDisableAsObservable().Subscribe(x =>
            {
                bulletPool.Return(bullet);
            }).AddTo(this.gameObject);
        }).AddTo(this.gameObject);

        bulletDataList.ObserveRemove()
            .Subscribe(_ => 
            {

            }).AddTo(this.gameObject);
    }

    public void BulletRemove(int index)
    {
        bulletCount--;
        bulletDataList.Remove(bulletDataList[index]);
    }
}

public class BulletData
{
    public int initAtk;
    public float initSpeed;
    public Transform initTrans;
    public BulletManager.ShootChara initShootChara;
    public BulletData(int atk, float speed, Transform trans, BulletManager.ShootChara chara)
    {
        initAtk = atk;
        initSpeed = speed;
        initTrans = trans;
        initShootChara = chara;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class EnemyManager : MonoBehaviour,IDamage
{
    [SerializeField] EnemyDataList enemyDataList;
    [SerializeField] EnemyStatus myStatus;

    // 敵のAI
    public enum EnemyAI
    {
        Approach = 0,       // 接近モード
        Wait,               // 待機モード
        Attack,             // 攻撃モード
        Escape              // 逃走モード
    }

    // キャラクターの種類
    public enum CharactorType
    {
        Player = 0,         // プレイヤー
        EnemyBoss = 1,      // ボス
        EnemyCommon = 2,    // 敵（一般）
        None = 3,           // 未設定
    }

    // 全キャラ共通の通常攻撃方法 
    public enum NormalAtkType
    {
        Burst = 0,      // 一定方向に3発連続する弾を発射する
        Scatter = 1,    // 3方向に1発ずつ弾を発射する
        OneShot = 2,    // 1発ずつだが強力な弾を発射する
        None = 3        // 攻撃を行わない、または未設定
    }

    // キャラクターの属性の変数
    [SerializeField] public CharactorType charaType = CharactorType.None;
    // 攻撃方法の変数
    [SerializeField] public NormalAtkType atkType = NormalAtkType.None;

    [SerializeField] private Rigidbody myRigid;

    [SerializeField] IntReactiveProperty myHp;
    [SerializeField] BoolReactiveProperty attackFlg = new BoolReactiveProperty(false);
    [SerializeField] EnemyAIReactiveProperty enemyAI = new EnemyAIReactiveProperty();
    [SerializeField] public IReadOnlyReactiveProperty<EnemyAI> enemyAIPropaty
    {
        get { return enemyAI; }
    }
    void Start()
    {
        enemyAIPropaty.Where(_ => _ == EnemyAI.Approach)
            .Subscribe(_ => 
            {
                
            }).AddTo(this.gameObject);


        Observable.Timer(TimeSpan.FromSeconds(30.0f)).Subscribe(_ =>
        {
            Death();
        }).AddTo(this.gameObject);

        myHp.Where(_ => _ <= 0).Subscribe(_ => 
        {
            Death();
        }).AddTo(this.gameObject);

        this.OnCollisionEnterAsObservable()
            .Subscribe(c => 
            {
                try
                {
                    BulletManager bullet;
                    bullet = c.gameObject.GetComponent<BulletManager>();
                if (bullet.shootChara == BulletManager.ShootChara.Player)
                    {
                        HitDamage(bullet.damageAtk);
                    }
                }
                catch 
                {
                }
            }).AddTo(this.gameObject);
    }

    public void EnemySpawn(Vector3 pos,int id,int level)
    {
        enemyDataList = Resources.Load<EnemyDataList>(string.Format("Enemy{0}", id));
        myStatus = enemyDataList.EnemyStatusList[level - 1];
        myHp.Value = myStatus.hp;
        charaType = CharactorType.EnemyCommon;
        atkType = NormalAtkType.Burst;
        enemyAI.Value = EnemyAI.Approach;
        transform.position = pos;
    }

    // 消滅時の処理
    public void Death()
    {
        this.gameObject.SetActive(false);
        EnemySpawner.Instance.EnemyDestroy();
    }

    // ダメージを受けたときの処理
    public void HitDamage(int damage)
    {
        myHp.Value -= damage;
    }
}

// 敵AI専用のカスタムプロパティ
[System.Serializable]
public class EnemyAIReactiveProperty : ReactiveProperty<EnemyManager.EnemyAI>
{
    public EnemyAIReactiveProperty() { }
    public EnemyAIReactiveProperty(EnemyManager.EnemyAI initialValue) : base (initialValue) { }
}
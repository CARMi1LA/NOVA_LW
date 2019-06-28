using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class EnemyManager : MonoBehaviour,IDamage
{
    // 敵データのリスト
    [SerializeField] EnemyDataList enemyDataList;
    // 敵データが格納されているクラス
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

    [SerializeField] private Rigidbody myRigid;             // 自分のRigidBody
    [SerializeField] private Transform playerTrans;         // プレイヤーの座標
    [SerializeField] private Vector3 movePos;               // 移動ベクトル
    [SerializeField] private float maxDistance = 30.0f;     // プレイヤーとの最大接近距離

    // 現在のHP
    [SerializeField] private IntReactiveProperty myHp;
    // 攻撃可能かどうかを管理するBool型プロパティ
    [SerializeField] private BoolReactiveProperty attackFlg = new BoolReactiveProperty(false);
    // 敵AIのステート用カスタムプロパティ
    [SerializeField] private EnemyAIReactiveProperty enemyAI = new EnemyAIReactiveProperty();
    // ステート用カスタムプロパティの書き換えを防ぐ為、参照はこちらを使用
    [SerializeField] private FloatReactiveProperty distance = new FloatReactiveProperty(0.0f);

    // 参照用のカスタムプロパティ
    [SerializeField] public IReadOnlyReactiveProperty<EnemyAI> enemyAIPropaty
    {
        get { return enemyAI; }
    }
    void Start()
    {
        // 【接近モード移行イベント】
        // 敵が最大接近距離よりも遠ければ接近モードへ移行する
        distance.Where(_ => _ >= maxDistance)
            .Subscribe(_ => 
            {
                enemyAI.Value = EnemyAI.Approach;
            }).AddTo(this.gameObject);

        // 【待機モード移行イベント】
        // 敵が最大接近距離に到達したら次の行動を待つ
        distance.Where(_ => _ == Mathf.Clamp(maxDistance, maxDistance - 5, maxDistance))
            .Where(_ => enemyAIPropaty.Value == EnemyAI.Approach)
            .Where(_ => attackFlg.Value == false)
            .Subscribe(_ => 
            {
                enemyAI.Value = EnemyAI.Wait;
            }).AddTo(this.gameObject);

        // 【攻撃モード移行イベント】
        // 一定時間待機した後、攻撃を行う
        enemyAIPropaty.Where(_ => _ == EnemyAI.Wait)
            .Delay(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                enemyAI.Value = EnemyAI.Attack;
            }).AddTo(this.gameObject);

        // 【逃走モード移行イベント】
        // 現在のHPが1/4以下になったら逃走モードへ切り替える
        myHp.Where(hp => hp <= hp * 0.25f)
            .Subscribe(_ =>
            {
                enemyAI.Value = EnemyAI.Escape;
            }).AddTo(this.gameObject);

        // 【接近モードイベント】
        enemyAIPropaty.Where(_ => _ == EnemyAI.Approach)
            .Subscribe(_ => 
            {
                Vector3 dif = playerTrans.position - this.transform.position;
                float radian = Mathf.Atan2(dif.z, dif.x);
                float degree = radian * Mathf.Rad2Deg;

                myRigid.AddForce(new Vector3(Mathf.Cos(radian),0,Mathf.Sin(radian)) * myStatus.moveSpeed);
            }).AddTo(this.gameObject);

        enemyAIPropaty.Where(_ => _ == EnemyAI.Attack)
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
        maxDistance = maxDistance * ((11 - level) * 0.1f);
        charaType = CharactorType.EnemyCommon;
        atkType = NormalAtkType.Burst;
        enemyAI.Value = EnemyAI.Approach;
        transform.position = pos;
        distance.Value = Vector3.Distance(playerTrans.position, this.transform.position);
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
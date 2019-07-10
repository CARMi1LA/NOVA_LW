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
    [SerializeField] private float velocityMag = 0.99f;     // 減速倍率

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
        distance.Where(_ => _ >= Mathf.Pow(maxDistance,2))
            .Subscribe(_ => 
            {
                velocityMag = 0.99f;
                enemyAI.Value = EnemyAI.Approach;
            }).AddTo(this.gameObject);

        // 【待機モード移行イベント】
        // 敵が最大接近距離に到達したら減速し次の行動を待つ
        distance.Where(_ => _ <= Mathf.Pow(maxDistance,2))
            .Where(_ => enemyAIPropaty.Value == EnemyAI.Approach)
            .Where(_ => attackFlg.Value == false)
            .Subscribe(_ => 
            {
                velocityMag = 0.66f;
                enemyAI.Value = EnemyAI.Wait;
            }).AddTo(this.gameObject);

        // 【攻撃モード移行イベント】
        // 一定時間待機した後、攻撃を行う
        enemyAIPropaty.Where(_ => _ == EnemyAI.Wait)
            .Sample(TimeSpan.FromSeconds(1.0f))
            .Subscribe(_ =>
            {
                attackFlg.Value = true;
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
                myRigid.AddForce(new Vector3(Mathf.Cos(radian),0,Mathf.Sin(radian)) * myStatus.moveSpeed * 100);
            }).AddTo(this.gameObject);

        // 攻撃モードイベント
        enemyAIPropaty.Where(_ => _ == EnemyAI.Attack)
        .Where(_ => attackFlg.Value == true)
        .Sample(TimeSpan.FromSeconds(0.25f))
        .Subscribe(_ =>
        {
            new BulletData(myStatus.atk, myStatus.moveSpeed, this.transform, BulletManager.ShootChara.Enemy);
        }).AddTo(this.gameObject);

        // HPが0になると消滅する
        myHp.Where(_ => _ <= 0).Subscribe(_ => 
        {
            Death();
        }).AddTo(this.gameObject);

        // 衝突判定
        this.OnTriggerEnterAsObservable()
            .Subscribe(c => 
            {
                try
                {
                    BulletManager bullet;
                    bullet = c.gameObject.GetComponent<BulletManager>();
                if (bullet.shootChara == BulletManager.ShootChara.Player)
                    {
                        // プレイヤーによる攻撃であればダメージを受ける
                        HitDamage(bullet.damageAtk);
                        bullet.BulletDestroy();
                    }
                }
                catch 
                {
                }
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Sample(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                myRigid.velocity *= velocityMag;
                distance.Value = (playerTrans.position - this.transform.position).sqrMagnitude;
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                this.transform.LookAt(playerTrans);
            }).AddTo(this.gameObject);
    }

    // 生成されたときの初期化メソッド
    public void EnemySpawn(Vector3 pos,int id,int level)
    {
        // IDより取得したいデータリストを取得する
        enemyDataList = Resources.Load<EnemyDataList>(string.Format("Enemy{0}", id));
        // データリストよりレベルに応じたパラメータを取得
        myStatus = enemyDataList.EnemyStatusList[level - 1];
        // プレイヤー座標の取得
        playerTrans = GameManagement.Instance.playerTransform;
        // HPの設定
        myHp.Value = myStatus.hp;
        // プレイヤーまでの接近距離の設定（レベルが高いほど近くまで接近）
        maxDistance = maxDistance * ((11 - level) * 0.1f);
        // キャラクターの種類の設定
        charaType = CharactorType.EnemyCommon;
        // 攻撃モードの設定
        atkType = NormalAtkType.Burst;
        // AIモードの設定
        enemyAI.Value = EnemyAI.Approach;
        // 座標の設定
        transform.position = pos;
        // プレイヤーとの距離を設定
        distance.Value = (playerTrans.position - this.transform.position).sqrMagnitude;
    }

    // 消滅時の処理
    public void Death()
    {
        // スコアドロップ用ループ文
        for (int i = 0; i < 5; i++)
        {
            // スコアアイテムを5個ドロップする。経験値もループ回数に応じて分割する
            new ItemData(myStatus.score / 5,0,0,DropItemManager.ItemType.Score,this.transform.position);
        }
        // オブジェクトを非表示に
        this.gameObject.SetActive(false);
        // スポーンクラスに消滅応報を送る
        EnemySpawner.Instance.EnemyDestroy();
    }

    // ダメージを受けたときの処理
    public void HitDamage(int damage)
    {
        myHp.Value -= damage;
        Debug.LogFormat("HIT!ダメージ{0}", damage);
    }

    public void MoveApproach()
    {

    }

    public void MoveEscape()
    {

    }
    
    public void MoveDeceleration()
    {

    }
}

// 敵AI専用のカスタムプロパティ
[System.Serializable]
public class EnemyAIReactiveProperty : ReactiveProperty<EnemyManager.EnemyAI>
{
    public EnemyAIReactiveProperty() { }
    public EnemyAIReactiveProperty(EnemyManager.EnemyAI initialValue) : base (initialValue) { }
}
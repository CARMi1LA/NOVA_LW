using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerManager : MonoBehaviour,IDamage
{
    public enum SpAttackType
    {
        Explosive = 0,
        Lazer = 1,
        None = 2
    }

    [SerializeField] PlayerUnit[] playerUnits;      // 子機の情報
    [SerializeField] Transform[] unitTrans;         // 子機のトランスフォーム
    [SerializeField] EnemyDataList playerDataList;  // データが格納されているリスト
    [SerializeField] public EnemyStatus myStatus;   // 各種パラメータの情報

    // 特殊攻撃のステート
    [SerializeField] private SpAttackType spAtk = SpAttackType.None;

    [SerializeField] private float hp;              // 現在のHP
    [SerializeField] private float maxHp = 0;       // 最大HP
    [SerializeField] private Rigidbody myRigid;

    // レベルを監視可能な変数
    [SerializeField] public IntReactiveProperty level;     
    // クリックしたか
    [SerializeField] public BoolReactiveProperty isClick = new BoolReactiveProperty(false);

    [SerializeField] private IntReactiveProperty unitValue = new IntReactiveProperty(1);

    [SerializeField] private Vector3 cScreen;
    [SerializeField] private Vector3 cWorld;
    [SerializeField] private Vector3 dif;

    // スコアを監視可能な変数
    public IntReactiveProperty score = new IntReactiveProperty(0);
    // コンストラクタ
    PlayerManager()
    {

    }

    void Awake()
    {
        // レベルの取得
        level = GameManagement.Instance.playerLevel;
        // プレイヤーのパラメータのデータリストを取得
        playerDataList = Resources.Load<EnemyDataList>(string.Format("PlayerData"));
        // データリストよりレベルに応じたパラメータを取得
        myStatus = playerDataList.EnemyStatusList[level.Value - 1];
        // HPの設定
        hp = myStatus.hp;
        // 最大HPの設定
        maxHp = myStatus.hp;
    }

    // Start is called before the first frame update
    void Start()
    {
        // レベルが更新された時のみ、呼び出される
        level.Subscribe(_ =>
        {
            // パラメータの更新
            myStatus = playerDataList.EnemyStatusList[level.Value - 1];
            // 最大HPの更新
            maxHp = myStatus.hp;
            // レベルアップ分のHPを現在のHPに代入
            hp = hp + (hp - maxHp);
        }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                cScreen = Input.mousePosition;
                cScreen.z = 100.0f;
                cWorld = Camera.main.ScreenToWorldPoint(cScreen);

                dif = (cWorld - this.transform.position).normalized;
                float radian = Mathf.Atan2(dif.y, dif.x);

                Vector3 movePos = this.transform.position + dif;
                movePos.y = 0.0f;

                transform.position = movePos;
            }).AddTo(this.gameObject);

        // クリックで弾を出します（デバッグ用）
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => 
            {
                new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
                Debug.Log("生成");
            }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Where(c => gameObject.tag == "Bullet")
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

        this.OnTriggerEnterAsObservable()
            .Where(c => c.gameObject.tag == "Item")
            .Subscribe(c => 
            {
                try
                {
                    DropItemManager item;
                    item = c.gameObject.GetComponent<DropItemManager>();

                    score.Value += item.itemScore;
                    item.ItemDestroy();
                }
                catch
                {
                }
            }).AddTo(this.gameObject);
    }
    public void HitDamage(int atk)
    {
        hp -= atk;
    }
}

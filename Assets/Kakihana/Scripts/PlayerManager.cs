﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerManager : MonoBehaviour
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

    // レベルを監視可能な変数
    [SerializeField] public IntReactiveProperty level;     
    // クリックしたか
    [SerializeField] public BoolReactiveProperty isClick = new BoolReactiveProperty(false);

    [SerializeField] private IntReactiveProperty unitValue = new IntReactiveProperty(1);

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

        // クリックで弾を出します（デバッグ用）
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => 
            {
                new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
                Debug.Log("生成");
            }).AddTo(this.gameObject);

    }
}

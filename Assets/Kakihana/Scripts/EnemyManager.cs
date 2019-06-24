using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] EnemyDataList enemyDataList;
    [SerializeField] EnemyStatus myStatus;

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
    //EnemyManager(int Id, int Level)
    //{
    //    enemyDataList = Resources.Load<EnemyDataList>(string.Format("Enemy{0}", Id));
    //    myStatus = enemyDataList.EnemyStatusList[Level - 1];

    //    #region 初期化処理 ステータス名 = myStatus.各ステータス
    //    charaType = myStatus.charaType;
    //    atkType = myStatus.atkType;
    //    hp = myStatus.hp;
    //    barrier = myStatus.barrier;
    //    atk = myStatus.atk;
    //    exp = myStatus.exp;
    //    moveSpeed = myStatus.moveSpeed;
    //    #endregion 
    //}

    void Start()
    {
        Observable.Timer(TimeSpan.FromSeconds(5.0f)).Subscribe(_ =>
        {
            Death();
        }).AddTo(this.gameObject);
    }

    public void EnemySpawn(Vector3 pos,int id,int level)
    {
        enemyDataList = Resources.Load<EnemyDataList>(string.Format("Enemy{0}", id));
        myStatus = enemyDataList.EnemyStatusList[level - 1];

        charaType = CharactorType.EnemyCommon;
        atkType = NormalAtkType.Burst;
        transform.position = pos;
    }

    // 消滅時の処理
    public void Death()
    {
        this.gameObject.SetActive(false);
        EnemySpawner.Instance.EnemyDestroy();
    }
}

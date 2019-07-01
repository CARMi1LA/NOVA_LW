using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using Random = UnityEngine.Random; // ランダム関数はUnityEngineの物を使う

public class EnemySpawner : PlanetSingleton<EnemySpawner>
{
    /*
       二代目PlanetSpawner 
       前作の星生成スクリプトを現在の仕様に調整したもの
       主に敵生成の管理を行う
    */

    [Header("シーン毎に設定が必要な変数")]
    [SerializeField] private int spawnMaxValue;                 // 最大スポーン数
    [SerializeField] private int hotSpotMax;                    // ホットスポット（プレイヤー周辺のエリア）の最大スポーン数
    [SerializeField] private float hotSpotRadiusMax;            // ホットスポットスポーン範囲の最大半径
    [SerializeField] private float hotSpotRadiusMin;            // ホットスポットスポーン範囲の最小半径

    [SerializeField] private int level;                         // 現在のレベル

    [Header("デバッグ用に値を変更可能な変数")]
    [SerializeField] private float enemySpawnInterval;          // 敵再生成までの時間（秒）
    [SerializeField] private EnemyManager[] enemyPrefab;        // 生成させる敵
    [SerializeField] private EnemyPool enemyPool;               // 敵のオブジェクトプール

    [Header("プールをまとめるオブジェクトを作成、格納")]
    [SerializeField] private Transform enemyPoolObj;            // スポーンした敵をまとめるオブジェクトをここに格納

    [Header("自動稼働し、設定する必要がない変数")]
    [SerializeField] private int spawnCount;                    // 現在のスポーン数
    [SerializeField] private int planetObjNum;                  // スポーン予定のプレハブの配列番号
    [SerializeField] private float xAbs, zAbs;                  // スポーン先座標の絶対値
    [SerializeField] private float maxR, minR;                  // ホットスポットのスポーン最小範囲と最大範囲を2乗したもの

    [SerializeField] private Vector3 spawnPos;                  // スポーン先の座標
    [SerializeField] private Transform playerTrans;             // プレイヤーのトランスフォーム
    [SerializeField] private Transform bossObjTrans;            // ボスオブジェクトのトランスフォーム

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤー情報の取得
       // playerTrans = GameManager.Instance.playerTransform;

        // レベルの取得
        //level = GameManager.Instance.playerLevel;

        // ホットスポットの半径を2乗する
        maxR = Mathf.Pow(hotSpotRadiusMax, 2);
        minR = Mathf.Pow(hotSpotRadiusMin, 2);

        // オブジェクトプールの初期化
        enemyPool = new EnemyPool(enemyPoolObj, enemyPrefab[0]);

        Observable.Interval(TimeSpan.FromSeconds(enemySpawnInterval))
            .Where(_ => spawnCount <= spawnMaxValue)
            .Subscribe(_ => 
            {
                EnemySpawn();
            }).AddTo(this.gameObject);
    }
    void EnemySpawn()
    {
        if (spawnCount >= spawnMaxValue) return;
        RaycastHit hit;                           // 敵重なり防止用Rayの当たり判定

        // 敵の生成先座標を設定
        spawnPos = new Vector3(
            Random.Range(-hotSpotRadiusMax, hotSpotRadiusMax),
            0,
            Random.Range(-hotSpotRadiusMax, hotSpotRadiusMax)
            );

        // 生成先座標の絶対値を計算
        xAbs = Mathf.Abs(Mathf.Pow(spawnPos.x, 2));
        zAbs = Mathf.Abs(Mathf.Pow(spawnPos.z, 2));

        // スポーン可能な場合、スポーン先座標が半径の2乗以内であればスポーンする
        if (maxR > xAbs + zAbs && zAbs + zAbs > minR)
        {
            var enemy = enemyPool.Rent();
            spawnCount++;
            enemy.EnemySpawn(spawnPos + playerTrans.position, 1, level);

            enemy.OnDisableAsObservable().Subscribe(_ =>
            {
                enemy.Death();
                enemyPool.Return(enemy);
            }).AddTo(this.gameObject);
        }
        else
        {
            Debug.Log("スポーン範囲外");
        }
    }

    public void EnemyDestroy()
    {
        spawnCount--;
    }
}

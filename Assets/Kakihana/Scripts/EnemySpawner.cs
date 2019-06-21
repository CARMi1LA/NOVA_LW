using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

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
    [SerializeField] private float planetSpawnInterval;         // 敵再生成までの時間（秒）
    [SerializeField] private EnemyManager[] enemyPrefab;        // 生成させる敵
    [SerializeField] private PlanetPool planetPool;             // 敵のオブジェクトプール

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
        playerTrans = GameManager.Instance.playerTransform;

        // レベルの取得
        level = GameManager.Instance.playerLevel;

        // ホットスポットの半径を2乗する
        maxR = Mathf.Pow(hotSpotRadiusMax, 2);
        minR = Mathf.Pow(hotSpotRadiusMin, 2);
    }
}

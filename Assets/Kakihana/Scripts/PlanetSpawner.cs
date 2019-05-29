﻿using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Random = UnityEngine.Random; // ランダム関数はUnityEngineの物を使う

public class PlanetSpawner : PlanetSingleton<PlanetSpawner>
{
    // 惑星自動生成スクリプト

    [Header("シーン毎に設定が必要な変数")]

    [SerializeField] private int planetMaxnum;                  // 最大スポーン数
    [SerializeField] private int hotSpotMax;                    // ボス周辺エリアの最大スポーン数

    [SerializeField] private float hotSpotRadiusMax;            // ボス周辺エリアスポーン範囲の最大半径
    [SerializeField] private float hotSpotRadiusMin;            // ボス周辺エリアスポーン範囲の最小半径

    [SerializeField] private float planetSpawnHeight;
    [SerializeField] private float stageSize;                   // ランダム生成の範囲

    [SerializeField] private int level;                         // 現在のレベル

    [Header("デバッグ用に値を変更可能")]
    [SerializeField] private float planetSpawnInterval;           // 惑星の再出現までの時間（秒）
    [SerializeField] private float highSpeedSpawnInterval;        // 惑星の再出現までの時間（高速生成用、単位は秒）
    [Header("シーン毎に設定が必要なコンポーネント")]
    [SerializeField] private EnemySystem[] planetPrefab;        // スポーンする惑星をここに格納
    [SerializeField] private PlanetPool planetPool;             // 惑星のオブジェクトプール
    [Header("プールをまとめるオブジェクトを作成、格納")]
    [SerializeField] private Transform poolObjTrans;            // スポーンした惑星をまとめるオブジェクトをここに格納
    [Header("レベル毎の惑星の大きさの最小値")]
    [SerializeField] private float[] planetScaleMin;            // レベル毎の惑星の大きさの最小値
    [Header("レベル毎の惑星の大きさの最大値")]
    [SerializeField] private float[] planetScaleMax;            // レベル毎の惑星の大きさの最大値


    [Header("自動稼働し、設定する必要がないもの")]
    [SerializeField] private int count;                         // 現在のスポーン数
    [SerializeField] private int planetObjNum;                  // スポーン予定のプレハブの配列番号
    [SerializeField] private float[] planetObjRadius;           // 惑星プレハブの半径を格納する配列
    [SerializeField] private float bossRadius;                  // ボスオブジェクトの円周
    [SerializeField] private float xAbs, zAbs;                  // スポーン先座標の絶対値
    [SerializeField] private float maxR, minR;                  // ボス周辺エリアスポーンの最小範囲と最大範囲を2乗したもの

    [SerializeField] private Vector3 spawnPos;                  // スポーンする惑星の座標
    [SerializeField] private Transform playerPos;
    [SerializeField] private Transform bossObjTrans;            // ボスオブジェクトのトランスフォーム
    public float debugTime;                                     // デバッグ用時間経過をカウントする
    
    // Start is called before the first frame update
    void Start()
    {
        // プレイヤー情報の取得
        playerPos = GameManager.Instance.playerTransform;
        // ボス情報の取得
        bossObjTrans = GameManager.Instance.bossTransform;
        // ボスオブジェクトの円周を求める
        bossRadius = bossObjTrans.localScale.x * Mathf.PI;

        level = GameManager.Instance.playerLevel;

        // ボス周辺エリアの半径を2乗する
        maxR = Mathf.Pow(hotSpotRadiusMax, 2);
        minR = Mathf.Pow(hotSpotRadiusMin, 2);

        // オブジェクトプールの初期化
        planetPool = new PlanetPool(poolObjTrans,planetPrefab[0]);

        /*
         惑星をスポーンするかどうかの処理を行う
         最大生成数を超えてスポーンしない
         また、敵残存に応じて処理の実行タイミングを変化させている。（既存は残存数が最大生成数÷2以下）
        */
        this.UpdateAsObservable()
            .Where(_ => count < planetMaxnum)
            .Delay(count <= planetMaxnum / 2 ? 
            (TimeSpan.FromSeconds(highSpeedSpawnInterval)) : (TimeSpan.FromSeconds(planetSpawnInterval)))
            .Subscribe(_ =>
            {
                if (count < hotSpotMax)
                {
                    // 惑星は一定値になるまでボス周辺エリアにスポーンする
                    HotSpotCreate();
                    Debug.Log("HotSpotSpawn");
                }
                else
                {
                    // 一定値を超えると最大スポーン数まですべての範囲でスポーンする
                    PlanetCreate();
                    Debug.Log("NormalSpawn");
                }
            }).AddTo(this.gameObject);

        // 指定したフレームごとに実行、最大生成数を超えると実行されない
        //Observable.IntervalFrame(planetSpawnInterval)
        //    .Where(_ => count < planetMaxnum).Subscribe(_ =>
        //    {
        //        if (count < hotSpotMax)
        //        {
        //            // 惑星は一定値になるまでボス周辺エリアにスポーンする
        //            HotSpotCreate();
        //            Debug.Log("HotSpotSpawn");
        //        }
        //        else
        //        {
        //            // 一定値を超えると最大スポーン数まですべての範囲でスポーンする
        //            PlanetCreate();
        //            Debug.Log("NormalSpawn");
        //        }
        //    }).AddTo(this.gameObject);

        // 60秒毎にオブジェクトプールをリフレッシュする
        Observable.Timer(TimeSpan.FromSeconds(60.0f)).Subscribe(_ =>
        {
            // オブジェクトプールのリフレッシュを行う
            // 現在のオブジェクトプールを50%削減するが最低でも生成した分は残す
            planetPool.Shrink(instanceCountRatio: 0.5f, minSize: count, callOnBeforeRent: false);
            Debug.Log("Pool開放");
        });

        this.UpdateAsObservable().
            Where(c => level != GameManager.Instance.playerLevel).
            Subscribe(c =>
            {
                level = GameManager.Instance.playerLevel;
            }).AddTo(this.gameObject);
    }

    // 通常スポーン用
    void PlanetCreate()
    {
        if (count == planetMaxnum ) return; // 生成量が最大生成量を超えたらこのメソッドは起動しない
        planetObjNum = Random.Range(0, planetPrefab.Length); // 生成したい惑星を取得

        spawnPos.x = Random.Range(-stageSize, stageSize); // 生成座標の設定
        spawnPos.y = 0.0f;
        spawnPos.z = Random.Range(-stageSize, stageSize);

        // スポーン予定座標とプレイヤーとの距離を計算
        float distance = Vector3.Distance(spawnPos, playerPos.position);
        Debug.Log("プレイヤーとの距離" + distance);

        // スポーン予定座標とプレイヤーとの距離が近ければスポーンしない
        if (distance <= 30.0f) return;

        // オブジェクトプールに追加
        var planet = planetPool.Rent();
        count++;
        // 惑星をスポーンさせる
        // 大きさはレベル毎に設定された最小値と最大値内のランダムで抽出し、自身の大きさと掛ける
        planet.PlanetSpawn(spawnPos, playerPos.localScale.x * Random.Range(planetScaleMin[level-1], planetScaleMax[level-1]));
        // 消滅時、オブジェクトをプールに返す
        planet.OnDisableAsObservable().Subscribe(_ =>
        {
            planet.Stop();
            planetPool.Return(planet);
        }).AddTo(planet.gameObject);
    }
    // ボス周辺エリア専用スポーン
    private void HotSpotCreate()
    {
        if (count == hotSpotMax) return;                     // 設定されてる値を超えたらこのメソッドは起動しない
        planetObjNum = Random.Range(0, planetPrefab.Length); // 生成したい惑星を取得
        RaycastHit hit;                                      // 惑星重なり防止用Rayの当たり判定

        // スポーン予定の惑星の大きさを事前に算出
        // 大きさはレベル毎に設定された最小値と最大値内のランダムで抽出し、自身の大きさと掛ける
        var planetSubscription = playerPos.localScale.x / 2 * Random.Range(planetScaleMin[level - 1], planetScaleMax[level - 1]);

        // スポーン座標をランダムで生成
        spawnPos.x = Random.Range(-hotSpotRadiusMax, hotSpotRadiusMax);
        spawnPos.y = planetSpawnHeight;
        spawnPos.z = Random.Range(-hotSpotRadiusMax, hotSpotRadiusMax);

        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(spawnPos, playerPos.position);
        Debug.Log("プレイヤーとの距離" + distance);

        // スポーン予定座標とプレイヤーとの距離が近ければスポーンしない
        if (distance <= 30.0f) {
            Debug.Log("プレイヤーが近くにいるため、スポーン範囲外");
            return;
        };

        xAbs = Mathf.Abs(Mathf.Pow(spawnPos.x, 2));
        zAbs = Mathf.Abs(Mathf.Pow(spawnPos.z, 2));
        // 惑星をスポーンする前にスポーンしたい惑星の大きさと同じ球型Rayを飛ばす
        if (Physics.SphereCast(spawnPos,planetSubscription,Vector3.down,out hit))
        {
            // 既に惑星がいる場合はスポーン不可
            Debug.DrawRay(spawnPos, hit.point, Color.red,5);
            Debug.Log("スポーン不可");
        }
        else
        {
            // スポーン可能な場合、スポーン先座標が半径の2乗以内であればスポーンする
            if (maxR > xAbs + zAbs && zAbs + zAbs > minR)
            {
                Debug.DrawRay(spawnPos, hit.point, Color.red);
                Debug.Log("惑星スポーン");
                // オブジェクトプールに追加
                var planet = planetPool.Rent();
                // 惑星スポーン、数をカウント
                count++;
                // 惑星をスポーンさせる
                // 事前に算出した大きさを代入
                planet.PlanetSpawn(
                spawnPos + bossObjTrans.position,
                planetSubscription);
                Debug.Log(spawnPos + bossObjTrans.position);
                
                // 消滅時、オブジェクトをプールに返す
                planet.OnDisableAsObservable().Subscribe(_ =>
                {
                    planet.Stop();
                    planetPool.Return(planet);
                }).AddTo(planet);
            }
            else
            {
                Debug.Log("スポーン範囲外");
            }
        }
    }

    // 惑星が消滅するときに呼び出される
    public void PlanetDestroy()
    {
        // 現在の生成数を減らす
        count--;
    }

    // シーン名取得メソッド
    public string NowSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}


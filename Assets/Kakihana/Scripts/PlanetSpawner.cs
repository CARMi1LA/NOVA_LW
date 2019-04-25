using System;
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

// 二次元配列のリストをインスペクター上で表示するためのクラス
[System.SerializableAttribute]
public class LevelSpawnPlanets
{
    public List<float> planetScales = new List<float>();
    public LevelSpawnPlanets(List<float> list)
    {
        planetScales = list;
    }
}

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

    [SerializeField] private int level;

    [Header("デバッグ用に値を変更可能")]
    [SerializeField] private int planetSpawnInterval;         // 惑星の再出現までのフレーム

    [Header("シーン毎に設定が必要なコンポーネント")]
    [SerializeField] private Transform bossObjTrans;            // ボスオブジェクトのトランスフォーム
    [SerializeField] private EnemyController[] planetPrefab;      // スポーンする惑星をここに格納
    [SerializeField] private List<LevelSpawnPlanets> planetScaleList = new List<LevelSpawnPlanets>();
    [SerializeField] private PlanetPool planetPool;             // 惑星のオブジェクトプール
    [SerializeField] private Transform hierarchyTrans;          // スポーンしたオブジェクトをまとめるために必要
    [SerializeField] private float[] planetScales;            // 惑星の大きさの格納できる配列
    [SerializeField] private float[] planetScaleMax;
    [SerializeField] private float[] planetScaleMin;

    [Header("自動稼働し、設定する必要がないもの")]
    [SerializeField] private int count;                         // 現在のスポーン数
    [SerializeField] private int planetObjNum;                  // スポーン予定のプレハブの配列番号
    [SerializeField] private float[] planetObjRadius;           // 惑星プレハブの半径を格納する配列
    [SerializeField] private float bossRadius;                  // ボスオブジェクトの円周
    [SerializeField] private float xAbs, zAbs;                  // スポーン先座標の絶対値
    [SerializeField] private float maxR, minR;                  // ボス周辺エリアスポーンの最小範囲と最大範囲を2乗したもの

    [SerializeField] private Vector3 spawnPos;                  // スポーンする惑星の座標
    [SerializeField] private Transform playerPos;
    [SerializeField] private Vector3 debugPlayerPos;
    public float debugTime;                                     // デバッグ用時間経過をカウントする
    [SerializeField] private TextMeshProUGUI debugLevelText;
    
    // Start is called before the first frame update
    void Start()
    {
        playerPos = GameManager.Instance.playerTransform;
        debugPlayerPos = playerPos.position;
        // ボスオブジェクトの円周を求める
        bossRadius = bossObjTrans.localScale.x * Mathf.PI;
        // 初期化メソッド
        PlanetInit();

        debugLevelText.text = string.Format("Level:{0}",level);

        // ボス周辺エリアの半径を2乗する
        maxR = Mathf.Pow(hotSpotRadiusMax, 2);
        minR = Mathf.Pow(hotSpotRadiusMin, 2);

        // オブジェクトプールの初期化
        planetPool = new PlanetPool(hierarchyTrans,planetPrefab[0]);

        // 指定したフレームごとに実行
        Observable.IntervalFrame(planetSpawnInterval)
            .Where(_ => count < planetMaxnum).Subscribe(_ =>
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

        // 60秒毎にオブジェクトプールをリフレッシュする
        Observable.Timer(TimeSpan.FromSeconds(60.0f)).Subscribe(_ =>
        {
            // オブジェクトプールのリフレッシュを行う
            // 現在のオブジェクトプールを50%削減するが最低でも生成した分は残す
            planetPool.Shrink(instanceCountRatio: 0.5f, minSize: count, callOnBeforeRent: false);
            Debug.Log("Pool開放");
        });
    }

    // 通常スポーン用
    void PlanetCreate()
    {
        if (count == planetMaxnum ) return; // 30回生成されたらこのメソッドは起動しない
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
        // 惑星生成  planetScales[Random.Range(0,planetScales.Length)]
        //planet.PlanetSpawn(
        //    spawnPos, 
        //    planetScaleList[level - 1].planetScales[Random.Range(0, planetScaleList[level - 1].planetScales.Count())]);
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

        int scaleRandom = Random.Range(0, planetScales.Length);

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
        if (Physics.SphereCast(spawnPos,planetObjRadius[scaleRandom],Vector3.down,out hit))
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
                planet.PlanetSpawn(
                spawnPos + bossObjTrans.position, 
                playerPos.localScale.x * Random.Range(planetScaleMin[level-1], planetScaleMax[level-1]));
                //planetScaleList[level - 1].planetScales[Random.Range(0, planetScaleList[level - 1].planetScales.Count())]);
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

    // スポーンクラス初期設定
    void PlanetInit()
    {
        // 惑星プレハブのアタッチ分、半径を格納する配列の初期化
        planetObjRadius = new float[planetScales.Length];
        for(int i = 0;i < planetScales.Length; ++i)
        {
            // プレハブに格納されている全ての惑星の半径を取得し配列に格納する
            planetObjRadius[i] = planetScales[i] * 0.5f;
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

    // デバッグ用ボス周辺エリアを可視化する
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawSphere(bossObjTrans.position, bossRadius);
    }

    public void LevelUp()
    {
        level++;
        debugLevelText.text = string.Format("Level:{0}", level);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class EnemyStatus : MonoBehaviour
{
    /*
     出現するオブジェクトのステータスをまとめたクラス
     （EnemyStatusと書いてあるがプレイヤーの情報もここでまとめる）
    */

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

    [SerializeField] CharactorType charaType = CharactorType.None;
    [SerializeField] NormalAtkType atkType = NormalAtkType.None;

    [SerializeField] const float APDISTANCE = 0.0f;     // 認識する距離


    [SerializeField] private int hp = 0;
    [SerializeField] private int barrier = 0;
    [SerializeField] private int atk = 0;
    [SerializeField] private int level = 0;

    [SerializeField] private float moveSpeed = 0.0f;
    [SerializeField] private Rigidbody charaRig;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

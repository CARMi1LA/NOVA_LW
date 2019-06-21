using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[System.Serializable]
public class EnemyStatus : MonoBehaviour
{
    /*
     出現するオブジェクトのパラメータをまとめたクラス
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

    // キャラクターの属性の変数
    [SerializeField] public CharactorType charaType = CharactorType.None;
    // 攻撃方法の変数
    [SerializeField] public NormalAtkType atkType = NormalAtkType.None; 

    [SerializeField] public int hp = 0;                 // HP
    [SerializeField] public int barrier = 0;            // ダメージを一定量吸収するバリア
    [SerializeField] public int atk = 0;                // 攻撃力
    [SerializeField] public int exp = 0;                // 消滅時に落とす経験値



    [SerializeField] public float moveSpeed = 0.0f;
    [SerializeField] public Rigidbody charaRig;
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class GameMaster : SingletonGM<GameMaster>
{
    // ゲームスコア
    public IntReactiveProperty gameScore = new IntReactiveProperty(0);
    // ゲームクリアフラグ
    public BoolReactiveProperty isClear;
    // ゲームオーバーフラグ
    public BoolReactiveProperty isGameOver;
    // ボス出現フラグ
    public BoolReactiveProperty bossFlg;

    // プレイヤーの座標
    public Transform playerPos;
    // プレイヤーの情報
    public PlayerController player;

    // 各種UI（左からスコア、クリア、ゲームオーバー、HP）
    public Text scoreUI, clearUI, gameOverUI, hpUI;

    // スコア加算処理
    public Subject<int> AddScore = new Subject<int>();
    // Start is called before the first frame update
    void Start()
    {
        // スコアの初期化
        gameScore.Value = 0;
        // スコアUIを表示
        scoreUI.text = string.Format("Score:{0}", gameScore);
        // ゲーム開始時、クリアとゲームオーバーUIを非表示
        clearUI.enabled = false;
        gameOverUI.enabled = false;
        // HPUIを表示
        hpUI.text = string.Format("HP:{0}", player.hp.Value);

        // スコア加算処理
        AddScore.Subscribe(value => 
            {
                // スコアを加算
                gameScore.Value += value;
                // UIに反映させる
                scoreUI.text = string.Format("Score:{0}", gameScore.Value);
            }).AddTo(this.gameObject);

        // HPUI処理
        player.hp
            .Subscribe(_ => 
            {
                // HPをUIに反映させる
                hpUI.text = string.Format("HP:{0}", player.hp.Value);
            }).AddTo(this.gameObject);

        // クリア処理
        isClear
            .Where(_ => isClear.Value == true)
            .Subscribe(_ => 
            {
                // クリアをUIに表示
                clearUI.enabled = true;
            }).AddTo(this.gameObject);

        // ゲームオーバー処理
        isGameOver
            .Where(_ => isGameOver.Value == true)
            .Subscribe(_ => 
            {
                // ゲームオーバーをUIに表示
                gameOverUI.enabled = true;
            }).AddTo(this.gameObject);

        // ボス出現処理
        gameScore
            .Where(_ => gameScore.Value >= 100)
            .Subscribe(_ => 
            {
                bossFlg.Value = true;
            }).AddTo(this.gameObject);
    }


}

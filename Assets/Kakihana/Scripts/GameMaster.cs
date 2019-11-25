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
        // スコアUIを表示
        scoreUI.text = string.Format("Score:{0}", gameScore);
        // ゲーム開始時、クリアとゲームオーバーUIを非表示
        clearUI.enabled = false;
        gameOverUI.enabled = false;
        // HPUIを表示
        hpUI.text = string.Format("HP:{0}", player.hp.Value);

        AddScore.Subscribe(value => 
            {
                gameScore.Value += value;
                scoreUI.text = string.Format("Score:{0}", gameScore.Value);
            }).AddTo(this.gameObject);

        player.hp
            .Subscribe(_ => 
            {
                hpUI.text = string.Format("HP:{0}", player.hp.Value);
            }).AddTo(this.gameObject);

        isClear
            .Where(_ => isClear.Value == true)
            .Subscribe(_ => 
            {
                clearUI.enabled = true;
            }).AddTo(this.gameObject);

        isGameOver
            .Where(_ => isClear.Value == true)
            .Subscribe(_ => 
            {
                gameOverUI.enabled = true;
            }).AddTo(this.gameObject);

        gameScore
            .Where(_ => gameScore.Value >= 100)
            .Subscribe(_ => 
            {
                bossFlg.Value = true;
            }).AddTo(this.gameObject);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class GameMaster : SingletonGM<GameMaster>
{
    public IntReactiveProperty gameScore = new IntReactiveProperty(0);
    public BoolReactiveProperty isClear;
    public BoolReactiveProperty isGameOver;

    public Text scoreUI, clearUI, gameOverUI;

    public Subject<int> AddScore = new Subject<int>();
    // Start is called before the first frame update
    void Start()
    {
        scoreUI.text = string.Format("Score:{0}", gameScore);
        clearUI.enabled = false;
        gameOverUI.enabled = false;

        AddScore.Subscribe(value => 
            {
                gameScore.Value = value;
                scoreUI.text = string.Format("Score:{0}", gameScore);
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
    }


}

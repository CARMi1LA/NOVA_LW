using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

public class GameManagement : GMSingleton<GameManagement>
{

    public Transform playerTransform;
    public Vector3 cameraPos = Vector3.zero;
    public Vector3 cursorPos = Vector3.zero;

    public IntReactiveProperty gameLevel = new IntReactiveProperty(1);
    public IntReactiveProperty playerLevel = new IntReactiveProperty(1);
    public IntReactiveProperty playerScore = new IntReactiveProperty(0);

    public BoolReactiveProperty isClear = new BoolReactiveProperty(false);
    public BoolReactiveProperty gameOver = new BoolReactiveProperty(false);
    public BoolReactiveProperty isPause = new BoolReactiveProperty(true);

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        isClear.Where(x => x).
            Subscribe(_ =>
            {
                // クリア処理
            }).AddTo(this.gameObject);

        gameOver
            .Where(x => x)
            .Where(x => !isClear.Value)
            .Subscribe(_ =>
            {
                //ゲームオーバー処理
            }).AddTo(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetScore(IntReactiveProperty score)
    {
        playerScore = score;
    }
}

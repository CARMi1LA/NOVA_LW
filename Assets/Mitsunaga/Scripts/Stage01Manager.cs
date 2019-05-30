using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class Stage01Manager : MonoBehaviour
{
    // テキストを表示するスクリプト
    // 通常モード

    [SerializeField]
    Text bigText;

    void Start()
    {
        float cnt = 0;

        // ポーズ中のテキスト表示
        GameManager.Instance.isPause
            .Where(x => !GameManager.Instance.isClear.Value)
            .Where(x => !GameManager.Instance.isGameOver.Value)
            .Subscribe(x =>
            {
                if (GameManager.Instance.isPause.Value)
                {
                    bigText.text = "Click Start";
                }
                else
                {
                    bigText.text = "";
                }

            })
            .AddTo(this.gameObject);
        // ゲームオーバー時のテキスト表示
        GameManager.Instance.isGameOver
            .Where(x => GameManager.Instance.isGameOver.Value)
            .Subscribe(x =>
            {
                bigText.text = "Game Over...";
            })
            .AddTo(this.gameObject);
        // アップデート
        this.UpdateAsObservable()
            .Where(x => GameManager.Instance.isPause.Value)
            .Subscribe(x =>
            {
                cnt += Time.deltaTime;

                bigText.color = new Color(1, 1, 1, Mathf.Abs(Mathf.Sin(cnt)));
            })
            .AddTo(this.gameObject);
    }
}

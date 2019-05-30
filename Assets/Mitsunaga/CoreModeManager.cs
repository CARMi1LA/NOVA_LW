using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class CoreModeManager : MonoBehaviour
{
    // コアモードの管理
    // 制限時間の管理、フィールドの縮小、死亡処理など

    [SerializeField, Header("フィールドオブジェクト、エフェクト、初期サイズ")]
    Transform fieldObject;
    [SerializeField]
    VisualEffect vfxField;
    [SerializeField]
    float fieldStartRange = 300.0f;
    [SerializeField, Header("制限時間、時間表示テキスト")]
    float timeLimit = 30.0f;
    [SerializeField]
    Text timeText;

    ReactiveProperty<float> timeCount = new ReactiveProperty<float>();

    public static float fieldRange = 0;

    void Start()
    {
        fieldRange = fieldStartRange;
        vfxField.SetFloat("FieldSize", fieldStartRange / 2);

        timeCount.Value = timeLimit;

        // 制限時間に応じてステージを縮小する
        timeCount.Subscribe(_ =>
        {
            // 制限時間を表示
            timeText.text = (Mathf.Ceil(timeCount.Value * 10) / 10).ToString();
            // 制限時間切れでゲームオーバー
            if (_ <= 0.0f)
            {
                // プレイヤーの死亡時処理を実行
                GameManager.Instance.playerTransform.gameObject.GetComponent<_StarParam>()
                    .playDeathFX.OnNext(0.5f);
                // ゲームオーバーフラグを立てる
                GameManager.Instance.isGameOver.Value = true;
            }
            vfxField.SetFloat("FieldSize", fieldStartRange / 2 * (timeCount.Value / timeLimit));
            fieldRange = fieldStartRange * (timeCount.Value / timeLimit);

            fieldObject.localScale = new Vector3(fieldRange, fieldObject.localScale.y, fieldRange);
        })
        .AddTo(this.gameObject);

        // 制限時間を数える
        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Where(x => timeCount.Value > 0)
            .Subscribe(_ => 
            {
                vfxField.SetVector3("CenterPosition", GameManager.Instance.bossTransform.position);
                timeCount.Value += -Time.deltaTime;
                
                if(timeCount.Value <= 0)
                {
                    timeCount.Value = 0.0f;
                }
            })
            .AddTo(this.gameObject);
    }
}
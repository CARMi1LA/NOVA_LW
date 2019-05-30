using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;
using Cinemachine;
using UnityEngine.Playables;

public class CoreModeManager : MonoBehaviour
{
    // コアモードの管理
    // 制限時間の管理、フィールドの縮小、死亡処理など

    [SerializeField]
    Text bigText;

    [SerializeField, Header("フィールドオブジェクト、エフェクト")]
    Transform fieldObject;
    [SerializeField]
    VisualEffect vfxField;
    [SerializeField, Header("制限時間")]
    float timeLimit = 30.0f;

    [SerializeField, Header("クリア演出タイムライン、エフェクト、シネマシーンカメラ")]
    PlayableDirector pdClear;
    [SerializeField]
    VisualEffect vfxClear;
    [SerializeField]
    CinemachineVirtualCamera vcClear;

    float timeCount = 0;

    float fieldStartRange;

    void Start()
    {
        // 初期設定
        vfxClear.gameObject.SetActive(false);
        fieldStartRange = GameManager.Instance.fieldRange;
        vfxField.SetFloat("FieldSize", fieldStartRange);
        PlayerSystem playerParam = GameManager.Instance.playerTransform.gameObject.GetComponent<PlayerSystem>();

        timeCount = timeLimit;

        // 制限時間を数える
        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Where(x => timeCount > 0)
            .Subscribe(_ => 
            {
                // 制限時間を減少
                timeCount += -Time.deltaTime;
                // 制限時間切れでゲームオーバー
                if (timeCount <= 0.0f)
                {
                    // プレイヤーの死亡時処理を実行
                    playerParam.playDeathFX.OnNext(playerParam.waitCount);
                    // ゲームオーバーフラグを立てる
                    GameManager.Instance.isGameOver.Value = true;
                }
                // フィールドを縮小
                GameManager.Instance.fieldRange = fieldStartRange * (timeCount / timeLimit);
                fieldObject.localScale = new Vector3(GameManager.Instance.fieldRange * 2, fieldObject.localScale.y, GameManager.Instance.fieldRange * 2);
                fieldObject.position = GameManager.Instance.bossTransform.position + new Vector3(0, -2, 0);
                // エフェクトに情報を割り当て
                vfxField.SetFloat("FieldSize", GameManager.Instance.fieldRange);
                vfxField.SetVector3("CenterPosition", GameManager.Instance.bossTransform.position);
            })
            .AddTo(this.gameObject);

        // ポーズ時のテキスト表示
        GameManager.Instance.isPause
            .Where(x => !GameManager.Instance.isClear.Value)
            .Where(x => !GameManager.Instance.isGameOver.Value)
            .Subscribe(x =>
            {
                if (GameManager.Instance.isPause.Value)
                {
                    bigText.text = "Destroy Core!";
                }
                else
                {
                    bigText.text = "";
                }
            })
            .AddTo(this.gameObject);

        GameManager.Instance.isClear
            .Where(x => x)
            .Subscribe(_ =>
            {
                pdClear.Play();
                vcClear.Priority = 11;
                vfxClear.gameObject.SetActive(true);

                // フィールドの大きさを初期値に戻す
                GameManager.Instance.fieldRange = fieldStartRange;
                fieldObject.position = Vector3.zero;
                fieldObject.localScale = new Vector3(GameManager.Instance.fieldRange * 2, fieldObject.localScale.y, GameManager.Instance.fieldRange * 2);
                // エフェクトに情報を割り当て
                vfxField.SetFloat("FieldSize", GameManager.Instance.fieldRange);
                vfxField.SetVector3("CenterPosition", Vector3.zero);
            })
            .AddTo(this.gameObject);
    }
}
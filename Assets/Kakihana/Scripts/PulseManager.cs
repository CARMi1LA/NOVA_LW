using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PulseManager : MonoBehaviour
{
    // エリア収縮の処理

    // 初期サイズと完全収縮後のエリアサイズ
    [SerializeField] Vector3 startPulseSize,endPulseSize;
    // Start is called before the first frame update
    void Start()
    {
        // 時間を初期化
        float time = 0.0f;
        // 元のサイズを保存
        startPulseSize = this.transform.localScale;
        // 用意したデータから完全収縮後のサイズを取得
        endPulseSize = startPulseSize * StageManager.Instance.stageData.pulseSize[0];

        this.UpdateAsObservable()
            .Where(_ => StageManager.Instance.shrinkPulse.Value == true)
            .Subscribe(_ => 
            {
                // 時間を計測
                time += Time.deltaTime;
                // 徐々に自身のサイズを減らしていく
                this.transform.localScale = Vector3.Lerp(startPulseSize, endPulseSize, time / StageManager.Instance.stageData.shrinkTime[0]);
            }).AddTo(this.gameObject);
    }

}

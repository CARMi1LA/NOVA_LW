using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PulseManager : MonoBehaviour
{
    [SerializeField] Vector3 startPulseSize,endPulseSize;
    // Start is called before the first frame update
    void Start()
    {
        float time = 0.0f;
        startPulseSize = this.transform.localScale;
        endPulseSize = startPulseSize * StageManager.Instance.stageData.pulseSize[0];
        this.UpdateAsObservable()
            .Where(_ => StageManager.Instance.shrinkPulse.Value == true)
            .Subscribe(_ => 
            {
                time += Time.deltaTime;
                this.transform.localScale = Vector3.Lerp(startPulseSize, endPulseSize, time / StageManager.Instance.stageData.shrinkTime[0]);
            }).AddTo(this.gameObject);
    }

}

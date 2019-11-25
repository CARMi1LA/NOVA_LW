using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

// Unity側のランダム関数を使用
using Random = UnityEngine.Random;
public class StageManager : SMSingleton<StageManager>
{
    public StageDataList dataList;
    public StageData stageData;

    // 読み込みたいステージID
    public int loadStageID = 0;
    // 収縮フラグ
    public BoolReactiveProperty shrinkPulse = new BoolReactiveProperty(false);
    // 
    [SerializeField] public float size;

    protected override void Awake()
    {
        base.Awake();
        // データリストの取得
        dataList = Resources.Load<StageDataList>("StageDataList");
        // ステージIDより各データを取得
        stageData = dataList.stageDataList[loadStageID - 1];
    }
    // Start is called before the first frame update
    void Start()
    {
    }
}

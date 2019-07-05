using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Toolkit;

[System.Serializable]
public class PlanetPool : ObjectPool<EnemySystem>
{

    /* 
     【オブジェクトプールクラス】
     Unity標準機能のInstantiateとDestroyでは負荷が大きいのでオブジェクトが必要でなければ非表示にし
     必要な時に初期化して再び表示させる 
    */
    public readonly EnemySystem planetObj; // プールしたいプレファブ
    private Transform myTrans;               // プールしたオブジェクトをまとめるオブジェクトの座標

    // コンストラクタ
    public PlanetPool(Transform trans, EnemySystem planetPre)
    {
        myTrans = trans;
        planetObj = planetPre;
    }

    // 惑星をスポーンさせる
    protected override EnemySystem CreateInstance()
    {
        var e = GameObject.Instantiate(planetObj);
        e.transform.SetParent(myTrans);

        return e;
    }
}

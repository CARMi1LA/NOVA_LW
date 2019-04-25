using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UniRx;

[RequireComponent(typeof(Rigidbody))]
public class _StarParam : MonoBehaviour
{
    // 全ての星に共通する項目をまとめておきたい
    // 星のパラメータ　ID、サイズ、リジッドボディ
    // 実装システム：サイズ変更

    // 未実装システム：星の移動、バリア、マテリアル変更　など

    [SerializeField, Header("星のID 自:1 ボス:2 敵:3")]
    public int starID = 0;  // 自機やボスなどを指定するため
    [SerializeField, Header("星のサイズ")]
    FloatReactiveProperty starSize = new FloatReactiveProperty(0.0f);

    IEnumerator routine;            // 星のサイズコルーチンの管理
    float nextSize = 1.0f;          // 目標の星のサイズ

    protected Rigidbody starRig;    // 星のRigidbody

    protected void Awake()
    {
        // コルーチンの再生、停止をコントロールするためここで宣言
        routine = SetStarSizeCoroutine(nextSize);

        // starSizeの値が変化した場合、値をスケールに適用
        starSize.Subscribe(c =>
        {
            transform.localScale = new Vector3(starSize.Value, starSize.Value, starSize.Value);
        })
        .AddTo(gameObject);

        // Rigidbodyを取得して、Y軸の移動を停止させる
        starRig = GetComponent<Rigidbody>();
        starRig.constraints = RigidbodyConstraints.FreezePositionY;
    }

    // 星のサイズ設定
    // publicでGetとSetを指定しておく　ここから情報を取ってね
    public float GetStarSize()
    {
        return starSize.Value;
    }
    // size … 目標サイズ
    public void SetStarSize(float size)
    {
        // サイズが更新されるたび、コルーチンを再起動する必要がある
        StopCoroutine(routine);
        routine = null;
        routine = SetStarSizeCoroutine(size);
        StartCoroutine(routine);
    }

    // 星のサイズを変化させるコルーチン
    // size … 目標サイズ
    IEnumerator SetStarSizeCoroutine(float size)
    {
        if (size >= starSize.Value)
        {
            // 目標サイズが現在よりも大きい場合
            while (size >= starSize.Value)
            {
                starSize.Value = Mathf.Lerp(starSize.Value, size, 0.05f);
                yield return null;
            }
        }
        else if (size <= starSize.Value)
        {
            // 目標サイズが現在よりも小さい場合
            while (size <= starSize.Value)
            {
                starSize.Value = Mathf.Lerp(starSize.Value, size, 0.05f);
                yield return null;
            }
        }

        // 最後に誤差を修正する
        starSize.Value = size;
    }
}

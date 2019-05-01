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

    const float MDISTANCE = 30.0f;   // 敵星が反応してくる距離

    [SerializeField, Header("星のID 自:1 ボス:2 敵:3")]
    public int starID = 0;  // 自機やボスなどを指定するため
    [SerializeField, Header("星のサイズ")]
    FloatReactiveProperty starSize = new FloatReactiveProperty(0.0f);

    IEnumerator routine;            // 星のサイズコルーチンの管理
    float nextSize = 1.0f;          // 目標の星のサイズ

    protected Rigidbody starRig;    // 星のRigidbody
    protected BoolReactiveProperty isMoving = new BoolReactiveProperty(false);

    protected void Awake()
    {
        // コルーチンの再生、停止をコントロールするためここで宣言
        routine = SetStarSizeCoroutine(nextSize);

        // Rigidbodyを取得して、Y軸の移動を停止させる
        starRig = GetComponent<Rigidbody>();
        starRig.constraints = RigidbodyConstraints.FreezePositionY;

        // starSizeの値が変化した場合、値をスケールに適用
        starSize.Subscribe(c =>
        {
            transform.localScale = new Vector3(starSize.Value, starSize.Value, starSize.Value);
        })
        .AddTo(gameObject);

        isMoving.Subscribe(_ =>
        {
            if (isMoving.Value)
            {
                starRig.isKinematic = false;
            }
            else
            {
                starRig.isKinematic = true;
            }
        })
        .AddTo(this.gameObject);
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

    // 星の移動処理
    // speed    … 移動速度
    // speedMul … 移動速度への追従度(大きいほど加速、減速大)
    protected void SetStarMove(float speed,float speedMul)
    {
        // 自星1、ボス2、敵星3でそれぞれの移動を実装する
        // × 敵星はイベントで別の処理したほうが良いかもしれない
        // 〇 これはマウスカーソルに対応した処理なので、敵星には別でAIをつける
        switch (starID)
        {
            case 1:
                // マウスカーソルの方向を取得し、その方向に向かって力を加える
                isMoving.Value = true;
                Vector3 pDir = (GameManager.Instance.cursorPos - transform.position).normalized;
                starRig.AddForce(speedMul * ((pDir * speed) - starRig.velocity));
                break;
            case 2:
                isMoving.Value = false;
                break;
            case 3:
                // マウスカーソルの方向を取得し、その方向に向かって力を加える
                // 敵星の場合は、マウスカーソルの距離が近い場合のみ力を加える
                if (Vector3.Distance(this.transform.position,GameManager.Instance.cursorPos) <= MDISTANCE)
                {
                    isMoving.Value = true;
                    Vector3 eDir = (GameManager.Instance.cursorPos - transform.position).normalized;
                    starRig.AddForce(speedMul * ((eDir * speed) - starRig.velocity));
                }
                else
                {
                    isMoving.Value = false;
                }
                break;
            default:
                isMoving.Value = false;
                break;

        }
    }
}

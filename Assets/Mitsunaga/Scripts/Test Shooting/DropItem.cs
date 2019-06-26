using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class DropItem : MonoBehaviour
{
    // アイテム単体のスクリプト
    // これを親オブジェクトであるスポナーから敵が倒されたときにスポーンさせる

    // 　実装する動作
    // 自転：無重力状態で浮かぶアイテムの動作
    // 自壊：アイテムは取得されずに一定時間が経つと消滅する

    public enum ItemID
    {
        SCORE  = 0,
        LIFE   = 1,
        BULLET = 2,
    }

    [SerializeField]
    ItemID iID;
    [SerializeField]
    float DeathCount = 3;

    Vector3 randomRotate = Vector3.zero;
    float   randomDir = 0.0f;
    Vector3 startPos;


    void Start()
    {
        // ランダムな角度で回転
        randomRotate.x  = UnityEngine.Random.Range(-90.0f, 90.0f);
        randomRotate.y  = UnityEngine.Random.Range(-90.0f, 90.0f);
        randomRotate.z  = UnityEngine.Random.Range(-90.0f, 90.0f);
        // ランダムな角度に移動
        randomDir       = UnityEngine.Random.Range(-180.0f, 180.0f);
        startPos        = this.transform.position;

        // 自転
        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Subscribe(_ =>
            {
                this.transform.eulerAngles += randomRotate * Time.deltaTime;

                this.transform.position = new Vector3(
                Mathf.Lerp(this.transform.position.x, startPos.x + 10 * Mathf.Cos(randomDir),0.1f),
                0.0f,
                Mathf.Lerp(this.transform.position.z, startPos.z + 10 * Mathf.Sin(randomDir), 0.1f)
                );
            })
            .AddTo(this.gameObject);
        // 自壊
        Observable.Timer(TimeSpan.FromSeconds(DeathCount))
            .Subscribe(_ =>
            {
                Destroy(this.gameObject);
            })
            .AddTo(this.gameObject);
    }
}

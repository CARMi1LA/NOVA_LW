using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEditor.ShaderGraph;

using UnityEngine.Experimental.VFX;

public class Aura : MonoBehaviour
{
    // オーラの状態
    public enum AuraState
    {
        AuraFull = 0,                  // オーラ満タン
        AuraOff,                       // オーラHP0
        AuraCharge                     // オーラ準備中
    }

    public AuraState auraState = AuraState.AuraFull;
        
    [SerializeField] private int auraHp = 0;                     // オーラのHP
    [SerializeField] public int[] auraHpLevelList;               // レベル毎のオーラの最大HPリスト
    [SerializeField] private int auraHealDelayFrame = 300;       // オーラ回復処理に掛かる遅延時間（フレーム）
    [SerializeField] private int auraHealIntervalFrame = 10;     // オーラを回復させる間隔（フレーム）
    [SerializeField] private int level = 0;                      // 現在のレベル

    [SerializeField] private Transform playerTrans;              // プレイヤーの座標

    [SerializeField] private Vector3 auraSizeMag;                // オーラの大きさの倍率
    [SerializeField] private Renderer auraRenderer;
    [SerializeField] private Material auraMat;
    [SerializeField] private Gradient a;
    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameManager.Instance.playerTransform; // プレイヤーの情報を取得
        level = GameManager.Instance.playerLevel;           // レベル情報を取得
        auraMat = auraRenderer.GetComponent<Renderer>().material;
        auraHp = auraHpLevelList[level - 1];                            // 初期のオーラHPを設定
        // オーラの大きさを設定
        transform.localScale = (playerTrans.localScale + auraSizeMag) / playerTrans.localScale.x;
        //transform.localScale = (playerTrans.localScale + auraSizeMag) / playerTrans.localScale.x;
        this.UpdateAsObservable()
            .Where(_ => level != GameManager.Instance.playerLevel)
            .Subscribe(_ => {
                level = GameManager.Instance.playerLevel;
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (auraHp <= auraHpLevelList[level - 1] * 0.5f)
                {
                    Debug.Log("KentiDanger");
                    auraMat.SetInt("_AuraFlgDanger", 1);
                }
                else if(auraHp <= auraHpLevelList[level - 1] * 0.25f)
                {
                    Debug.Log("KentiCaution");
                    auraMat.SetInt("_AuraFlgDanger", 0);
                }
                if (auraHp >= auraHpLevelList[level - 1] * 0.8f)
                {
                    Debug.Log("KentiFine");
                    auraMat.SetInt("_AuraFlgFine", 0);
                }
                else
                {
                    Debug.Log("KentiDamage");
                    auraMat.SetInt("_AuraFlgFine", 1);
                }
            }).AddTo(this.gameObject);
        
        // 通常時のイベント ポーズしてなければ稼働する
        this.UpdateAsObservable()
            .Where(c => !GameManager.Instance.isPause.Value)
            .Subscribe(c =>
            {
                if (auraHp == auraHpLevelList[level-1])
                {
                    auraState = AuraState.AuraFull;
                    this.gameObject.SetActive(true);
                }
                else if (auraHp < auraHpLevelList[level - 1])
                {
                    auraState = AuraState.AuraCharge;
                    this.gameObject.SetActive(true);
                    // ここにオーラONOFFメソッドを入れる
                    Debug.Log("KENTICHARGE");
                }
                else if (auraHp <= 0)
                {
                    auraState = AuraState.AuraOff;
                    this.gameObject.SetActive(false);
                }

            }).AddTo(this.gameObject);
        // 1でもダメージを受けていたらオーラHP回復処理を実行 1秒毎にオーラ回復
        Observable.TimerFrame(auraHealDelayFrame, auraHealIntervalFrame)
            .Where(c => !GameManager.Instance.isPause.Value)
            .Where(c => auraState == AuraState.AuraCharge || auraState == AuraState.AuraOff)
            .Where(c => auraHp != auraHpLevelList[level - 1])
            .Subscribe(c =>
            {
                int heal = 1;   // 回復量
                auraHp += heal; // オーラHPに回復量を代入
                if (auraHp == auraHpLevelList[level - 1])
                {
                    auraState = AuraState.AuraFull;
                }
                Debug.Log("回復");
            }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Subscribe(c =>
            {
                float enemySize = 0.0f;
                int damage = 0;

                // 当たってもオーラが有効でないと動作しない
                if (auraState == AuraState.AuraFull || auraState == AuraState.AuraCharge)
                {
                    try
                    {
                        // 衝突した惑星のコンポーネント取得
                        enemySize = c.gameObject.GetComponent<_StarParam>().GetStarSize();
                        // 衝突した惑星が自分より遥かに小さい場合は蒸発する。オーラのHPは【減少しない】
                        if (enemySize <= playerTrans.localScale.x / 8)
                        {
                            if (c.gameObject.GetComponent<_StarParam>().starID == 2)
                            {
                                // ボスと衝突したらボスイベントへ突入
                            }
                            else
                            {
                                c.gameObject.GetComponent<_StarParam>().playDeathFX.OnNext(0.5f);
                                Debug.Log("衝突1/8");
                            }
                        }
                        // 衝突した惑星が自分よりかなり小さい場合は蒸発する。オーラのHPは【減少する】
                        else if (enemySize <= playerTrans.localScale.x / 4)
                        {
                            if (c.gameObject.GetComponent<_StarParam>().starID == 2)
                            {
                                // ボスと衝突したらボスイベントへ突入
                            }
                            else
                            {
                                damage = 1;
                                auraHp -= damage;
                                c.gameObject.GetComponent<_StarParam>().playDeathFX.OnNext(0.5f);
                                Debug.Log("衝突1/4");
                            }
                        }
                        // 衝突した惑星が自分とほぼ同じ大きさの場合オーラは発動しない
                        else if (enemySize <= playerTrans.localScale.x * 1.1f)
                        {
                            Debug.Log("衝突等倍");
                        }
                        // 衝突した惑星が自分よりやや大きい場合、オーラのHPを消費してゲームオーバーを回避する
                        else if (enemySize <= playerTrans.localScale.x * 1.5f)
                        {
                            // 暫定消滅
                            c.gameObject.SetActive(false);
                            // レベルに応じてオーラのHPを減らす
                            damage = Mathf.RoundToInt((enemySize * level - 1) / playerTrans.localScale.x);
                            auraHp -= damage;
                            Debug.Log(damage);
                            Debug.Log("衝突1.5/1");
                        }
                        // 衝突した惑星が自分よりかなり大きい場合、オーラのHPをより多く消費してゲームオーバーを回避する
                        else if (enemySize <= playerTrans.localScale.x * 2.0f)
                        {
                            // 暫定消滅
                            c.gameObject.SetActive(false);
                            // レベルに応じてオーラのHPを減らす
                            damage = Mathf.RoundToInt((enemySize * (2 * level - 1)) / playerTrans.localScale.x);
                            auraHp -= damage;
                            Debug.Log(damage);
                            Debug.Log("衝突2/1");
                        }
                    }
                    catch
                    {
                        Debug.Log("_StarParam Not Found");
                    }
                }
            }).AddTo(this.gameObject);
    }

}

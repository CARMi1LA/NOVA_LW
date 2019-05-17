using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Aura : MonoBehaviour
{
    // オーラの状態
    public enum AuraState
    {
        AuraOn = 0,                  // オーラ起動中
        AuraOff,                     // オーラ未起動
        AuraCharge                   // オーラ準備中
    }

    public AuraState auraState = AuraState.AuraOn;

    [SerializeField] private int auraHp = 0; // オーラのHP
    [SerializeField] public int[] auraHpLevelList; // レベル毎のオーラの最大HPリスト
    [SerializeField] private int auraStatingInterval = 10;

    [SerializeField] private int level = 0;

    [SerializeField] private float auraSizeMag = 1.25f;

    [SerializeField] private Transform playerTrans;

    private void Awake()
    {
        playerTrans = GameManager.Instance.playerTransform;
        level = GameManager.Instance.playerLevel;
        auraHp = SetAura(level);
        transform.localScale = playerTrans.localScale * auraSizeMag;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Where(c => GameManager.Instance.isPause.Value)
            .Subscribe(c =>
            {
                if (auraState == AuraState.AuraOn && auraHp <= 0)
                {
                    auraState = AuraState.AuraOff;
                }
                if (auraState == AuraState.AuraOff)
                {
                    AuraRecharge();
                    auraState = AuraState.AuraCharge;
                }
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(c => GameManager.Instance.isPause.Value)
            .Where(c => auraState == AuraState.AuraCharge)
            .Subscribe(c =>
            {

            }).AddTo(this.gameObject);

        this.OnCollisionEnterAsObservable()
            .Subscribe(c =>
            {
                float enemySize = 0.0f;
                // 当たってもオーラが有効でないと動作しない
                if (auraState == AuraState.AuraOn)
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
                                c.gameObject.SetActive(false);
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
                                c.gameObject.SetActive(false);
                            }
                        }
                        // 衝突した惑星が自分とほぼ同じ大きさの場合オーラは発動しない
                        else if (enemySize <= playerTrans.localScale.x * 1.1f)
                        {

                        }
                        // 衝突した惑星が自分よりやや大きい場合、オーラのHPを消費してゲームオーバーを回避する
                        else if (enemySize <= playerTrans.localScale.x * 1.5f)
                        {

                        }
                        // 衝突した惑星が自分よりかなり大きい場合、オーラのHPをより多く消費してゲームオーバーを回避する
                        else if (enemySize <= playerTrans.localScale.x * 2.0f)
                        {

                        }
                    }
                    catch
                    {
                        Debug.Log("_StarParam Not Found");
                    }
                }
            }).AddTo(this.gameObject);
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private int SetAura(int level)
    {
        int newHp = 0;
        newHp = auraHpLevelList[level - 1];
        return newHp;
    }

    private void AuraRecharge()
    {

    }
}

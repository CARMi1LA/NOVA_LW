using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using Random = UnityEngine.Random; 

public class DropItemManager : MonoBehaviour
{
    // ドロップしたアイテムのスクリプト

    // アイテムの種類
    public enum ItemType
    {
        Score = 0,      // スコア
        Life,           // 回復
        Energy          // エネルギー
    }

    public int itemScore;   // アイテムのスコア
    public int itemLife;    // HP回復量
    public int itemEnergy;  // エネルギー回復量

    [SerializeField] private ItemType itemType;

    // アイテムの向き
    [SerializeField] private Vector3 itemRot = Vector3.zero;
    // 飛び出すアイテムの角度
    [SerializeField] private float itemDir = 0.0f;
    // 生成元の座標
    [SerializeField] private Vector3 originPos;

    // プレイヤーの座標
    [SerializeField] private Transform playerTrans;
    // アイテムのrigidbody
    [SerializeField] private Rigidbody itemRigid;

    // アイテムが吸い込まれる距離
    [SerializeField] private float maxDistance;
    // プレイヤーとの距離
    [SerializeField] private FloatReactiveProperty distance;


    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                // 常にプレイヤーとの距離を計算する
                distance.Value = (playerTrans.position - this.transform.position).sqrMagnitude;
                // プレイヤーとの距離が近くなったらプレイヤーに引き寄せられる処理を行う
                if (distance.Value <= Mathf.Pow(maxDistance, 2))
                {
                    Vector3 lerp = Vector3.Lerp(playerTrans.position,this.transform.position,  1.0f);
                    this.transform.position = Vector3.Lerp(playerTrans.position, this.transform.position, 0.75f);
                }
                // それ以外は生成元座標からランダムな方向へ一定距離進んだ後、回転しつづける
                else
                {
                    this.transform.eulerAngles += itemRot * Time.deltaTime;

                    this.transform.position = new Vector3(
                        Mathf.Lerp(this.transform.position.x, originPos.x + 10 * Mathf.Cos(itemDir), 0.1f),
                        0,
                        Mathf.Lerp(this.transform.position.z, originPos.z + 10 * Mathf.Sin(itemDir), 0.1f)
                        );

                    itemRigid.velocity *= 0.99f;
                }

            }).AddTo(this.gameObject);
    }

    // アイテム消滅処理
    public void ItemDestroy()
    {
        // このオブジェクトを非表示にする
        this.gameObject.SetActive(false);
        ItemSpawner.Instance.ItemRemove(this);
    }

    // アイテム生成メソッド、各種パラメータの初期設定を行う
    public void CreateItem(int score,int hp,int energy,ItemType type,Vector3 pos)
    {
        // オブジェクトを表示させる
        this.gameObject.SetActive(true);
        // プレイヤーの座標を取得
        playerTrans = GameManagement.Instance.playerTransform;

        // ランダムに回転量を取得する
        itemRot.x = Random.Range(-90.0f, 90.0f);
        itemRot.y = Random.Range(-90.0f, 90.0f);
        itemRot.z = Random.Range(-90.0f, 90.0f);

        // アイテムの射出方向を取得する
        itemDir = Random.Range(-180.0f, 180.0f);

        // 各種パラメータを設定する
        itemScore = score;
        itemLife = hp;
        itemEnergy = energy;
        itemType = type;

        // 生成元座標の設定
        originPos = pos;
        // 初期座標の設定
        this.transform.position = pos;

    }

}

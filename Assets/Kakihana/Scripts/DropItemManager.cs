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
    [SerializeField] private float itemDir = 0.0f;
    [SerializeField] private Vector3 originPos;

    [SerializeField] private Transform playerTrans;
    [SerializeField] private Rigidbody itemRigid;

    [SerializeField] private float maxDistance;
    [SerializeField] private FloatReactiveProperty distance;


    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                distance.Value = (playerTrans.position - this.transform.position).sqrMagnitude;
                if (distance.Value <= Mathf.Pow(maxDistance, 2))
                {
                    Vector3 lerp = Vector3.Lerp(playerTrans.position,this.transform.position,  1.0f);
                    this.transform.position = Vector3.Lerp(playerTrans.position, this.transform.position, 0.75f);
                }
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

    public void ItemDestroy()
    {
        // このオブジェクトを非表示にする
        this.gameObject.SetActive(false);
        ItemSpawner.Instance.ItemRemove(this);
    }

    public void CreateItem(int score,int hp,int energy,ItemType type,Vector3 pos)
    {
        this.gameObject.SetActive(true);
        playerTrans = GameManagement.Instance.playerTransform;

        itemRot.x = Random.Range(-90.0f, 90.0f);
        itemRot.y = Random.Range(-90.0f, 90.0f);
        itemRot.z = Random.Range(-90.0f, 90.0f);

        itemDir = Random.Range(-180.0f, 180.0f);

        itemScore = score;
        itemLife = hp;
        itemEnergy = energy;
        itemType = type;

        originPos = pos;
        this.transform.position = pos;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using Random = UnityEngine.Random; 

public class DropItemManager : MonoBehaviour
{
    public enum ItemType
    {
        Score = 0,
        Life,
        Energy
    }

    public int itemScore;
    public int itemLife;
    public int itemEnergy;

    [SerializeField] private ItemType itemType;

    [SerializeField] private Vector3 itemRot = Vector3.zero;
    [SerializeField] private float itemDir = 0.0f;
    [SerializeField] private Vector3 originPos;


    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                this.transform.eulerAngles += itemRot * Time.deltaTime;

                this.transform.position = new Vector3(
                    Mathf.Lerp(this.transform.position.x, originPos.x + 10 * Mathf.Cos(itemDir), 0.1f),
                    0,
                    Mathf.Lerp(this.transform.position.z, originPos.z + 10 * Mathf.Sin(itemDir), 0.1f)
                    );
            }).AddTo(this.gameObject);
    }

    public void ItemDestroy()
    {
        ItemSpawner.Instance.ItemRemove(this);
    }

    public void CreateItem(int score,int hp,int energy,ItemType type,Vector3 pos)
    {
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

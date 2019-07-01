using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletManager : MonoBehaviour
{
    public enum ShootChara
    {
        Player = 0,
        Enemy
    }

    [SerializeField] public ShootChara shootChara;
    [SerializeField] private float shootSpeed;
    [SerializeField] private float rangeLimit;
    [SerializeField] private int bulletListIndex;
    [SerializeField] public int damageAtk;

    [SerializeField] private Transform shootOriginTrans;

    BulletManager(float speed,ShootChara shootChara)
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().AddForce(transform.up * shootSpeed, ForceMode.Impulse);

        this.UpdateAsObservable()
            //.Where(_ => !GameManager.Instance.isPause.Value)
            .Where(_ => Vector3.Distance(this.transform.position, shootOriginTrans.position) >= rangeLimit)
            .Subscribe(_ => 
            {
                BulletDestroy();
            }).AddTo(this.gameObject);
    }

    public void BulletDestroy()
    {
        gameObject.SetActive(false);
        //BulletSpawner.Instance.BulletRemove(bulletListIndex);
        //bulletSpawner.bulletDataList.RemoveAt(bulletListIndex);
    }

    public void BulletCreate(int atk, float speed, Transform origin, ShootChara chara,int index)
    {
        this.gameObject.SetActive(true);
        shootSpeed = speed;
        shootChara = chara;
        damageAtk = atk;
        shootOriginTrans = origin;
        bulletListIndex = index;
    }

}

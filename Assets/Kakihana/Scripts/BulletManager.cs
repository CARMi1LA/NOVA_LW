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
            .Where(_ => !GameManager.Instance.isPause.Value)
            .Where(_ => Vector3.Distance(this.transform.position, shootOriginTrans.position) >= rangeLimit)
            .Subscribe(_ => 
            {
                gameObject.SetActive(false);
            }).AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Subscribe(c => 
            {
                switch (shootChara)
                {
                    case ShootChara.Player:
                        EnemyManager em;
                        try
                        {
                            em = c.gameObject.GetComponent<EnemyManager>();
                        }
                        catch (System.Exception)
                        {

                            throw;
                        }
                        break;
                    case ShootChara.Enemy:
                        break;
                    default:
                        break;
                }
                gameObject.SetActive(false);
            }).AddTo(this.gameObject);
    }

    public void BulletCreate(float speed, int atk,ShootChara chara,Transform trans)
    {
        shootSpeed = speed;
        shootChara = chara;
        damageAtk = atk;
        shootOriginTrans = trans;
    }

}

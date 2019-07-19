using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class PlayerUnit : MonoBehaviour
{
    public enum UnitID
    {
        No1 = 0,
        No2
    }

    public enum Mode
    {
        Revolution = 0,
        LookAt
    }
    [SerializeField] private PlayerManager pm;

    [SerializeField] private UnitID unitId;
    [SerializeField] private Mode atkMode = Mode.Revolution;

    [SerializeField] private Transform playerTrans;
    [SerializeField] private Transform partnerTrans;
    [SerializeField] private Transform shootTarget;
    [SerializeField] private Vector3 targetPos;

    [SerializeField] private int Id;
    [SerializeField] private int level;
    [SerializeField] private int atk;

    [SerializeField] private float unitSpeed;
    [SerializeField] private float rotSpeed;
    [SerializeField] private float range;

    [SerializeField] BoolReactiveProperty shootFlg = new BoolReactiveProperty(false);

    private void Awake()
    {
        playerTrans = GameManagement.Instance.playerTransform;
    }

    // Start is called before the first frame update
    void Start()
    {

        float time = 0.0f;
        //GameManagement.Instance.onClick
        //    .Where(_ => _ == true)
        //    .Subscribe(_ => 
        //    {
        //        atkMode = Mode.LookAt;
        //    }).AddTo(this.gameObject);

        //GameManagement.Instance.onClick
        //    .Where(_ => _ == false)
        //    .Subscribe(_ =>
        //    {
        //        atkMode = Mode.Revolution;
        //    }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                time += Time.deltaTime;
                targetPos = GetTargetPos(time);

                this.transform.position = playerTrans.position + targetPos * range;
                var unitVec = (playerTrans.position - targetPos).normalized;
                unitVec.y = 0;
                this.transform.rotation = Quaternion.FromToRotation(unitVec,this.transform.position);
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(_ => GameManagement.Instance.onClick.Value == false)
            .Sample(TimeSpan.FromSeconds(0.33f))
            .Subscribe(_ => 
            {
                new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
            }).AddTo(this.gameObject);

        this.UpdateAsObservable()
        .Where(_ => GameManagement.Instance.onClick.Value == true)
        .Sample(TimeSpan.FromSeconds(0.1f))
        .Subscribe(_ =>
        {
            new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
        }).AddTo(this.gameObject);

        //this.UpdateAsObservable()
        //    .Where(_ => atkMode == Mode.Revolution)
        //    .Subscribe(_ =>
        //    {
        //        this.transform.RotateAround(playerTrans.position, Vector3.up, unitSpeed);
        //    }).AddTo(this.gameObject);

        //this.UpdateAsObservable()
        //    .Where(_ => atkMode == Mode.Revolution)
        //    .Sample(TimeSpan.FromSeconds(0.25f))
        //    .Subscribe(_ => 
        //    {
        //        new BulletData(10, 50, this.transform, BulletManager.ShootChara.Player);
        //    }).AddTo(this.gameObject);
        //this.UpdateAsObservable()
        //    .Where(_ => atkMode == Mode.LookAt)
        //    .Subscribe(_ =>
        //    {
        //        if (shootFlg.Value == false)
        //        {
        //            switch(unitId)
        //            {
        //                case UnitID.No1:

        //                    break;
        //                case UnitID.No2:
        //                    break;
        //            }
        //        }
        //    }).AddTo(this.gameObject);

        //this.UpdateAsObservable()
        //    .Where(_ => unitId == UnitID.No1)
        //    .Subscribe(_ =>
        //    {

        //    }).AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(_ => unitId == UnitID.No2)
            .Subscribe(_ =>
            {

            }).AddTo(this.gameObject);

        //pm.level.Subscribe(_ =>
        //{
 
        //}).AddTo(this.gameObject);
    }

    Vector3 GetTargetPos(float time)
    {
        Vector3 tp = Vector3.zero;
        float deg = 0;

        if (GameManagement.Instance.onClick.Value == false)
        {
            if(unitId == UnitID.No1)
            {
                tp = new Vector3(Mathf.Cos(time * rotSpeed), 0.0f, Mathf.Sin(time * rotSpeed));
            }
            else
            {
                tp = new Vector3(Mathf.Cos((time * rotSpeed) + (Mathf.Deg2Rad * 180.0f)), 0.0f, Mathf.Sin((time * rotSpeed) + (Mathf.Deg2Rad * 180.0f)));
            }
        }
        else
        {
            deg = GetAngleXZ(playerTrans.position, pm.cWorld);
            tp = new Vector3(Mathf.Cos(Mathf.Deg2Rad * deg), 0.0f, Mathf.Sin(Mathf.Deg2Rad * deg));
        }
        return tp;
    }

    float GetAngleXZ(Vector3 origin,Vector3 target)
    {
        Vector3 dis = target - origin;
        float radian = Mathf.Atan2(dis.z, dis.x);
        float deg = radian * Mathf.Rad2Deg;

        return deg;
    }
}

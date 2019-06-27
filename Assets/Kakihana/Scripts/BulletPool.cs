using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Toolkit;

public class BulletPool : ObjectPool<BulletManager>
{
    public readonly BulletManager bulletObj;
    private Transform myTrans;

    BulletPool(BulletManager bm,Transform trans)
    {
        bulletObj = bm;
        myTrans = trans;
    }

    protected override BulletManager CreateInstance()
    {
        var e = GameObject.Instantiate(bulletObj);
        e.transform.SetParent(myTrans);

        return e;
    }
}

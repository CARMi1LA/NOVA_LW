using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BulletSpawner : BMSingleton<BulletSpawner>
{
    // 弾生成管理クラス

    [SerializeField] private int bulletValueMax;        // 弾の最大生成数
    [SerializeField] private BulletManager[] bulletObj;
    [SerializeField] private BulletPool bulletPool;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
}

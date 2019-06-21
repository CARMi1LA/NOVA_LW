using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : EnemyStatus
{
    [SerializeField] EnemyDataList enemyDataList;
    [SerializeField] EnemyStatus myStatus;
    EnemyManager(int Id, int Level)
    {
        enemyDataList = Resources.Load<EnemyDataList>(string.Format("Enemy{0}", Id));
        myStatus = enemyDataList.EnemyStatusList[Level - 1];

        #region 初期化処理 ステータス名 = myStatus.各ステータス
        charaType = myStatus.charaType;
        atkType = myStatus.atkType;
        hp = myStatus.hp;
        barrier = myStatus.barrier;
        atk = myStatus.atk;
        exp = myStatus.exp;
        moveSpeed = myStatus.moveSpeed;
        #endregion 
    }
}

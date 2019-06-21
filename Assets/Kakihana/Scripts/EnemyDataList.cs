using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
  fileName = "EnemyDataList",
  menuName = "ScriptableObject/EnemyDataList",
  order = 0)
]
public class EnemyDataList : ScriptableObject
{

    //ListステータスのList
    public List<EnemyStatus> EnemyStatusList = new List<EnemyStatus>();

}

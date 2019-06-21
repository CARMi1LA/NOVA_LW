using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerManager : EnemyStatus
{
    public enum SpAttackType
    {
        Explosive = 0,
        Lazer = 1,
        None = 2
    }

    [SerializeField] PlayerUnit[] playerUnits;      // 子機の情報
    [SerializeField] EnemyDataList playerDataList;  // データが格納されているリスト
    [SerializeField] EnemyStatus myStatus;          // 各種パラメータの情報

    [SerializeField] private SpAttackType spAtk = SpAttackType.None;

    [SerializeField] private float maxHp = 0;

    [SerializeField] private IntReactiveProperty level;

    [SerializeField] private IntReactiveProperty unitValue = new IntReactiveProperty(1);

    public IntReactiveProperty score = new IntReactiveProperty(0);
    PlayerManager()
    {

    }

    void Awake()
    {
        level = GameManagement.Instance.playerLevel;
    }

    // Start is called before the first frame update
    void Start()
    {
        level.Subscribe(_ =>
        {
            unitValue.Value++;
        });

        if (true)
        {
            level.Value++;
        }
    }
}

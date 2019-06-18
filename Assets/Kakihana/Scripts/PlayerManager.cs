using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerManager : EnemyStatus,IDamage,IMove
{
    public enum SpAttackType
    {
        Explosive = 0,
        Lazer = 1,
        None = 2
    }

    [SerializeField] PlayerUnit[] playerUnits;
    [SerializeField] private SpAttackType spAtk = SpAttackType.None;
    [SerializeField] NormalAtkType atkType = NormalAtkType.Burst;

    [SerializeField] private IntReactiveProperty level;

    [SerializeField] private IntReactiveProperty unitValue = new IntReactiveProperty(1);

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
        }).AddTo(this.gameObject);

        if (true)
        {
            level.Value++;
        }
    }

    void IDamage.HitDamage(int damage)
    {

    }

    void IMove.Move(float speed, float speedMul)
    {

    }
}

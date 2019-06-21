using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerUnit : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;
    [SerializeField] private Transform partnerTrans;

    [SerializeField] private int Id;
    [SerializeField] private int level;
    [SerializeField] private int atk;

    [SerializeField] private float unitSpeed;
    [SerializeField] private float unitSpeedMul;
    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameManagement.Instance.playerTransform;
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
            }).AddTo(this.gameObject);
    }

    void SetAtk(int playerAtk)
    {
        atk = playerAtk;
    }
}
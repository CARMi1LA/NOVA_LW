﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

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

        pm.level.Subscribe(_ =>
        {
 
        }).AddTo(this.gameObject);
    }
}
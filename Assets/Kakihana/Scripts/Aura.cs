using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Aura : PlayerSystem
{
    // オーラの状態
    public enum AuraState
    {
        AuraOn = 0,                  // オーラ起動中
        AuraOff,                     // オーラ未起動
        AuraPre                      // オーラ準備中
    }

    [SerializeField] private int auraHp = 0; // オーラのHP
    [SerializeField] private int auraStatingInterval = 10;


    private void Awake()
    {
        auraHp =
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private int SetAura()
    {

    }
}

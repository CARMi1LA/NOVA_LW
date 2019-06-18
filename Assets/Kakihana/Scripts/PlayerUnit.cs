using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;

    [SerializeField] private int Id;

    [SerializeField] private int atk;
    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameManagement.Instance.playerTransform;
    }

    void SetAtk(int playerAtk)
    {
        atk = playerAtk;
    }
}

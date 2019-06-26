using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Bullet : MonoBehaviour
{
    // 弾を発射したキャラクターリスト
    public enum ShootCharactor
    {
        Player = 0,
        Enemy
    }

    [SerializeField] ShootCharactor shootCharactor;
    [SerializeField] private float shootSpeed;
    [SerializeField] private int atk;

    [SerializeField] private Vector3 originPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CreateBullet(Vector3 dir,float speed,int atk, ShootCharactor shootChar)
    {

    }
}

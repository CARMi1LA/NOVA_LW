using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXCollisionEnemy : MonoBehaviour
{
    // エネミーの当たり判定時のエフェクト
    // 生成後に一定時間で自壊する

    [SerializeField, Header("自動で削除")]
    public float deathCount = 1.0f;

    void Start()
    {
        Destroy(this.gameObject, deathCount);
    }
}

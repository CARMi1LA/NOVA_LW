using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Razer : MonoBehaviour
{
    LineRenderer line;

    float speed = 2;
    float range = 10;

    Vector3 moveDir;

    void Start()
    {
        
    }

    public void SetMoveDir(Vector3 dir)
    {
        moveDir = dir;
    }
}

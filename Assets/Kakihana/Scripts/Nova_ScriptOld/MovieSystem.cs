using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class MovieSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Observable.TimerFrame(1600).Subscribe(c =>
        {
            this.gameObject.SetActive(false);
        }).AddTo(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

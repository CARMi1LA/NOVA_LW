using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float speed = 50.0f;

    float deathRange = 150.0f;

    void Start()
    {
        this.GetComponent<Rigidbody>().AddForce(transform.up * speed, ForceMode.Impulse);

        this.UpdateAsObservable()
            .Where(x => !GameManager.Instance.isPause.Value)
            .Where(x => Vector3.Distance(this.transform.position, GameManager.Instance.playerTransform.position) >= deathRange)
            .Subscribe(_ =>
            {
                Destroy(this.gameObject);
            })
            .AddTo(this.gameObject);

        this.OnTriggerEnterAsObservable()
            .Subscribe(_ =>
            {
                if(_.gameObject.tag != "Bullet")
                {
                    Destroy(this.gameObject);
                }
            })
            .AddTo(this.gameObject);
    }
}

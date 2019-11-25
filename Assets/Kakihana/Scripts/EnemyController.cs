using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Experimental.VFX;

public class EnemyController : MonoBehaviour
{

    public Vector3 rot;
    public int score;
    // 回転スピード
    [SerializeField] private float rotSpeed;
    [SerializeField] GameObject destroyEffect;

    // Start is called before the first frame update
    void Start()
    {
        rot = new Vector3(
            Random.Range(-5.0f, 5.0f),
            Random.Range(-5.0f, 5.0f),
            Random.Range(-5.0f, 5.0f)
            );

        this.UpdateAsObservable()
            .Subscribe(_ => 
            {
                this.transform.localEulerAngles += (rot * rotSpeed * Time.deltaTime);
            }).AddTo(this.gameObject);

        this.OnTriggerExitAsObservable()
            .Where(c => c.gameObject.tag == "Pulse")
            .Subscribe(c => 
            {
                EnemyDestroy();
            }).AddTo(this.gameObject);
    }

    public void EnemyDestroy()
    {
        Instantiate(destroyEffect, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}

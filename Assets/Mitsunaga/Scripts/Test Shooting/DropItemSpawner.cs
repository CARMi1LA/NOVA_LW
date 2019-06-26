using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class DropItemSpawner : MonoBehaviour
{
    [SerializeField]
    DropItem itemPrefab;
    [SerializeField]
    int itemSpawnRate;

    Subject<int> itemSpawnSubject = new Subject<int>();

    void Start()
    {
        itemSpawnSubject
            .Subscribe(Rate =>
            {

                GameObject[] items = new GameObject[Rate];
                GameObject item = null;

                for (int i = 0; i < Rate; ++i)
                {
                    Debug.Log("Spawn Item !!");

                    item = Instantiate(itemPrefab.gameObject);
                    item.transform.position = this.transform.position;

                }
            })
            .AddTo(this.gameObject);

        // キーを押された時にアイテムスポーンを実行する
        this.UpdateAsObservable()
            .Where(x => Input.GetKeyDown(KeyCode.S))
            .Where(x => !GameManager.Instance.isPause.Value)
            .Subscribe(_ =>
            {
                itemSpawnSubject.OnNext(itemSpawnRate);
            })
            .AddTo(this.gameObject);
    }

}

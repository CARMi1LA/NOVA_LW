using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;
using System;

public class PlanetDestroy : MonoBehaviour
{
    
    [SerializeField] PlanetSpawner spawner; // 惑星が持つスクリプト 30秒経つと消える
    public Transform myTrans;               // 自身のトランスフォーム
    string sceneName;                       // スポーン時点でのシーン名
    float time;                             // スポーンしてからの経過時間

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(true);
        // スポーン時にシーン名を取得
        sceneName = PlanetSpawner.Instance.NowSceneName();
        // シーンが切り替わってもDestroyしない問題を解決
        if (sceneName != SceneManager.GetActiveScene().name)
        {
            Destroy(this.gameObject);
        }
        
    }
    // 惑星スポーンの座標設定
    public void PlanetSpawn(Vector3 pos)
    {
        transform.position = pos;
    }

    // 惑星スポーンの設定（オーバーロード、スケール値追加）
    public void PlanetSpawn(Vector3 pos,float scale)
    {
        transform.position = pos;
        transform.localScale = new Vector3(scale,scale,scale);
    }
    // 消滅情報をスポーンクラスに送る
    public void Stop()
    {
        PlanetSpawner.Instance.PlanetDestroy();
    }
}

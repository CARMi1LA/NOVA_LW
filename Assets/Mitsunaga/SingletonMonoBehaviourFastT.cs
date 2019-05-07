using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMBGameManager<T> : MonoBehaviour where T : SingletonMBGameManager<T>
{
    // シングルトンの実装
    // T.instance.変数名　で変数などを参照できるっぽい
    // http://tsubakit1.hateblo.jp/entry/20140709/1404915381
    
    // 実装先のゲームオブジェクトのタグを設定しておく
    protected static readonly string[] findTags =
    {
        "GameController",
    };

    protected static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {

                Type type = typeof(T);

                foreach (var tag in findTags)
                {
                    GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

                    for (int j = 0; j < objs.Length; j++)
                    {
                        instance = (T)objs[j].GetComponent(type);
                        if (instance != null)
                            return instance;
                    }
                }

                Debug.LogWarning(string.Format("{0} is not found", type.Name));
            }

            return instance;
        }
    }

    virtual protected void Awake()
    {
        CheckInstance();
    }

    // 初期設定
    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = (T)this;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }

        Destroy(this.gameObject);
        return false;
    }
}

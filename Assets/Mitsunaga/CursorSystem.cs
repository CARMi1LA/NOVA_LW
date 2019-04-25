using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CursorSystem : MonoBehaviour
{
    // マウスカーソルの処理

    [SerializeField, Header("ブラックホールとホワイトホール")]
    GameObject bHole;
    [SerializeField]
    GameObject wHole;

    bool cursorFlg = false;
    void Awake()
    {
    }
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(c =>
            {
                // マウスカーソルの座標を取得、変換し、ポジションに適用
                Vector3 cScreen = Input.mousePosition;
                cScreen.z = (GameManager.Instance.cameraPosition -
                             GameManager.Instance.playerPosition).z;
                this.transform.position = Camera.main.ScreenToWorldPoint(cScreen);

                // マウスがクリックされている間、ホールを切り替える
                if (Input.GetMouseButton(0))
                {
                    GameManager.Instance.cursorFlg = false;

                    bHole.SetActive(false);
                    wHole.SetActive(true);
                }
                else
                {
                    GameManager.Instance.cursorFlg = true;

                    bHole.SetActive(true);
                    wHole.SetActive(false);
                }

                GameManager.Instance.cursorPos = this.gameObject.transform.position;
            })
            .AddTo(this.gameObject);

    }
}

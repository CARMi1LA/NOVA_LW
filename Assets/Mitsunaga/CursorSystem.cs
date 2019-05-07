using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using UnityEngine.VFX;
using UnityEditor.VFX;
using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Experimental.VFX;

public class CursorSystem : MonoBehaviour
{
    // マウスカーソルの処理

    [SerializeField, Header("ブラックホールとホワイトホール")]
    VisualEffect vfxHole;

    VisualEffect holeEffect;
    bool cursorFlg = false;

    const float bSize = 2.5f;
    const float wSize = 8.0f;

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

                    vfxHole.SetFloat("ConformSize", wSize);
                }
                else
                {
                    GameManager.Instance.cursorFlg = true;

                    vfxHole.SetFloat("ConformSize", bSize);
                }

                GameManager.Instance.cursorPos = this.gameObject.transform.position;
            })
            .AddTo(this.gameObject);

    }
}

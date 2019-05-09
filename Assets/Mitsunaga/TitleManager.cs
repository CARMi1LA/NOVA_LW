using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class TitleManager : MonoBehaviour
{
    [SerializeField, Header("点滅させるテキスト")]
    Text startText;

    // Start is called before the first frame update
    void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                startText.color = new Color(1, 1, 1, (Mathf.Sin(Time.realtimeSinceStartup) + 1.0f) / 2);
            })
            .AddTo(this.gameObject);

        this.UpdateAsObservable()
            .Where(x => Input.GetMouseButtonDown(0))
            .Subscribe(_ =>
            {
                GameManager.Instance.FadeOut("02 StageSelect");
            })
            .AddTo(this.gameObject);
    }
}

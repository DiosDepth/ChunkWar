using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class ShipHUDMessagePanel : MonoBehaviour
{
    private TextMeshProUGUI _topMsgText;
    private CanvasGroup _topCanvas;

    public void Awake()
    {
        _topCanvas = transform.Find("TopMessage").SafeGetComponent<CanvasGroup>();
        _topMsgText = transform.Find("TopMessage/Text").SafeGetComponent<TextMeshProUGUI>();

        _topCanvas.alpha = 0;
    }

    public async void ShowTopMessage(string content)
    {
        _topMsgText.text = content;
        var anim = _topCanvas.transform.SafeGetComponent<Animation>();
        anim.Play();
        await UniTask.Delay(2000);
        _topCanvas.alpha = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if GMDEBUG
public class GMTalkInputItemCmpt : MonoBehaviour
{
    private InputField _filed;

    public enum GMTalkInputType
    {
        addPlug,
        addWreckage,
    }

    public GMTalkInputType InputType = GMTalkInputType.addPlug;

    private void Awake()
    {
        _filed = transform.Find("Input").SafeGetComponent<InputField>();
        transform.Find("Button").SafeGetComponent<Button>().onClick.AddListener(SendGMBtnPressed);
    }

    private void SendGMBtnPressed()
    {
        string key = InputType.ToString();
        var content = string.Format("{0} {1}", key, _filed.text);
        GMTalkManager.Instance.HandleGMTalkInputContent(content);
    }
}

#endif
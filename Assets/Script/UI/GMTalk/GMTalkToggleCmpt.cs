using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if GMDEBUG
public class GMTalkToggleCmpt : MonoBehaviour
{
    public int ToggleKey;
    public string GM_KEY_ON;
    public string GM_KEY_OFF;

    private Toggle toggle;

    public void Awake()
    {
        toggle = transform.SafeGetComponent<Toggle>();
        toggle.isOn = GMTalkManager.Instance.GetToggleCacheValue(ToggleKey);
        toggle.onValueChanged.AddListener(OnToggleChange);
    }

    private void OnToggleChange(bool value)
    {
        if (value)
        {
            GMTalkManager.Instance.HandleGMTalkInputContent(GM_KEY_ON);
        }
        else
        {
            GMTalkManager.Instance.HandleGMTalkInputContent(GM_KEY_OFF);
        }
    }

    private void OnDestroy()
    {
        GMTalkManager.Instance.AddToggleStorage(ToggleKey, toggle.isOn);
    }
}
#endif
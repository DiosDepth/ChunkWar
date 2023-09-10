using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable
{
    public float duration = 0.75f;

    private static Color _colorNormal = Color.white;
    private static Color _colorCritical = Color.yellow;

    private TextMeshProUGUI _text;

    public override void Initialization()
    {
        base.Initialization();
        _text = transform.Find("uiGroup/Textinfo").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Show()
    {
 
        StartCoroutine(MonoManager.Instance.Delay(duration, () => 
        {
            Hidden();
        }));
    }

    public override void Hidden()
    {
        base.Hidden();
        PoolableDestroy();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
       
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    public void SetText(string text, bool isCritical)
    {
        _text.SetText(text);
        _text.color = isCritical ? _colorCritical : _colorNormal;
    }

    public void SetText(float value, bool isCritical)
    {
        _text.SetText(value.ToString());
        _text.color = isCritical ? _colorCritical : _colorNormal;
    }

}

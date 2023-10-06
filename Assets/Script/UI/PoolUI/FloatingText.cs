using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable
{
    public float duration = 0.75f;

    private static UnityEngine.Color _colorNormal = UnityEngine.Color.white;
    private static UnityEngine.Color _colorCritical = UnityEngine.Color.yellow;

    private static float _defaultTextSize = 16;
    private TextMeshProUGUI _text;

    protected override void Awake()
    {
        base.Awake();
        
    }

    public override void Initialization()
    {
        base.Initialization();
        _text = transform.Find("uiGroup/Textinfo").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Show()
    {
        StartCoroutine(MonoManager.Instance.DelayUnSacleTime(duration, () => 
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
        _text.fontSize = _defaultTextSize;
        _text.color = _colorNormal;
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

    public void SetSize(float size)
    {
        _text.fontSize = size;
    }
    public void SetColor(UnityEngine.Color color)
    {
        _text.color = color;
    }

}

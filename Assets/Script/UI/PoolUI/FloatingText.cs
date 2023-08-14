using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable
{
    public float duration = 0.75f;

    private static Color _colorNormal = Color.white;
    private static Color _colorCritical = Color.yellow;

    public override void Initialization()
    {
        base.Initialization();

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
        var txt = GetGUIComponent<TMP_Text>("Textinfo");
        txt.SetText(text);
        txt.color = isCritical ? _colorNormal : _colorCritical;
    }

    public void SetText(float value, bool isCritical)
    {
        var txt = GetGUIComponent<TMP_Text>("Textinfo");
        txt.SetText(value.ToString());
        txt.color = isCritical ? _colorNormal : _colorCritical;
    }

}

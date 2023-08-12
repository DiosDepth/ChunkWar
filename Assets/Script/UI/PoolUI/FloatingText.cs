using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : GUIBasePanel,IPoolable
{
    public float duration = 0.75f;

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

    public void SetText(string text)
    {
        GetGUIComponent<TMP_Text>("Textinfo").SetText(text);
    }

    public void SetText(float value)
    {
        GetGUIComponent<TMP_Text>("Textinfo").SetText(value.ToString());
    }

}

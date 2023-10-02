using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemTagCmpt : MonoBehaviour, IPoolable
{
    private TextMeshProUGUI _text1;
    private TextMeshProUGUI _text2;

    public void Awake()
    {
        _text1 = transform.SafeGetComponent<TextMeshProUGUI>();
        _text2 = transform.Find("Text").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(ItemTag tag)
    {
        var tagName = GameHelper.GetItemTagName(tag);
        _text1.text = string.Format("X{0}X", tagName);
        _text2.text = tagName;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.SafeGetComponent<RectTransform>());
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}

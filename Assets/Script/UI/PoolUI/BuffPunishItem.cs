using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffPunishItem : MonoBehaviour, IPoolable
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;

    void Awake()
    {
        _icon = transform.Find("Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Name").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("ValueBG/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(PropertyDisplayConfig cfg, float value)
    {
        _icon.sprite = cfg.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);

        string mark = value >= 0 ? "+" : "-";
        string content = cfg.IsPercent ? mark + value.ToString("f0") + "%" : mark + value.ToString("f0");
        var colorCode = GameHelper.GetColorCode_PropertyHover(value, 0, cfg.ReverseColor);

        _valueText.text = string.Format("<color={0}>{1}</color>", colorCode, content);
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

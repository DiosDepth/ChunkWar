using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemPropertyCmpt : MonoBehaviour
{
    private Image _icon;
    private Text _nameText;
    private TextMeshProUGUI _valueText;

    public void Awake()
    {
        _icon = transform.Find("Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("PropertyName").SafeGetComponent<Text>();
        _valueText = transform.Find("PropertyValue").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp(PropertyModifyKey key, float value)
    {
        var targetValue = Mathf.RoundToInt(value);
        var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(key);
        if(cfg != null)
        {
            _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);
            _icon.sprite = cfg.Icon;
            _valueText.text = cfg.IsPercent ? string.Format("{0}%", targetValue) : targetValue.ToString();
        }
    }
}

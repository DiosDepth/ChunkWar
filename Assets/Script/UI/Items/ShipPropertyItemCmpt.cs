using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPropertyItemCmpt : MonoBehaviour
{
    public PropertyModifyKey PropertyKey;

    private Image _icon;
    private Text _nameText;
    private TextMeshProUGUI _valueText;

    public void Awake()
    {
        _icon = transform.Find("Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Name").SafeGetComponent<Text>();
        _valueText = transform.Find("Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public void SetUp()
    {
        var propertyData = RogueManager.Instance.MainPropertyData;
        var value = propertyData.GetPropertyFinal(PropertyKey);
        propertyData.BindPropertyChangeAction(PropertyKey, OnPropertyChangeAction);

        var targetValue = Mathf.RoundToInt(value);
        var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(PropertyKey);
        if (cfg != null)
        {
            _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);
            _icon.sprite = cfg.Icon;
            _valueText.text = cfg.IsPercent ? string.Format("{0}%", targetValue) : targetValue.ToString();
        }
    }

    public void OnDisable()
    {
        var propertyData = RogueManager.Instance.MainPropertyData;
        var value = propertyData.GetPropertyFinal(PropertyKey);
        propertyData.BindPropertyChangeAction(PropertyKey, OnPropertyChangeAction);
    }

    private void OnPropertyChangeAction()
    {
        var propertyData = RogueManager.Instance.MainPropertyData;
        var value = propertyData.GetPropertyFinal(PropertyKey);
        var targetValue = Mathf.RoundToInt(value);
        var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(PropertyKey);
        if (cfg != null)
        {
            _valueText.text = cfg.IsPercent ? string.Format("{0}%", targetValue) : targetValue.ToString();
        }
    }
}

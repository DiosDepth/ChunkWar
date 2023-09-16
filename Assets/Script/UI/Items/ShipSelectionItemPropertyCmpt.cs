using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectionItemPropertyCmpt : MonoBehaviour, IPoolable
{
    public PropertyModifyKey PropertyKey;

    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;

    private static Color blue_color = new Color(0, 0.9f, 1f);
    private static Color red_color = new Color(1, 0, 0);

    public void Awake()
    {
        _icon = transform.Find("Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("PropertyName").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("ValueContent/PropertyValue").SafeGetComponent<TextMeshProUGUI>();
    }
    public void SetUp(PropertyModifyKey key, float value, bool modifyPercent)
    {
        var targetValue = Mathf.RoundToInt(value);
        var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(key);
        if (cfg != null)
        {
            if (modifyPercent)
            {
                _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.ModifyPercentNameText);
            }
            else
            {
                _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);
            }
            _icon.sprite = cfg.Icon;
            string figure = targetValue > 0 ? "+" : "";
            _valueText.text = cfg.IsPercent ? string.Format("{0}{1}%", figure, targetValue) :
                string.Format("{0}{1}", figure, targetValue);

            Color targetColor;
            if (targetValue > 0)
            {
                targetColor = cfg.ReverseColor ? red_color : blue_color;
            }
            else
            {
                targetColor = cfg.ReverseColor ? blue_color : red_color;
            }
            _valueText.color = targetColor;
        }
    }

    public void PoolableDestroy()
    {
        PoolManager.Instance.BackObject(gameObject.name, gameObject);
    }

    public void PoolableReset()
    {
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}

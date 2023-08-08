using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPropertyModifyCmpt : MonoBehaviour,IPoolable
{
    private Image _icon;
    private Text _nameText;
    private TextMeshProUGUI _valueText;

    private static Color blue_color = new Color(0, 0.9f, 1f);
    private static Color red_color = new Color(1, 0, 0);

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
            string figure = targetValue > 0 ? "+" : "";
            _valueText.text = cfg.IsPercent ? string.Format("{0}{1}%", figure, targetValue) :
                string.Format("{0}{1}", figure, targetValue);

            Color targetColor;
            if(targetValue > 0)
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

    }

    public void PoolableReset()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}

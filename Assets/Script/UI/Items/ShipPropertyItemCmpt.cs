using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPropertyItemCmpt : MonoBehaviour, IPoolable, IHoverUIItem
{
    public PropertyModifyKey PropertyKey;

    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _valueText;
    private Transform _hoverTrans;
    private PropertyHoverItem _hoverItem;

    private static Color _normalColor = Color.white;
    private static Color _positiveColor = new Color(0f, 0.92f, 1f);
    private static Color _negativeColor = Color.red;

    public void Awake()
    {
        transform.Find("BG").SafeGetComponent<GeneralHoverItemControl>().item = this;
        _icon = transform.Find("Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Name").SafeGetComponent<TextMeshProUGUI>();
        _valueText = transform.Find("ValueBG/Value").SafeGetComponent<TextMeshProUGUI>();
        _hoverTrans = transform.Find("Hover");
        _hoverTrans.transform.SafeSetActive(false);
    }

    public void Refresh()
    {
        var propertyData = RogueManager.Instance.MainPropertyData;

        float value = 0;
        if(PropertyKey == PropertyModifyKey.TOTAL_ENERGY)
        {
            value = GameHelper.GetPlayerShipEnergyTotal();
        }
        else if (PropertyKey == PropertyModifyKey.TOTAL_LOAD)
        {
            value = RogueManager.Instance.WreckageTotalLoadValue;
        }
        else
        {
            value = propertyData.GetPropertyFinal(PropertyKey);
        }

        var targetValue = Mathf.RoundToInt(value);
        var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(PropertyKey);
        if (cfg != null)
        {
            _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.NameText);
            _icon.sprite = cfg.Icon;
            _valueText.text = cfg.IsPercent ? string.Format("{0}%", targetValue) : targetValue.ToString();

            if (targetValue > 0)
            {
                _valueText.color = cfg.ReverseColor ? _negativeColor : _positiveColor;
            }
            else if (targetValue == 0)
            {
                _valueText.color = _normalColor;
            }
            else
            {
                _valueText.color = cfg.ReverseColor ? _positiveColor : _negativeColor;
            }
        }
    }

    public void SetUp(PropertyModifyKey key)
    {
        this.PropertyKey = key;
        Refresh();

        var propertyData = RogueManager.Instance.MainPropertyData;
        propertyData.BindPropertyChangeAction(PropertyKey, OnPropertyChangeAction);
    }

    public void OnDestroy()
    {
        _hoverItem?.PoolableDestroy();
        var propertyData = RogueManager.Instance.MainPropertyData;
        propertyData.UnBindPropertyChangeAction(PropertyKey, OnPropertyChangeAction);
    }

    public void OnDisable()
    {
        _hoverItem?.PoolableDestroy();
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

    public void PoolableReset()
    {
    }

    public void PoolableDestroy()
    {
        PoolManager.Instance.BackObject(gameObject.name, gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    public void OnHoverEnter()
    {
        SoundManager.Instance.PlayUISound(SoundEventStr.Mouse_PointOver_2);
        _hoverTrans.SafeSetActive(true);
        UIManager.Instance.CreatePoolerUI<PropertyHoverItem>("PropertyHoverItem", true, E_UI_Layer.Top, null, (panel) =>
        {
            var cfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(PropertyKey);
            panel.Initialization(cfg);
            _hoverItem = panel;
        });
    }

    public void OnHoverExit()
    {
        _hoverTrans.SafeSetActive(false);
        _hoverItem?.PoolableDestroy();
    }
}

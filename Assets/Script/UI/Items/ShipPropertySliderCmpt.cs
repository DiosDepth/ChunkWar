using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPropertySliderCmpt : MonoBehaviour, IHoverUIItem
{
    public enum SliderPropertyType
    {
        HP,
        Energy,
        EXP,
        Load
    }

    public SliderPropertyType PropertyType;
    public bool PercentMode = true;

    /// <summary>
    /// ≥¨‘ÿ
    /// </summary>
    public float overlordPercent;

    private TextMeshProUGUI _valueText;
    private TextMeshProUGUI _levelText;
    private TextMeshProUGUI _value2Text;
    private Image _fillImage;
    private Image _fillImage2;

    private float LerpMaxTime = 1f;
    private float _lerpTimer = 0f;
    private bool _startLerp = false;
    private float _targetPercent;

    private IUIHoverPanel _hoverItem;

    public void Awake()
    {
        _valueText = transform.Find("Content/Info/Value").SafeGetComponent<TextMeshProUGUI>();
        _fillImage = transform.Find("Content/Slider/Fill").SafeGetComponent<Image>();

        ///Set Hover
        var hoverControl = transform.GetComponent<GeneralHoverItemControl>();
        if (hoverControl != null)
            hoverControl.item = this;

        if(PropertyType == SliderPropertyType.EXP)
        {
            _levelText = transform.Find("Content/Info/Title").SafeGetComponent<TextMeshProUGUI>();
        }
        else if (PropertyType == SliderPropertyType.HP)
        {
            _fillImage2 = transform.Find("Content/Slider/Fill_2").SafeGetComponent<Image>();
        }

        if((PropertyType == SliderPropertyType.Energy || PropertyType == SliderPropertyType.Load) && !PercentMode)
        {
            _value2Text = transform.Find("Content/Slider/Value").SafeGetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (!_startLerp)
            return;

        _lerpTimer += Time.deltaTime;
        if(_lerpTimer >= LerpMaxTime)
        {
            _startLerp = false;
            _lerpTimer = 0;
            var startvalue = _fillImage2.fillAmount;
            LeanTween.value(this.gameObject, (value) =>
            {
                _fillImage2.fillAmount = value;
            }, startvalue, _targetPercent, 0.25f);
        }
    }

    /// <summary>
    /// À¢–¬HPœ‘ æ
    /// </summary>
    public void RefreshHP()
    {
        if (PropertyType != SliderPropertyType.HP)
            return;

        var hpCmpt = GameHelper.GetPlayerShipHPComponet();
        if (hpCmpt == null)
            return;

        var maxValue = hpCmpt.MaxHP;
        var currentValue = hpCmpt.GetCurrentHP;
        _fillImage.fillAmount = hpCmpt.HPPercent;
        _targetPercent = hpCmpt.HPPercent;
        if (!_startLerp)
            _startLerp = true;
        _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
    }

    public void RefreshEXP()
    {
        if (PropertyType != SliderPropertyType.EXP)
            return;

        var mgr = RogueManager.Instance;
        if(mgr.GetCurrentShipLevel >= GameGlobalConfig.Ship_MaxLevel)
        {
            _fillImage.fillAmount = 1;
            _valueText.text = string.Format("MAX");
            return;
        }

        var maxValue = mgr.CurrentRequireEXP;
        var currentValue = mgr.GetCurrentExp;
        _fillImage.fillAmount = mgr.EXPPercent;
        _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
    }

    public void RefreshLoad()
    {
        if (PropertyType != SliderPropertyType.Load)
            return;

        var currentValue = RogueManager.Instance.WreckageTotalLoadCost;
        var maxValue = RogueManager.Instance.WreckageTotalLoadValue;
        var percent = currentValue / (float)maxValue;
        overlordPercent = percent;
        _fillImage.fillAmount = percent;
        if (PercentMode)
        {
            _valueText.text = string.Format("{0}%", (int)(percent * 100f));
        }
        else
        {
            _valueText.text = string.Format("{0}%", (int)(percent * 100f));
            _value2Text.text = string.Format("{0:F1} / {1}", currentValue, maxValue);
        }
    }

    public void RefreshEnergy()
    {
        if (PropertyType != SliderPropertyType.Energy)
            return;

        PlayerShip ship = null;
        if(ShipBuilder.instance != null)
        {
            ship = ShipBuilder.instance.editorShip;
        }
        else
        {
            ship = RogueManager.Instance.currentShip;
        }

        if (ship != null)
        {
            var currentValue = ship.CurrentUsedEnergy;
            var maxValue = ship.TotalEnergy;
            var percent = currentValue / (float)maxValue;
            overlordPercent = percent;
            _fillImage.fillAmount = percent;
            if (PercentMode)
            {
                _valueText.text = string.Format("{0}%", (int)(percent * 100f));
            }
            else
            {
                _valueText.text = string.Format("{0}%", (int)(percent * 100f));
                _value2Text.text = string.Format("{0} / {1}", currentValue, maxValue);
            }
        }
    }

    public void RefreshLevelUp()
    {
        if (PropertyType != SliderPropertyType.EXP)
            return;
        var level = RogueManager.Instance.GetCurrentShipLevel;
        _levelText.text = string.Format("Lv.{0}", level);
    }

    public void OnHoverEnter()
    {
        SoundManager.Instance.PlayUISound(SoundEventStr.Mouse_PointOver_2);
        UIManager.Instance.CreatePoolerUI<PunishHoverItem>("PunishHoverItem", true, E_UI_Layer.Top, null, (panel) =>
        {
            panel.Initialization(PropertyType);
            _hoverItem = panel;
        });
    }

    public void OnHoverExit()
    {
        if(_hoverItem != null)
        {
            _hoverItem.PoolableDestroy();
        }
    }
}

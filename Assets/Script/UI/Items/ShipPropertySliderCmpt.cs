using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPropertySliderCmpt : MonoBehaviour
{
    public enum SliderPropertyType
    {
        HP,
        Energy,
        EXP,
        Load
    }

    public SliderPropertyType PropertyType;

    private TextMeshProUGUI _valueText;
    private TextMeshProUGUI _levelText;
    private Image _fillImage;
    private Image _fillImage2;

    private float LerpMaxTime = 1f;
    private float _lerpTimer = 0f;
    private bool _startLerp = false;
    private float _targetPercent;

    public void Awake()
    {
        _valueText = transform.Find("Content/Info/Value").SafeGetComponent<TextMeshProUGUI>();
        _fillImage = transform.Find("Content/Slider/Fill").SafeGetComponent<Image>();
        if(PropertyType == SliderPropertyType.EXP)
        {
            _levelText = transform.Find("Content/Info/Title").SafeGetComponent<TextMeshProUGUI>();
        }
        else if (PropertyType == SliderPropertyType.HP)
        {
            _fillImage2 = transform.Find("Content/Slider/Fill_2").SafeGetComponent<Image>();
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

    public void RefreshProperty()
    {
        int maxValue = 0;
        int currentValue = 0;

        var mgr = RogueManager.Instance;
        if(PropertyType == SliderPropertyType.HP)
        {
            var hpCmpt = GameHelper.GetPlayerShipHPComponet();
            if (hpCmpt == null)
                return;

            maxValue = hpCmpt.MaxHP;
            currentValue = hpCmpt.GetCurrentHP;
            _fillImage.fillAmount = hpCmpt.HPPercent;
            _fillImage2.fillAmount = hpCmpt.HPPercent;
            _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
        }
        else if (PropertyType == SliderPropertyType.EXP)
        {
            maxValue = mgr.CurrentRequireEXP;
            currentValue = mgr.GetCurrentExp;
            _fillImage.fillAmount = mgr.EXPPercent;
            _levelText.text = string.Format("Lv.{0}", mgr.GetCurrentShipLevel);
            _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
        }
        else if (PropertyType == SliderPropertyType.Energy)
        {
            var currentShip = RogueManager.Instance.currentShip;
            if(currentShip != null)
            {
                currentValue = currentShip.CurrentUsedEnergy;
                maxValue = currentShip.TotalEnergy;
                _fillImage.fillAmount = currentValue / (float)maxValue;
                _valueText.text = string.Format("{0}%", (int)(currentValue / (float)maxValue));
            }

        }
        else if (PropertyType == SliderPropertyType.Load)
        {

        }
    }

    /// <summary>
    /// Ë¢ÐÂHPÏÔÊ¾
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
        var maxValue = mgr.CurrentRequireEXP;
        var currentValue = mgr.GetCurrentExp;
        _fillImage.fillAmount = mgr.EXPPercent;
        _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
    }

    public void RefreshEnergy()
    {
        if (PropertyType != SliderPropertyType.Energy)
            return;

        var currentShip = RogueManager.Instance.currentShip;
        if (currentShip != null)
        {
            var currentValue = currentShip.CurrentUsedEnergy;
            var maxValue = currentShip.TotalEnergy;
            _fillImage.fillAmount = currentValue / (float)maxValue;
            _valueText.text = string.Format("{0} / {1}", currentValue, maxValue);
        }
        
    }

    public void RefreshLevelUp()
    {
        if (PropertyType != SliderPropertyType.EXP)
            return;
        var level = RogueManager.Instance.GetCurrentShipLevel;
        _levelText.text = string.Format("Lv.{0}", level);
    }
}

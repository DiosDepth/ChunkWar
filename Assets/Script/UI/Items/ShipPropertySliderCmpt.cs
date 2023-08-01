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
        Shield,
        EXP
    }

    public SliderPropertyType PropertyType;

    private TextMeshProUGUI _valueText;
    private TextMeshProUGUI _levelText;
    private Image _fillImage;
    private Image _fillImage2;

    public void Awake()
    {
        _valueText = transform.Find("Content/Info/Value").SafeGetComponent<TextMeshProUGUI>();
        _fillImage = transform.Find("Content/Slider/Fill").SafeGetComponent<Image>();
        if(PropertyType == SliderPropertyType.EXP)
        {
            _levelText = transform.Find("Content/Info/Title").SafeGetComponent<TextMeshProUGUI>();
        }
        else
        {
            _fillImage2 = transform.Find("Content/Slider/Fill_2").SafeGetComponent<Image>();
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
        }
        else if (PropertyType == SliderPropertyType.EXP)
        {
            maxValue = mgr.CurrentRequireEXP;
            currentValue = mgr.GetCurrentExp;
            _fillImage.fillAmount = mgr.EXPPercent;
            _levelText.text = string.Format("Lv.{0}", mgr.GetCurrentShipLevel);
        }

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

    public void RefreshLevelUp()
    {
        if (PropertyType != SliderPropertyType.EXP)
            return;
        var level = RogueManager.Instance.GetCurrentShipLevel;
        _levelText.text = string.Format("Lv.{0}", level);
    }
}

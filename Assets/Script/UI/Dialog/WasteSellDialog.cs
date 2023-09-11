using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WasteSellDialog : GUIBasePanel
{
    private Slider _slider;
    private TextMeshProUGUI _loadValueText;
    private TextMeshProUGUI _sliderText;
    private TextMeshProUGUI _sellPriceText;

    private int currentSellCount;
    private int maxSellCount;
    private float wasteLoad_per;
    private float shipLoadCurrent;
    private float shipLoadMax;

    protected override void Awake()
    {
        base.Awake();
        _slider = transform.Find("Content/SliderContent/Slider").SafeGetComponent<Slider>();
        _loadValueText = transform.Find("Content/LoadText/Value").SafeGetComponent<TextMeshProUGUI>();
        _sliderText = transform.Find("Content/SliderContent/ValueMax").SafeGetComponent<TextMeshProUGUI>();
        _sellPriceText = transform.Find("Content/CurrencyGain/Value").SafeGetComponent<TextMeshProUGUI>();

        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(CloseDialog);

        var reduceBtn = transform.Find("Content/SliderContent/ReduceButton").SafeGetComponent<LongClickButton>();
        reduceBtn.ClickAction = OnReduceButtonClick;
        reduceBtn.onClick.AddListener(OnReduceButtonClick);

        var addBtn = transform.Find("Content/SliderContent/AddButton").SafeGetComponent<LongClickButton>();
        addBtn.ClickAction = OnAddButtonClick;
        addBtn.onClick.AddListener(OnAddButtonClick);
        GetGUIComponent<Button>("SellButton").onClick.AddListener(OnSellButtonClick);
    }

    public override void Initialization()
    {
        base.Initialization();
        wasteLoad_per = DataManager.Instance.battleCfg.Drop_Waste_LoadCostBase;
        shipLoadCurrent = RogueManager.Instance.WreckageTotalLoadCost;
        shipLoadMax = RogueManager.Instance.WreckageTotalLoadValue;
        _slider.onValueChanged.AddListener(OnSliderValueChange);
        InitDialog();
    }

    private void InitDialog()
    {
        maxSellCount = RogueManager.Instance.GetDropWasteCount;
        transform.Find("Content/TotalText/Value").SafeGetComponent<TextMeshProUGUI>().text = maxSellCount.ToString();
        _slider.maxValue = maxSellCount;
        currentSellCount = 1;
        _slider.value = 1;
        _slider.minValue = 1;
        OnSellCountChange();
    }

    private void OnSliderValueChange(float value)
    {
        currentSellCount = (int)value;
        OnSellCountChange();
    }

    private void CloseDialog()
    {
        UIManager.Instance.HiddenUI("WasteSellDialog");
    }

    private void OnReduceButtonClick()
    {
        if (currentSellCount <= 1)
            return;

        currentSellCount--;
        _slider.value = currentSellCount;
        OnSellCountChange();
    }

    private void OnAddButtonClick()
    {
        if (currentSellCount >= maxSellCount)
            return;

        currentSellCount++;
        _slider.value = currentSellCount;
        OnSellCountChange();
    }

    private void OnSellButtonClick()
    {
        RogueManager.Instance.SellWaste(currentSellCount);
        CloseDialog();
    }

    private void OnSellCountChange()
    {
        _sliderText.text = currentSellCount.ToString();
        _sellPriceText.text = GameHelper.CalculateWasteSellPrice(currentSellCount).ToString();

        ///Calculate Load
        var loadReduce = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.WasteLoadPercent);
        float loadPercent = Mathf.Clamp(loadReduce + 1, 0, float.MaxValue);
        var wasteLoadReduce = wasteLoad_per * loadPercent * currentSellCount;
        var newLoad = shipLoadCurrent - wasteLoadReduce;

        _loadValueText.text = string.Format("{0:F1} [ {1:F1}% ]", newLoad, (newLoad / shipLoadMax) * 100f);
    }
}

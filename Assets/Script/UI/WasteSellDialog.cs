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

    private int currentSellCount;
    private int maxSellCount;

    protected override void Awake()
    {
        base.Awake();
        _slider = transform.Find("Content/SliderContent/Slider").SafeGetComponent<Slider>();
        _loadValueText = transform.Find("Content/LoadText/Value").SafeGetComponent<TextMeshProUGUI>();
        _sliderText = transform.Find("Content/SliderContent/ValueMax").SafeGetComponent<TextMeshProUGUI>();

        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(CloseDialog);
        transform.Find("Content/SliderContent/ReduceButton").SafeGetComponent<LongClickButton>().ClickAction = OnReduceButtonClick;
        transform.Find("Content/SliderContent/AddButton").SafeGetComponent<LongClickButton>().ClickAction = OnAddButtonClick;
        GetGUIComponent<Button>("SellButton").onClick.AddListener(OnSellButtonClick);
    }

    public override void Initialization()
    {
        base.Initialization();
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
    }

    private void OnSliderValueChange(float value)
    {
        _sliderText.text = ((int)value).ToString();
        currentSellCount = (int)value;
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
    }

    private void OnAddButtonClick()
    {
        if (currentSellCount >= maxSellCount)
            return;

        currentSellCount++;
        _slider.value = currentSellCount;
    }

    private void OnSellButtonClick()
    {
        CloseDialog();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipHUD : GUIBasePanel, EventListener<ShipPropertyEvent>, EventListener<RogueEvent>
{
    private TextMeshProUGUI _waveTextID;
    private TextMeshProUGUI _currencyText;
    private ShipPropertySliderCmpt _hpSliderCmpt;
    private ShipPropertySliderCmpt _shieldCmpt;
    private ShipPropertySliderCmpt _expCmpt;

    protected override void Awake()
    {
        base.Awake();
        _waveTextID = transform.Find("WaveIndex/WaveInfo/Value").SafeGetComponent<TextMeshProUGUI>();
        _currencyText = transform.Find("Currency/Currency/Value").SafeGetComponent<TextMeshProUGUI>();
        _hpSliderCmpt = transform.Find("ShipInfo/HPSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _shieldCmpt = transform.Find("ShipInfo/ShieldSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _expCmpt = transform.Find("ShipInfo/EXPSlider").SafeGetComponent<ShipPropertySliderCmpt>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<ShipPropertyEvent>();
        this.EventStartListening<RogueEvent>();
        SetUpSlider();
        RefreshCurrency();
    }

    public override void Hidden()
    {
        this.EventStopListening<ShipPropertyEvent>();
        this.EventStopListening<RogueEvent>();
        base.Hidden();
    }

    private void SetUpSlider()
    {
        _hpSliderCmpt.RefreshProperty();
        _shieldCmpt.RefreshProperty();
        _expCmpt.RefreshProperty();
    }

    public void OnEvent(ShipPropertyEvent evt)
    {
        switch (evt.type)
        {
            case ShipPropertyEventType.EXPChange:
                RefreshEXP();
                break;

            case ShipPropertyEventType.LevelUp:
                RefreshLevelUp();
                break;

            case ShipPropertyEventType.CoreHPChange:
                RefreshShipCoreHP();
                break;
        }
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.CurrencyChange:
                RefreshCurrency();
                break;
        }
    }

    private void RefreshEXP()
    {
        _expCmpt.RefreshEXP();
    }

    private void RefreshCurrency()
    {
        _currencyText.text = RogueManager.Instance.CurrentCurrency.ToString();
    }

    private void RefreshLevelUp()
    {
        _expCmpt.RefreshLevelUp();
    }

    private void RefreshShipCoreHP()
    {
        _hpSliderCmpt.RefreshHP();
    }
}

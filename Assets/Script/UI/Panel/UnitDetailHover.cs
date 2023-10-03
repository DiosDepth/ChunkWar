using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailHover : DetailHoverItemBase
{
    private TextMeshProUGUI _energyCostText;
    private TextMeshProUGUI _loadCostText;

    protected override void Awake()
    {
        base.Awake();
        
        _energyCostText = _contentRect.Find("Content/Energy/EnergyValue").SafeGetComponent<TextMeshProUGUI>();
        _loadCostText = _contentRect.Find("Content/Load/LoadValue").SafeGetComponent<TextMeshProUGUI>();
    }


    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
    }

    protected override void SetUp(int unitID)
    {
        var unitCfg = DataManager.Instance.GetUnitConfig(unitID);
        if (unitCfg == null)
            return;

        _nameText.text = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name);
        _nameText.color = GameHelper.GetRarityColor(unitCfg.GeneralConfig.Rarity);
        _rarityBG.sprite = GameHelper.GetRarityBGSprite(unitCfg.GeneralConfig.Rarity);
        _icon.sprite = unitCfg.GeneralConfig.IconSprite;
        if (!string.IsNullOrEmpty(unitCfg.GeneralConfig.Desc))
        {
            _descText.text = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Desc);
        }
        else
        {
            _descText.text = string.Empty;
        }

        _loadCostText.text = string.Format("{0:F1}", GameHelper.GetUnitLoadCost(unitCfg));
        _energyCostText.text = string.Format("{0:F1}", GameHelper.GetUnitEnergyCost(unitCfg));

        SetUpUintInfo(unitCfg);
        SetUpItemTag(unitCfg);

        base.SetUp(unitID);
    }
   
}
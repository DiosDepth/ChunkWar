using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IUIHoverPanel
{
    void PoolableDestroy();
}

public class UnitDetailHover : DetailHoverItemBase
{
    private TextMeshProUGUI _energyCostText;
    private TextMeshProUGUI _loadCostText;
    private TextMeshProUGUI _damage_Total_CurrentText;
    private TextMeshProUGUI _damage_TotalText;

    protected uint _itemUID = 0;

    protected override void Awake()
    {
        base.Awake();
        
        _energyCostText = _contentRect.Find("Content/Energy/EnergyValue").SafeGetComponent<TextMeshProUGUI>();
        _loadCostText = _contentRect.Find("Content/Load/LoadValue").SafeGetComponent<TextMeshProUGUI>();
        _damage_Total_CurrentText = _damageInfoRoot.Find("CurrentDamage/Value").SafeGetComponent<TextMeshProUGUI>();
        _damage_TotalText = _damageInfoRoot.Find("TotalDamage/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization(params object[] param)
    {
        _itemUID = (uint)param[1];
        base.Initialization(param); 
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void SetUp(int unitID, bool updatePosition = true)
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

        if (!string.IsNullOrEmpty(unitCfg.ProertyDescText))
        {
            var text = LocalizationManager.Instance.GetTextValue(unitCfg.ProertyDescText);
            _effectDesc1.text = text;
            _effectDesc2.text = text;
            _effectDesc1.transform.SafeSetActive(true);
        }
        else
        {
            _effectDesc1.transform.SafeSetActive(false);
        }

        _loadCostText.text = string.Format("{0:F1}", GameHelper.GetUnitLoadCost(unitCfg));
        _energyCostText.text = string.Format("{0:F1}", GameHelper.GetUnitEnergyCost(unitCfg));

        SetUpUintInfo(unitCfg);
        SetUpItemTag(unitCfg);
        SetUpDamageInfo(unitCfg);
        base.SetUp(unitID, updatePosition);
    }

    public override void PoolableReset()
    {
        base.PoolableReset();
        _itemUID = 0;
    }

    private void SetUpDamageInfo(BaseUnitConfig _unitCfg)
    {
        bool enable = false;
        if(_unitCfg.unitType == UnitType.MainWeapons || _unitCfg.unitType == UnitType.Weapons)
        {
            enable = true;
        }

        if (_itemUID == 0)
            enable = false;

        _damageInfoRoot.SafeSetActive(enable);
        if (!enable)
            return;

        var itemData = AchievementManager.Instance.GetOrCreateRuntimeUnitStatisticsData(_itemUID);
        _damage_Total_CurrentText.text = itemData.GetWaveDamageCurrent().ToString();
        _damage_TotalText.text = itemData.DamageCreateTotal.ToString();
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Buildings,
    Weapons,
    MainWeapons,
}

public enum OwnerShipType
{
    NONE,
    AIShip,
    AIDrone,
    PlayerShip,
    PlayerDrone
}

[System.Serializable]
public class UnitBaseAttribute
{
    /// <summary>
    /// 最大血量
    /// </summary>
    public int HPMax
    {
        get;
        protected set;
    }

    /// <summary>
    /// 能量消耗
    /// </summary>
    public int EnergyCost
    {
        get;
        protected set;
    }

    /// <summary>
    /// 能源产生
    /// </summary>
    public int EnergyGenerate
    {
        get;
        protected set;
    }

    /// <summary>
    /// 瘫痪恢复时长
    /// </summary>
    public float UnitParalysisRecoverTime
    {
        get;
        protected set;
    }

    private int BaseHP;
    private int BaseEnergyCost;
    private int BaseEnergyGenerate;
    private float BaseParalysisRecoverTime;

    /// <summary>
    /// 武器射程和建筑索敌范围
    /// </summary>
    public float BaseRange { get; protected set; }

    /// <summary>
    /// 是否玩家舰船，影响伤害计算
    /// </summary>
    protected OwnerShipType _ownerShipType;


    protected UnitPropertyData mainProperty;
    protected Unit _parentUnit;

    protected EnemyHardLevelItem _hardLevelItem;

    public virtual void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, OwnerShipType ownerType)
    {
        this._ownerShipType = ownerType;
        this._parentUnit = parentUnit;

        mainProperty = RogueManager.Instance.MainPropertyData;
        BaseHP = cfg.BaseHP;
        BaseEnergyCost = cfg.BaseEnergyCost;
        BaseEnergyGenerate = cfg.BaseEnergyGenerate;
        BaseParalysisRecoverTime = cfg.ParalysisResumeTime;
        BaseRange = cfg.BaseRange;
        if (_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);

            if (BaseEnergyCost != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitParalysisRecoverTimeRatio, CalculateUnitParalysis);
            }

            if (BaseEnergyGenerate != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            }

            CalculateHP();
            CalculateEnergyCost();
            CalculateEnergyGenerate();
            CalculateUnitParalysis();
        }
        else if(_ownerShipType == OwnerShipType.PlayerDrone)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Aircraft_HP, CalculateDroneHP);

            CalculateDroneHP();
        }
        else
        {
            //unit with no owner ,that means this is a dispersed unit;
            if (_parentUnit._owner == null)
            {
                _hardLevelItem = GameHelper.GetEnemyHardLevelItem(1);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
                CalculateEnemyHP();
            }
            else
            {
                InitEnemyHardLevelItem();
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
                CalculateEnemyHP();
            }
        }
    }

    public virtual void Destroy()
    {
        if (_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitParalysisRecoverTimeRatio, CalculateUnitParalysis);
        }
        else if(_ownerShipType == OwnerShipType.PlayerDrone)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Aircraft_HP, CalculateDroneHP);
        }
        else if(_ownerShipType == OwnerShipType.AIShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
        }
    }

    private void CalculateHP()
    {
        HPMax = GameHelper.CalculateHP(BaseHP);
    }

    private void CalculateDroneHP()
    {
        var hp = mainProperty.GetPropertyFinal(PropertyModifyKey.Aircraft_HP);
        HPMax = BaseHP + Mathf.RoundToInt(hp);
    }

    private void CalculateEnergyCost()
    {
        float rate = 0;

        var baseCfg = _parentUnit._baseUnitConfig;

        if (baseCfg.HasUnitTag(ItemTag.Weapon) || baseCfg.HasUnitTag(ItemTag.MainWeapon) || baseCfg.HasUnitTag(ItemTag.Building))
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.WeaponEnergyCostPercent);
        }
        else if (baseCfg.HasUnitTag(ItemTag.Shield))
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldEnergyCostPercent);
        }

        var totalUnitCostPercent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyCostPercent);
        rate += totalUnitCostPercent;
        ///Calculate Local Property
        var localEnergyCostPercent = _parentUnit.LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitEnergyCostPercent);
        rate += localEnergyCostPercent;
        rate = Mathf.Clamp(rate, -100, float.MaxValue);

        EnergyCost = Mathf.RoundToInt(BaseEnergyCost * (100 + rate) / 100f);

        RefreshShipEnergy();
    }

    private void CalculateEnergyGenerate()
    {
        var percent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyGenerate);
        ///Self GenerateValue
        var delta = _parentUnit.LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitEnergyGenerateValue);
        var newValue = BaseEnergyGenerate + delta;

        newValue *= (1 + percent / 100f);

        EnergyGenerate = (int)Mathf.Clamp(newValue, 0, float.MaxValue);
        RefreshShipEnergy();
    }

    private void CalculateUnitParalysis()
    {
        var percent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitParalysisRecoverTimeRatio);
        percent = Mathf.Clamp(percent, -100, float.MaxValue);
        UnitParalysisRecoverTime = (1 + percent / 100f) * BaseParalysisRecoverTime;
    }

    private void CalculateEnemyHP()
    {
        var hpPercent = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyHPPercent);
        float totalPercent = _hardLevelItem != null ? hpPercent + _hardLevelItem.HPpercentAdd : hpPercent;
        HPMax = Mathf.RoundToInt(BaseHP * (1 + totalPercent / 100f));
    }

    protected void RefreshShipEnergy()
    {
        var playerShip = RogueManager.Instance.currentShip;
        if (playerShip != null)
        {
            playerShip.RefreshShipEnergy();
        }
    }

    private void InitEnemyHardLevelItem()
    {
        var enemyShip = _parentUnit._owner as AIShip;
        if(enemyShip.OverrideHardLevelID <= 0)
        {
            _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID);
        }
        else
        {
            _hardLevelItem = DataManager.Instance.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID, enemyShip.OverrideHardLevelID);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Buildings,
    Weapons,
    MainWeapons,
}

[System.Serializable]
public class UnitBaseAttribute
{
    /// <summary>
    /// ���Ѫ��
    /// </summary>
    public int HPMax
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public int EnergyCost
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��Դ����
    /// </summary>
    public int EnergyGenerate
    {
        get;
        protected set;
    }

    /// <summary>
    /// ̱���ָ�ʱ��
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
    /// �������
    /// </summary>
    public float WeaponRange { get; protected set; }

    /// <summary>
    /// �Ƿ���ҽ�����Ӱ���˺�����
    /// </summary>
    protected bool isPlayerShip;


    protected UnitPropertyData mainProperty;
    protected Unit _parentUnit;

    protected EnemyHardLevelItem _hardLevelItem;

    public virtual void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, bool isPlayerShip)
    {
        this.isPlayerShip = isPlayerShip;
        this._parentUnit = parentUnit;

        mainProperty = RogueManager.Instance.MainPropertyData;
        BaseHP = cfg.BaseHP;
        BaseEnergyCost = cfg.BaseEnergyCost;
        BaseEnergyGenerate = cfg.BaseEnergyGenerate;
        BaseParalysisRecoverTime = cfg.ParalysisResumeTime;

        if (isPlayerShip)
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
        if (isPlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitParalysisRecoverTimeRatio, CalculateUnitParalysis);
        }
        else
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
        }
    }

    private void CalculateHP()
    {
        var hp = mainProperty.GetPropertyFinal(PropertyModifyKey.HP);
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
        int hardLevelHPAdd = _hardLevelItem != null ? _hardLevelItem.HPAdd : 0;
        HPMax = Mathf.RoundToInt((BaseHP + hardLevelHPAdd) * (1 + hpPercent));
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
        if(enemyShip.OverrideHardLevelID != -1)
        {
            _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID);
        }
        else
        {
            _hardLevelItem = DataManager.Instance.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID, enemyShip.OverrideHardLevelID);
        }
    }
}

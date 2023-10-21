using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageResultInfo
{
    public Unit attackerUnit;
    public Vector2 HitPoint;
    public IDamageble Target;

    public int Damage;
    public bool IsCritical;
    public bool IsPlayerAttack;
    public bool IsHit = true;
    public WeaponDamageType DamageType;
    public float ShieldDamagePercent;
}

[System.Serializable]
public class WeaponAttribute : UnitBaseAttribute
{
    /// <summary>
    /// 基础伤害
    /// </summary>
    public float BaseDamage { get; protected set; }

    public WeaponDamageType DamageType { get; protected set; }

    /// <summary>
    /// 基础伤害修正
    /// </summary>
    public float BaseDamageModifyValue
    {
        get;
        protected set;
    }

    public float DamageRatioMin { get; protected set; }
    public float DamageRatioMax { get; protected set; }

    /// <summary>
    /// 暴击率
    /// </summary>
    public float CriticalRatio { get; protected set; }

    /// <summary>
    /// 暴击伤害, 100百分比单位
    /// </summary>
    public float CriticalDamagePercent { get; protected set; }



    /// <summary>
    /// 贯穿数
    /// </summary>
    public byte Transfixion { get; protected set; }

    /// <summary>
    /// 贯穿衰减, 100百分比单位
    /// </summary>
    public float TransfixionReduce { get; protected set; }

    public float ShieldDamagePercent { get; protected set; }

    public float Rate;

    public float ChargeTime;
    public float ChargeCost;

    public bool MagazineBased = true;


    /// <summary>
    /// 总弹药数量
    /// </summary>
    public int MaxMagazineSize
    {
        get;
        protected set;
    }



    /// <summary>
    /// 装填CD
    /// </summary>
    public float ReloadTime
    {
        get; protected set;
    }

    /// <summary>
    /// 开火间隔
    /// </summary>
    public float FireCD
    {
        get; protected set;
    }

    public float BeforeDelay;
    public float AfterDelay;

    private List<UnitPropertyModifyFrom> modifyFrom;
    private bool UseDamageRatio;
    private float BaseCritical;
    private float BaseWeaponRange;
    private float BaseReloadCD;
    private float BaseFireCD;
    private float BaseCriticalDamagePercent;
    private int BaseMaxMagazineSize;
    private byte BaseTransfixion;
    private float BaseTransfixionDamage;
    private float BaseShieldDamagePercent;
    private float BaseDamageRatioMin;
    private float BaseDamageRatioMax;
    private float EnemyWeaponATKPercent;

    /// <summary>
    /// 武器伤害
    /// </summary>
    /// <returns></returns>
    public DamageResultInfo GetDamage()
    {
        int Damage = 0;
        bool isCritical = false;
        float ratio = 1;
        if (_ownerShipType == OwnerShipType.PlayerShip || _ownerShipType == OwnerShipType.AIDrone)
        {
            var damage = BaseDamage + BaseDamageModifyValue;
            float damagePercent = 0;

            if (_ownerShipType == OwnerShipType.PlayerShip)
            {
                damagePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.DamagePercent);
            }
            else if(_ownerShipType == OwnerShipType.PlayerDrone)
            {
                damagePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.Aircraft_Damage);
            }
            var unitDamagePercent = _parentUnit.LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitDamagePercentAdd);

            if (UseDamageRatio)
            {
                ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            }
            var finalDamage = Mathf.Clamp(damage * (1 + (damagePercent + unitDamagePercent) / 100f) * ratio, 0, int.MaxValue);
            isCritical = Utility.CalculateRate100(CriticalRatio);
            if (isCritical)
            {
                Damage = Mathf.RoundToInt(finalDamage * (CriticalDamagePercent / 100f));
            }
            else
            {
                Damage = Mathf.RoundToInt(finalDamage);
            }
        }
        else if(_ownerShipType == OwnerShipType.AIShip)
        {
            if (UseDamageRatio)
            {
                ///伤害浮动
                ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            }
            var damageRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyDamagePercent);
            float hardLevelAdd = _hardLevelItem != null ? _hardLevelItem.ATKAdd : 0; ;
            if (_parentUnit._owner != null && _parentUnit._owner is AIShip)
            {
                var aishipCfg = (_parentUnit._owner as AIShip).AIShipCfg;
                hardLevelAdd += aishipCfg.AttackBase;
            }
            hardLevelAdd *= EnemyWeaponATKPercent;
            var damage = Mathf.Clamp((BaseDamage + hardLevelAdd) * (1 + damageRatio) * ratio, 0, int.MaxValue);
            Damage = Mathf.RoundToInt(damage);
        }

        return new DamageResultInfo
        {
            Damage = Damage,
            IsCritical = isCritical,
            IsPlayerAttack = _ownerShipType == OwnerShipType.PlayerDrone || _ownerShipType == OwnerShipType.PlayerShip,
            DamageType = DamageType,
            ShieldDamagePercent = ShieldDamagePercent
        };
    }

    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, OwnerShipType ownerType)
    {
        base.InitProeprty(parentUnit, cfg, ownerType);

        WeaponConfig _weaponCfg = cfg as WeaponConfig;
        if (_weaponCfg == null)
            return;

        UseDamageRatio = _weaponCfg.UseDamageRatio;
        DamageType = _weaponCfg.DamageType;
        BaseDamage = _weaponCfg.DamageBase;
        BaseCritical = _weaponCfg.BaseCriticalRate;
        BaseWeaponRange = _weaponCfg.BaseRange;
        BaseReloadCD = _weaponCfg.CD;
        BaseFireCD = _weaponCfg.FireCD;
        BaseCriticalDamagePercent = _weaponCfg.CriticalDamage;
        BaseMaxMagazineSize = _weaponCfg.TotalDamageCount;
        BaseTransfixion = _weaponCfg.BaseTransfixion;
        BaseTransfixionDamage = DataManager.Instance.battleCfg.TransfixionDamage_Base;
        BaseShieldDamagePercent = _weaponCfg.ShieldDamage;
        BaseDamageRatioMin = _weaponCfg.DamageRatioMin;
        BaseDamageRatioMax = _weaponCfg.DamageRatioMax;

        if (ownerType == OwnerShipType.PlayerShip || ownerType == OwnerShipType.AIDrone)
        {
            ///BindAction
            var baseDamageModify = _weaponCfg.DamageModifyFrom;
            modifyFrom = baseDamageModify;
            for (int i = 0; i < baseDamageModify.Count; i++)
            {
                var modifyKey = baseDamageModify[i].PropertyKey;
                mainProperty.BindPropertyChangeAction(modifyKey, CalculateBaseDamageModify);
            }

            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Critical, CalculateCriticalRatio);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.DamageRangeMin, CalclculateDamageRatioMin);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.DamageRangeMax, CalclculateDamageRatioMax);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);

            if (ownerType == OwnerShipType.PlayerShip)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.WeaponRange, CalculateWeaponRange);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.FireSpeed, CalculateDamageDeltaTime);

                CalculateWeaponRange();
                CalculateMaxMagazineSize();
                CalculateTransfixionCount();
                CalculateTransfixionPercent();
                CalculateReloadTime();
                CalculateDamageDeltaTime();
            }
            else if(ownerType == OwnerShipType.PlayerDrone)
            {

            }

            CalculateBaseDamageModify();
            CalculateCriticalRatio();
            CalculateCriticalDamagePercent();
            CalculateShieldDamagePercent();
            CalclculateDamageRatioMin();
            CalclculateDamageRatioMax();
        }
        else if (ownerType == OwnerShipType.AIShip) 
        {
            BaseDamageModifyValue = 0;
            CriticalRatio = 0;
            FireCD = BaseFireCD;
            ReloadTime = BaseReloadCD;
            MaxMagazineSize = BaseMaxMagazineSize;
            WeaponRange = BaseWeaponRange / 10f;
            Transfixion = BaseTransfixion;
            TransfixionReduce = BaseTransfixionDamage;
            EnemyWeaponATKPercent = _weaponCfg.EnemyAttackModify;

            if (_parentUnit._owner == null)
            {
                _hardLevelItem = GameHelper.GetEnemyHardLevelItem(1);
            }
            else
            {
                var enemyShip = _parentUnit._owner as AIShip;
                _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID);
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();

        if (_ownerShipType == OwnerShipType.PlayerShip || _ownerShipType == OwnerShipType.PlayerDrone)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Critical, CalculateCriticalRatio);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageRangeMin, CalclculateDamageRatioMin);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageRangeMax, CalclculateDamageRatioMax);
        }

        if(_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.WeaponRange, CalculateWeaponRange);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.FireSpeed, CalculateDamageDeltaTime);
        }

       
    }

    /// <summary>
    /// 刷新伤害修正
    /// </summary>
    public void RefreshDamageModify()
    {
        CalculateBaseDamageModify();
    }

    private void CalculateBaseDamageModify()
    {
        float finalValue = 0;
        for (int i = 0; i < modifyFrom.Count; i++)
        {
            var value = mainProperty.GetPropertyFinal(modifyFrom[i].PropertyKey);
            value *= modifyFrom[i].Ratio;
            finalValue += value;
        }
        BaseDamageModifyValue = finalValue;
    }

    private void CalculateCriticalRatio()
    {
        var criticalRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.Critical);
        CriticalRatio = criticalRatio + BaseCritical;
    }

    private void CalculateWeaponRange()
    {
        var weaponRange = mainProperty.GetPropertyFinal(PropertyModifyKey.WeaponRange);
        WeaponRange = Mathf.Clamp((BaseWeaponRange + weaponRange) / 10f, 0, float.MaxValue);
    }

    private void CalculateReloadTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponCD(BaseReloadCD);
        ReloadTime = cd;
    }

    private void CalculateDamageDeltaTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponDamageDeltaCD(BaseFireCD);
        FireCD = cd;
    }

    private void CalculateMaxMagazineSize()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.MagazineSize);
        var newValue = delta + BaseMaxMagazineSize;
        MaxMagazineSize = (int)Mathf.Clamp(newValue, 1, int.MaxValue);
    }

    private void CalculateTransfixionCount()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.Transfixion);
        var newValue = delta + BaseTransfixion;
        Transfixion = (byte)Mathf.Clamp(newValue, 0, byte.MaxValue);
    }

    private void CalculateTransfixionPercent()
    {
        var reducemax = DataManager.Instance.battleCfg.TransfixionDamage_Max;
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.TransfixionDamagePercent);
        var newValue = delta + BaseTransfixionDamage;
        TransfixionReduce = Mathf.Clamp(newValue, 0, reducemax);
    }

    private void CalculateCriticalDamagePercent()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.CriticalDamagePercentAdd);
        var newValue = delta + BaseCriticalDamagePercent;
        CriticalDamagePercent = Mathf.Max(newValue, 100f);
    }

    private void CalculateShieldDamagePercent()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldDamageAdd);
        var newValue = delta + BaseShieldDamagePercent;
        ShieldDamagePercent = Mathf.Max(-100, newValue);
    }

    private void CalclculateDamageRatioMin()
    {
        var modify = mainProperty.GetPropertyFinal(PropertyModifyKey.DamageRangeMin);
        var newValue = modify / 100f + BaseDamageRatioMin;
        DamageRatioMin = Mathf.Max(0, newValue);
    }

    private void CalclculateDamageRatioMax()
    {
        var modify = mainProperty.GetPropertyFinal(PropertyModifyKey.DamageRangeMax);
        var newValue = modify / 100f + BaseDamageRatioMax;
        DamageRatioMax = Mathf.Clamp(newValue, DamageRatioMin, float.MaxValue);
    }
}
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
    /// �����˺�
    /// </summary>
    public float BaseDamage { get; protected set; }

    public WeaponDamageType DamageType { get; protected set; }

    /// <summary>
    /// �����˺�����
    /// </summary>
    public float BaseDamageModifyValue
    {
        get;
        protected set;
    }

    public float DamageRatioMin { get; protected set; }
    public float DamageRatioMax { get; protected set; }

    /// <summary>
    /// ������
    /// </summary>
    public float CriticalRatio { get; protected set; }

    /// <summary>
    /// �����˺�, 100�ٷֱȵ�λ
    /// </summary>
    public float CriticalDamagePercent { get; protected set; }



    /// <summary>
    /// �ᴩ��
    /// </summary>
    public byte Transfixion { get; protected set; }

    /// <summary>
    /// �ᴩ˥��, 100�ٷֱȵ�λ
    /// </summary>
    public float TransfixionReduce { get; protected set; }

    public float ShieldDamagePercent { get; protected set; }

    public float Rate;

    public float ChargeTime;
    public float ChargeCost;

    public bool MagazineBased = true;


    /// <summary>
    /// �ܵ�ҩ����
    /// </summary>
    public int MaxMagazineSize
    {
        get;
        protected set;
    }



    /// <summary>
    /// װ��CD
    /// </summary>
    public float ReloadTime
    {
        get; protected set;
    }

    /// <summary>
    /// ������
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
    private bool CanModifyDamageCount;

    /// <summary>
    /// �����˺�
    /// </summary>
    /// <returns></returns>
    public DamageResultInfo GetDamage()
    {
        int Damage = 0;
        bool isCritical = false;
        float ratio = 1;
        if (_ownerShipType == OwnerShipType.PlayerShip || _ownerShipType == OwnerShipType.PlayerDrone)
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
        ///��������˺�
        else if(_ownerShipType == OwnerShipType.AIShip || _ownerShipType == OwnerShipType.AIDrone)
        {
            if (UseDamageRatio)
            {
                ///�˺�����
                ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            }
            var damageRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyDamagePercent);
            float hardLevelAdd = _hardLevelItem != null ? _hardLevelItem.ATKPercentAdd : 0; ;
            if (_parentUnit._owner != null && _parentUnit._owner is AIShip)
            {
                var aishipCfg = (_parentUnit._owner as AIShip).AIShipCfg;
                hardLevelAdd += aishipCfg.AttackBase;
            }
            else if(_parentUnit._owner != null && _parentUnit._owner is AIDrone)
            {
                var aiDroneCfg = (_parentUnit._owner as AIDrone).droneCfg;
                hardLevelAdd += aiDroneCfg.EnmeyATKBase;
            }

            hardLevelAdd *= EnemyWeaponATKPercent;
            var damage = Mathf.Clamp((BaseDamage * (1 + hardLevelAdd / 100f)) * (1 + damageRatio) * ratio, 0, int.MaxValue);
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
        CanModifyDamageCount = _weaponCfg.CanModifyDamageCount;

        BaseReloadCD = _weaponCfg.CD;
        BaseFireCD = _weaponCfg.FireCD;
        BaseCriticalDamagePercent = _weaponCfg.CriticalDamage;
        BaseMaxMagazineSize = _weaponCfg.TotalDamageCount;
        BaseTransfixion = _weaponCfg.BaseTransfixion;
        BaseTransfixionDamage = DataManager.Instance.battleCfg.TransfixionDamage_Base;
        BaseShieldDamagePercent = _weaponCfg.ShieldDamage;
        BaseDamageRatioMin = _weaponCfg.DamageRatioMin;
        BaseDamageRatioMax = _weaponCfg.DamageRatioMax;

        if (ownerType == OwnerShipType.PlayerShip)
        {
            FireCD = BaseFireCD;
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

            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Range, CalculateRange);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.DamageCount, CalculateDamageCount);

            CalculateRange();
            CalculateTransfixionCount();
            CalculateTransfixionPercent();
            CalculateReloadTime();
            CalculateDamageCount();

            CalculateBaseDamageModify();
            CalculateCriticalRatio();
            CalculateCriticalDamagePercent();
            CalculateShieldDamagePercent();
            CalclculateDamageRatioMin();
            CalclculateDamageRatioMax();
        }
        else if (ownerType == OwnerShipType.PlayerDrone)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Critical, CalculateCriticalRatio);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Aircraft_Speed, CalculateDroneSpeed);

            FireCD = BaseFireCD;
            Range = _weaponCfg.BaseRange / 10f;
            CalculateTransfixionCount();
            CalculateTransfixionPercent();
            CalculateDroneSpeed();
            CalculateCriticalRatio();
            CalculateCriticalDamagePercent();
        }
        else if (ownerType == OwnerShipType.AIShip || ownerType == OwnerShipType.AIDrone) 
        {
            BaseDamageModifyValue = 0;
            CriticalRatio = 0;
            FireCD = BaseFireCD;
            ReloadTime = BaseReloadCD;
            MaxMagazineSize = BaseMaxMagazineSize;

            Transfixion = BaseTransfixion;
            TransfixionReduce = BaseTransfixionDamage;
            EnemyWeaponATKPercent = _weaponCfg.EnemyAttackModify;

            if(ownerType == OwnerShipType.AIShip)
            {
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
            else
            {
                if(_parentUnit._owner == null)
                {
                    _hardLevelItem = GameHelper.GetEnemyHardLevelItem(1);
                }
                else
                {
                    var enemyDrone = _parentUnit._owner as AIDrone;
                    _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyDrone.droneCfg.HardLevelGroupID);
                }
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();

        if (_ownerShipType == OwnerShipType.PlayerShip || _ownerShipType == OwnerShipType.PlayerDrone)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Critical, CalculateCriticalRatio);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
        }

        if(_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Range, CalculateRange);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageRangeMin, CalclculateDamageRatioMin);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageRangeMax, CalclculateDamageRatioMax);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageCount, CalculateDamageCount);
        }
        else if (_ownerShipType == OwnerShipType.PlayerDrone)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Aircraft_Speed, CalculateDroneSpeed);
        }
    }

    /// <summary>
    /// ˢ���˺�����
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


    private void CalculateReloadTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponCD(BaseReloadCD);
        ReloadTime = cd;
    }

    private void CalculateDroneSpeed()
    {
        var cd = GameHelper.CalculatePlayerDroneWeaponCD(BaseReloadCD);
        ReloadTime = cd;
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
        var newValue =  BaseTransfixionDamage - delta;
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

    private void CalculateDamageCount()
    {
        if (!CanModifyDamageCount)
        {
            MaxMagazineSize = BaseMaxMagazineSize;
        }
        else
        {
            MaxMagazineSize = GameHelper.CalculateDamageCount(BaseMaxMagazineSize);
        }
    }
}
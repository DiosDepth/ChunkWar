using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



[System.Serializable]
public class WeaponAttribute : UnitBaseAttribute
{
    /// <summary>
    /// �����˺�
    /// </summary>
    public float BaseDamage
    {   
        get;
        protected set;
    }

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
    /// �������
    /// </summary>
    public float WeaponRange { get; protected set; }

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
    private float criticalBase;
    private float rangeBase;
    private float ReloadCDBase;
    private float FireCDBase;
    private int BaseMaxMagazineSize;

    /// <summary>
    /// �����˺�
    /// </summary>
    /// <returns></returns>
    public int GetDamage()
    {
        if (isPlayerShip)
        {
            var damage = BaseDamage + BaseDamageModifyValue;
            var damagePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.DamagePercent);
            var ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            var finalDamage = Mathf.Clamp(damage * (1 + damagePercent / 100f) * ratio, 0, int.MaxValue);
            return Mathf.RoundToInt(finalDamage);
        }
        else
        {
            var damageRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyDamagePercent);
            var enemy = _parentUnit._owner as AIShip;
            float hardLevelRatio = 0;
            if(enemy != null)
            {
                hardLevelRatio = GameHelper.GetEnemyDamageByHardLevel(enemy.AIShipCfg.HardLevelCfg);
            }

            var ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            var damage = Mathf.Clamp(BaseDamage * (1 + damageRatio + hardLevelRatio / 100f) * ratio, 0, int.MaxValue);
            return Mathf.RoundToInt(damage);
        }
    }

    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, bool isPlayerShip)
    {
        base.InitProeprty(parentUnit, cfg, isPlayerShip);
        
        WeaponConfig _weaponCfg = cfg as WeaponConfig;
        if (_weaponCfg == null)
            return;

        BaseDamage = _weaponCfg.DamageBase;
        DamageRatioMin = _weaponCfg.DamageRatioMin;
        DamageRatioMax = _weaponCfg.DamageRatioMax;
        criticalBase = _weaponCfg.BaseCriticalRate;
        rangeBase = _weaponCfg.BaseRange;
        ReloadCDBase = _weaponCfg.CD;
        FireCDBase = _weaponCfg.FireCD;
        BaseMaxMagazineSize = _weaponCfg.TotalDamageCount;


        if (isPlayerShip)
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
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.WeaponRange, CalculateWeaponRange);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.DamageDeltaPercent, CalculateDamageDeltaTime);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);

            CalculateBaseDamageModify();
            CalculateCriticalRatio();
            CalculateReloadTime();
            CalculateDamageDeltaTime();
            CalculateMaxMagazineSize();
        }
        else
        {
            BaseDamageModifyValue = 0;
            CriticalRatio = 0;
            FireCD = FireCDBase;
            ReloadTime = ReloadCDBase;
            MaxMagazineSize = BaseMaxMagazineSize;
            WeaponRange = rangeBase;
        }
    }

    public override void Destroy()
    {
        base.Destroy();

        if (isPlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Critical, CalculateCriticalRatio);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.WeaponRange, CalculateWeaponRange);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateReloadTime);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.DamageDeltaPercent, CalculateDamageDeltaTime);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);
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
        CriticalRatio = criticalRatio + criticalBase;
    }

    private void CalculateWeaponRange()
    {
        var weaponRange = mainProperty.GetPropertyFinal(PropertyModifyKey.WeaponRange);
        WeaponRange = rangeBase + weaponRange;
    }

    private void CalculateReloadTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponCD(ReloadCDBase);
        ReloadTime = cd;
    }

    private void CalculateDamageDeltaTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponDamageDeltaCD(FireCDBase);
        FireCD = cd;
    }

    private void CalculateMaxMagazineSize()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.MagazineSize);
        var newValue = delta + BaseMaxMagazineSize;
        MaxMagazineSize = (int)Mathf.Clamp(newValue, 1, int.MaxValue);
    }
}


public class ShipWeapon : Weapon
{
    public virtual void HandleWeapon(InputAction.CallbackContext context)
    {
        if(weaponmode  == WeaponControlType.Autonomy)
        {
            return;
        }
        if(weaponmode == WeaponControlType.Manual)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    weaponstate.ChangeState(WeaponState.Charging);
                    break;

                case InputActionPhase.Canceled:

                    WeaponOn();
                    break;
            }
        }

        if (weaponmode == WeaponControlType.SemiAuto)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    WeaponOn();
                    break;

                case InputActionPhase.Canceled:
                    WeaponOff();
                    break;
            }
        }
    }



    public override void Start()
    {
        base.Start();

    }

    public override void Update()
    {
        base.Update();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

    }


    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
    }

    public override void ProcessWeapon()
    {
        base.ProcessWeapon();
    }

    public override void WeaponOn()
    {
        base.WeaponOn();
    }

    public override void WeaponOff()
    {
        base.WeaponOff();
    }

    public override void WeaponReady()
    {
        base.WeaponReady();
    }

    public override void WeaponStart()
    {
        base.WeaponStart();
    }

    public override void WeaponBeforeDelay()
    {
        base.WeaponBeforeDelay();
    }

    public override void FireRequest()
    {
        base.FireRequest();
    }

    public override void WeaponFiring()
    {
        base.WeaponFiring();
    }

    public override void DoFire()
    {
        base.DoFire();
    }

    public override void WeaponBetweenDelay()
    {
        base.WeaponBetweenDelay();
    }

    public override void WeaponCharging()
    {
        base.WeaponCharging();
    }

    public override void WeaponCharged()
    {
        base.WeaponCharged();
    }

    public override void WeaponFired()
    {
        base.WeaponFired();
    }

    public override void WeaponAfterDelay()
    {
        base.WeaponAfterDelay();
    }

    public override void WeaponEnd()
    {
        base.WeaponEnd();
    }

    public override void WeaponReload()
    {
        base.WeaponReload();
    }

    public override void WeaponRecover()
    {
        base.WeaponRecover();
    }

    public override bool TakeDamage(int value)
    {
        return base.TakeDamage(value);
    }
}

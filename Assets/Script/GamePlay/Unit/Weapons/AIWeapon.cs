using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWeapon : Weapon
{



    public virtual void HandleWeapon()
    {

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

    public override void SetUnitProcess(bool isprocess)
    {
        base.SetUnitProcess(isprocess);
        if (_weaponCfg.unitType != UnitType.MainWeapons)
        {
            WeaponOn();
        }

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

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        return base.TakeDamage(ref info);
    }
}

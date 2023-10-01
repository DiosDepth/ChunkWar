using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        _owner = m_owner;
        base.Initialization(m_owner, m_unitconfig);
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
    }
    public override void SetUnitProcess(bool isprocess)
    {
        base.SetUnitProcess(isprocess);


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

        //if(targetList == null || targetList.Count == 0)
        //{
        //    weaponstate.ChangeState(WeaponState.End);
        //}
        //if(aimingtype == WeaponAimingType.TargetDirectional || aimingtype == WeaponAimingType.Directional)
        //{
        //    if (_firepointindex >= firePoint.Length)
        //    {
        //        _firepointindex = 0;
        //    }
        //    weaponstate.ChangeState(WeaponState.Firing);
        //}
        //if(aimingtype == WeaponAimingType.TargetBased )
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

    public override bool TakeDamage(DamageResultInfo info)
    {
        return base.TakeDamage(info);
    }
}

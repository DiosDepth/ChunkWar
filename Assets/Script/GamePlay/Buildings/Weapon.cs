using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public enum WeaponType
{
    OneShort,
    Burst,
}

public enum WeaponFireModeType
{
    Auto,
    Manual,
}
public enum WeaponState
{

    Ready,
    Start,
    BeforeDelay,
    Firing,
    BetweenDelay,
    Charging,
    Charged,
    Fired,
    AfterDelay,
    End,
    Recover,
}

[System.Serializable]
public class WeaponAttribute
{

    public float ATK;
    public float Rate;
    public float ChargeCost;

    public float BeforeDelay;
    public float BetweenDelay;
    public float AfterDelay;
}

public enum AvalibalMainWeapon
{
    LongRangeIonBlaster,
    TripleNeutronBlaster,
}


public class Weapon : Unit
{

    public WeaponType weapontype;
    public WeaponFireModeType weaponmode;
    public StateMachine<WeaponState> weaponstate;


    [SerializeField]
    public WeaponAttribute baseAttribute;


    private float _beforeDelayCounter;
    private float _betweenDelayCounter;
    private float _afterDelayCounter;

    private float _isChargeFire;

    public override void Initialization()
    {
        base.Initialization();
        weaponstate = new StateMachine<WeaponState>(this.gameObject, false, false);
    }

    public override void Start()
    {
        base.Start();
        
    }

    public override void Update()
    {
        base.Update();
        ProcessWeapon();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public virtual void ProcessWeapon()
    {
        switch (weaponstate.CurrentState)
        {
            case WeaponState.Ready:
                WeaponReady();
                break;
            case WeaponState.BeforeDelay:
                WeaponBeforeDelay();
                break;
            case WeaponState.Start:
                WeaponStart();
                break;
            case WeaponState.Firing:
                WeaponFiring();
                break;
            case WeaponState.BetweenDelay:
                WeaponBetweenDelay();
                break;
            case WeaponState.Charging:
                WeaponCharging();
                break;
            case WeaponState.Charged:
                WeaponCharged();
                break;
            case WeaponState.Fired:
                WeaponFired();
                break;
            case WeaponState.AfterDelay:
                WeaponAfterDelay();
                break;
            case WeaponState.End:
                WeaponEnd();
                break;
            case WeaponState.Recover:
                WeaponRecover();
                break;
        }
    }


    public virtual void WeaponOn()
    {
        weaponstate.ChangeState(WeaponState.Start);
    }

    public virtual void WeaponOff()
    {
        weaponstate.ChangeState(WeaponState.End);
    }


    public virtual void WeaponReady()
    {

    }

    public virtual void WeaponStart()
    {
        _beforeDelayCounter = baseAttribute.BeforeDelay;
        _betweenDelayCounter = baseAttribute.BetweenDelay;
        _afterDelayCounter = baseAttribute.AfterDelay;

        weaponstate.ChangeState(WeaponState.BeforeDelay);
    }
    public virtual void WeaponBeforeDelay()
    {
        _beforeDelayCounter -= Time.deltaTime;
        if(_beforeDelayCounter < 0)
        {
            _beforeDelayCounter = baseAttribute.BeforeDelay;
            FireRequest();
        }
    }
    public virtual void FireRequest()
    {
        // 进行开火条件的判定,成功则 WeaponFiring ,否则 WeaponEnd
        weaponstate.ChangeState(WeaponState.Firing);
    }

    public virtual void WeaponFiring()
    {
        DoFire();
        switch (weaponmode)
        {
            case WeaponFireModeType.Auto:
                weaponstate.ChangeState(WeaponState.BetweenDelay);
                break;
            case WeaponFireModeType.Manual:
                weaponstate.ChangeState(WeaponState.Fired);
                break;
        }
    }

    public virtual void DoFire()
    {

    }

    public virtual void WeaponBetweenDelay()
    {
        _betweenDelayCounter -= Time.deltaTime;
        if (_betweenDelayCounter < 0)
        {
            _betweenDelayCounter = baseAttribute.BetweenDelay;
            FireRequest();
        }
    }
    public virtual void WeaponCharging()
    {

    }



    public virtual void WeaponCharged()
    {

    }



    public virtual void WeaponFired()
    {
        
    }

    public virtual void WeaponAfterDelay()
    {
        _afterDelayCounter -= Time.deltaTime;
        if (_afterDelayCounter < 0)
        {
            _afterDelayCounter = baseAttribute.AfterDelay;
            weaponstate.ChangeState(WeaponState.End);
        }
    }

    public virtual void WeaponEnd()
    {
        weaponstate.ChangeState(WeaponState.Recover);
    }

    public virtual void WeaponRecover()
    {
        weaponstate.ChangeState(WeaponState.Ready);
    }




    public override void TakeDamage()
    {
        base.TakeDamage();
    }
}

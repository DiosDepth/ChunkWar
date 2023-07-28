using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float ChargeTime;
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

    public WeaponFireModeType weaponmode;
    public StateMachine<WeaponState> weaponstate;


    [SerializeField]
    public WeaponAttribute baseAttribute;


    private float _beforeDelayCounter;
    private float _betweenDelayCounter;
    private float _afterDelayCounter;
    private float _chargeConter;
    
    private bool _isChargeFire;
    private bool _isWeaponOff;

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

            case WeaponState.Start:
                WeaponStart();
                break;
            case WeaponState.BeforeDelay:
                WeaponBeforeDelay();
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

    public virtual void HandleWeapon(InputAction.CallbackContext context)
    {
        if(weaponmode == WeaponFireModeType.Manual)
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

        if (weaponmode == WeaponFireModeType.Auto)
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


    public virtual void WeaponOn()
    {
        weaponstate.ChangeState(WeaponState.Start);

    }

    public virtual void WeaponOff()
    {
        _isWeaponOff = true;
        
    }



    public virtual void WeaponReady()
    {
        Debug.Log(this.gameObject + " : WeaponReady");

        _beforeDelayCounter = baseAttribute.BeforeDelay;
        _betweenDelayCounter = baseAttribute.BetweenDelay;
        _afterDelayCounter = baseAttribute.AfterDelay;
        _chargeConter = baseAttribute.ChargeTime;
    }

    public virtual void WeaponStart()
    {
        Debug.Log(this.gameObject + " : WeaponStart");
        _beforeDelayCounter = baseAttribute.BeforeDelay;
        _betweenDelayCounter = baseAttribute.BetweenDelay;
        _afterDelayCounter = baseAttribute.AfterDelay;
        _chargeConter = baseAttribute.ChargeTime;

        weaponstate.ChangeState(WeaponState.BeforeDelay);
    }
    public virtual void WeaponBeforeDelay()
    {
        Debug.Log(this.gameObject + " : WeaponBeforeDelay");
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

                if(!_isWeaponOff)
                {
                    weaponstate.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponstate.ChangeState(WeaponState.Fired);
                }
       
                break;
            case WeaponFireModeType.Manual:
              
                weaponstate.ChangeState(WeaponState.Fired);
                break;
        }
    }

    public virtual void DoFire()
    {
        switch (weaponmode)
        {
            case WeaponFireModeType.Auto:
                Debug.Log(this.gameObject + " : Auto Firing!!!");
                break;
            case WeaponFireModeType.Manual:
                if(_isChargeFire)
                {
                    Debug.Log(this.gameObject + " : Manual Charge Firing!!!");
                }
                else
                {
                    Debug.Log(this.gameObject + " : Manual Firing!!!");
                }

                break;
        }
    }

    public virtual void WeaponBetweenDelay()
    {
        Debug.Log(this.gameObject + " : WeaponBetweenDelay");
        _betweenDelayCounter -= Time.deltaTime;
        if (_betweenDelayCounter < 0)
        {
            _betweenDelayCounter = baseAttribute.BetweenDelay;
            FireRequest();
        }
    }
    public virtual void WeaponCharging()
    {
        Debug.Log(this.gameObject + " : WeaponCharging");

        _chargeConter -= Time.deltaTime;
        if (_chargeConter < 0)
        {
            _chargeConter = baseAttribute.ChargeTime;
            weaponstate.ChangeState(WeaponState.Charged);
        }
    }



    public virtual void WeaponCharged()
    {
        Debug.Log(this.gameObject + " : WeaponCharged");
        _isChargeFire = true;
    }



    public virtual void WeaponFired()
    {
        Debug.Log(this.gameObject + " : WeaponFired");
        weaponstate.ChangeState(WeaponState.AfterDelay);
    }

    public virtual void WeaponAfterDelay()
    {
        Debug.Log(this.gameObject + " : WeaponAfterDelay");
        _afterDelayCounter -= Time.deltaTime;
        if (_afterDelayCounter < 0)
        {
            _afterDelayCounter = baseAttribute.AfterDelay;
            weaponstate.ChangeState(WeaponState.End);
        }
    }

    public virtual void WeaponEnd()
    {
        Debug.Log(this.gameObject + " : WeaponEnd");
        weaponstate.ChangeState(WeaponState.Recover);
    }

    public virtual void WeaponRecover()
    {
        Debug.Log(this.gameObject + " : WeaponRecover");
        _isChargeFire = false;
        _isWeaponOff = false;
        weaponstate.ChangeState(WeaponState.Ready);
    }




    public override void TakeDamage()
    {
        base.TakeDamage();
    }
}

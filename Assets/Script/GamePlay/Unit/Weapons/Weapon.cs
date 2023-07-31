using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponFireModeType
{
    Autonomy,
    SemiAuto,
    Manual,
}
public enum WeaponState
{

    Ready,
    Start,
    Reload,
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

    public bool MagazineBased = false;
    public int MaxMagazineSize = 30;

    public float ReloadTime = 1f;


    public float BeforeDelay;
    public float BetweenDelay;
    public float AfterDelay;

}

public enum AvaliableBulletType
{
    TestBullet,
}

public class Weapon : Unit
{

    public WeaponFireModeType weaponmode;
    public StateMachine<WeaponState> weaponstate;

    public AvaliableBulletType bulletType = AvaliableBulletType.TestBullet;
    public Transform firePoint;

    [SerializeField]
    public WeaponAttribute baseAttribute;
    public int magazine;

    private float _beforeDelayCounter;
    private float _betweenDelayCounter;
    private float _afterDelayCounter;
    private float _chargeCounter;
    private float _reloadCounter;
    
    private bool _isChargeFire;
    private bool _isWeaponOn;

    private BulletData _bulletdata;
    public Projectile _lastbullet;
    public override void Initialization(PlayerShip m_owner)
    {
        base.Initialization(m_owner);

        weaponstate = new StateMachine<WeaponState>(this.gameObject, false, false);
        if(baseAttribute.MagazineBased)
        {
            magazine = baseAttribute.MaxMagazineSize;
        }

        _beforeDelayCounter = baseAttribute.BeforeDelay;
        _betweenDelayCounter = baseAttribute.BetweenDelay;
        _afterDelayCounter = baseAttribute.AfterDelay;
        _chargeCounter = baseAttribute.ChargeTime;
        _reloadCounter = baseAttribute.ReloadTime;

        DataManager.Instance.BulletDataDic.TryGetValue(bulletType.ToString(), out _bulletdata);
        if(_bulletdata ==  null)
        {
            Debug.LogError("Bullet Data is Invalid");
        }

        firePoint = transform.Find("FirePoint");
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
            case WeaponState.Reload:
                WeaponReload();
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

        if (weaponmode == WeaponFireModeType.SemiAuto)
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
        _isWeaponOn = true;
        if(weaponstate.CurrentState != WeaponState.Reload)
        {
            weaponstate.ChangeState(WeaponState.Start);
        }

    }

    public virtual void WeaponOff()
    {
        _isWeaponOn = false;
        _lastbullet = null;
    }



    public virtual void WeaponReady()
    {
        Debug.Log(this.gameObject + " : WeaponReady");
    }

    public virtual void WeaponStart()
    {
        Debug.Log(this.gameObject + " : WeaponStart");
        _beforeDelayCounter = baseAttribute.BeforeDelay;
        _betweenDelayCounter = baseAttribute.BetweenDelay;
        _afterDelayCounter = baseAttribute.AfterDelay;
        _chargeCounter = baseAttribute.ChargeTime;
        _reloadCounter = baseAttribute.ReloadTime;

        if (magazine <= 0 && baseAttribute.MagazineBased)
        {
            weaponstate.ChangeState(WeaponState.End);
        }

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
        if (magazine <= 0 && baseAttribute.MagazineBased)
        {
            weaponstate.ChangeState(WeaponState.End);
            return;
        }

        weaponstate.ChangeState(WeaponState.Firing);
    }

    public virtual void WeaponFiring()
    {
       
        DoFire();
        weaponstate.ChangeState(WeaponState.Fired);

    }

    public virtual void DoFire()
    {
        
        switch (weaponmode)
        {
            case WeaponFireModeType.SemiAuto:
                Debug.Log(this.gameObject + " : SemiAuto Firing!!!");
                PoolManager.Instance.GetObjectAsync(_bulletdata.PrefabPath, false, (obj) =>
                {
                    obj.transform.SetTransform(firePoint);
                    _lastbullet = obj.GetComponent<Projectile>();
                    _lastbullet.InitialmoveDirection = firePoint.transform.up;

                    _lastbullet.SetActive();
                });
                break;
            case WeaponFireModeType.Manual:
                if(_isChargeFire)
                {
                    Debug.Log(this.gameObject + " : Manual Charge Firing!!!");
                    PoolManager.Instance.GetObjectAsync(_bulletdata.PrefabPath, false, (obj) =>
                    {
                        obj.transform.SetTransform(firePoint);
                        _lastbullet = obj.GetComponent<Projectile>();
                        _lastbullet.speed = 200f;
                        _lastbullet.InitialmoveDirection = firePoint.transform.up;
                        _lastbullet.SetActive();
                    });
                }
                else
                {
                    Debug.Log(this.gameObject + " : Manual Firing!!!");

                    PoolManager.Instance.GetObjectAsync(_bulletdata.PrefabPath, false, (obj) =>
                    {
                        obj.transform.SetTransform(firePoint);
                        _lastbullet = obj.GetComponent<Projectile>();
                        _lastbullet.speed = 100f;
                        _lastbullet.InitialmoveDirection = firePoint.transform.up;
                        _lastbullet.SetActive();
                    });
                }
                break;
            case WeaponFireModeType.Autonomy:
                Debug.Log(this.gameObject + " : Autonomy Firing!!!");
                break;
        }
        if(baseAttribute.MagazineBased)
        {
            magazine --;
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

        _chargeCounter -= Time.deltaTime;
        if (_chargeCounter < 0)
        {
            _chargeCounter = baseAttribute.ChargeTime;
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
        switch (weaponmode)
        {
            case WeaponFireModeType.SemiAuto:
                if (_isWeaponOn)
                {
                    weaponstate.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponstate.ChangeState(WeaponState.AfterDelay);
                }
                break;
            case WeaponFireModeType.Manual:
                weaponstate.ChangeState(WeaponState.AfterDelay);

                break;
        }
        
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
        
        if(magazine <= 0 && baseAttribute.MagazineBased)
        {
            weaponstate.ChangeState(WeaponState.Reload);
        }
        else
        {
            weaponstate.ChangeState(WeaponState.Recover);
        }

    }


    public virtual void WeaponReload()
    {
        Debug.Log(this.gameObject + " : WeaponReload");
        _reloadCounter -= Time.deltaTime;
        if (_reloadCounter < 0)
        {
            _reloadCounter = baseAttribute.ReloadTime;
            magazine = baseAttribute.MaxMagazineSize;
            if(_isWeaponOn)
            {
                if (weaponmode == WeaponFireModeType.Autonomy)
                {
                    weaponstate.ChangeState(WeaponState.Start);
                }
                if (weaponmode == WeaponFireModeType.Manual)
                {
                    weaponstate.ChangeState(WeaponState.Recover);
                }
                if (weaponmode == WeaponFireModeType.SemiAuto)
                {
                    weaponstate.ChangeState(WeaponState.Start);
                }

            }
            else
            {
                weaponstate.ChangeState(WeaponState.Recover);
            }
        }
    }
    public virtual void WeaponRecover()
    {
        Debug.Log(this.gameObject + " : WeaponRecover");
        _isChargeFire = false;
        _isWeaponOn = false;
        _lastbullet = null;
        weaponstate.ChangeState(WeaponState.Ready);
    }

    public override void TakeDamage()
    {
        base.TakeDamage();
    }
}

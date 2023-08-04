using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Unit
{
    public WeaponFireModeType weaponmode;
    public StateMachine<WeaponState> weaponstate;

    public AvaliableBulletType bulletType = AvaliableBulletType.None;
    public Transform firePoint;

    [SerializeField]
    public WeaponAttribute weaponAttribute;
    public int magazine;

    protected float _beforeDelayCounter;
    protected float _betweenDelayCounter;
    protected float _afterDelayCounter;
    protected float _chargeCounter;
    protected float _reloadCounter;

    protected bool _isChargeFire;
    protected bool _isWeaponOn;

    protected BulletData _bulletdata;
    public Projectile _lastbullet;

    protected WeaponConfig _weaponCfg;



    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        this._weaponCfg = m_unitconfig as WeaponConfig;
        InitWeaponAttribute();
        base.Initialization(m_owner,m_unitconfig);
        weaponstate = new StateMachine<WeaponState>(this.gameObject, false, false);
        DataManager.Instance.BulletDataDic.TryGetValue(bulletType.ToString(), out _bulletdata);
        if (_bulletdata == null)
        {
            Debug.LogError("Bullet Data is Invalid");
        }

        firePoint = transform.Find("FirePoint");

        if (_owner is PlayerShip)
        {
            var playerShip = _owner as PlayerShip;
            _isActive = !playerShip.IsEditorShip;
        }
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        weaponAttribute.Destroy();

    }

    public virtual void ProcessWeapon()
    {
        if (!_isActive)
            return;

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





    public virtual void WeaponOn()
    {
        _isWeaponOn = true;
        if (weaponstate.CurrentState != WeaponState.Reload)
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
        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.BetweenDelay;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;

        if (magazine <= 0 && weaponAttribute.MagazineBased)
        {
            weaponstate.ChangeState(WeaponState.End);
        }

        weaponstate.ChangeState(WeaponState.BeforeDelay);
    }
    public virtual void WeaponBeforeDelay()
    {
        Debug.Log(this.gameObject + " : WeaponBeforeDelay");
        _beforeDelayCounter -= Time.deltaTime;
        if (_beforeDelayCounter < 0)
        {
            _beforeDelayCounter = weaponAttribute.BeforeDelay;
            FireRequest();
        }
    }
    public virtual void FireRequest()
    {
        // 进行开火条件的判定,成功则 WeaponFiring ,否则 WeaponEnd
        if (magazine <= 0 && weaponAttribute.MagazineBased)
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
                if (_isChargeFire)
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
        if (weaponAttribute.MagazineBased)
        {
            magazine--;
        }
    }

    public virtual void WeaponBetweenDelay()
    {
        Debug.Log(this.gameObject + " : WeaponBetweenDelay");
        _betweenDelayCounter -= Time.deltaTime;
        if (_betweenDelayCounter < 0)
        {
            _betweenDelayCounter = weaponAttribute.BetweenDelay;
            FireRequest();
        }
    }
    public virtual void WeaponCharging()
    {
        Debug.Log(this.gameObject + " : WeaponCharging");

        _chargeCounter -= Time.deltaTime;
        if (_chargeCounter < 0)
        {
            _chargeCounter = weaponAttribute.ChargeTime;
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
            _afterDelayCounter = weaponAttribute.AfterDelay;
            weaponstate.ChangeState(WeaponState.End);
        }
    }

    public virtual void WeaponEnd()
    {
        Debug.Log(this.gameObject + " : WeaponEnd");

        if (magazine <= 0 && weaponAttribute.MagazineBased)
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
            _reloadCounter = weaponAttribute.ReloadTime;
            magazine = weaponAttribute.MaxMagazineSize;
            if (_isWeaponOn)
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



    public override bool TakeDamage(int value)
    {
        return base.TakeDamage(value);
    }

    public virtual void InitWeaponAttribute()
    {
    
        if (weaponAttribute.MagazineBased)
        {
            magazine = weaponAttribute.MaxMagazineSize;
        }

        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.BetweenDelay;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;

        weaponAttribute.InitProeprty(_weaponCfg, _owner is PlayerShip);
        baseAttribute = weaponAttribute;

    }
}

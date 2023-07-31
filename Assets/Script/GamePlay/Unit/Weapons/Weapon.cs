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
public class WeaponAttribute : UnitBaseAttribute
{
    /// <summary>
    /// 基础伤害
    /// </summary>
    public float BaseDamage
    {   
        get;
        protected set;
    }

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
    /// 武器射程
    /// </summary>
    public float WeaponRange { get; protected set; }

    public float Rate;
    public float ChargeTime;
    public float ChargeCost;

    public bool MagazineBased = false;
    public int MaxMagazineSize = 30;

    public float ReloadTime = 1f;


    public float BeforeDelay;
    public float BetweenDelay;
    public float AfterDelay;

    private List<UnitPropertyModifyFrom> modifyFrom;
    private float criticalBase;
    private float rangeBase;

    /// <summary>
    /// 武器伤害
    /// </summary>
    /// <returns></returns>
    public int GetDamage()
    {
        var damage = BaseDamage + BaseDamageModifyValue;
        var damagePercent = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.DamagePercent);
        var ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
        var finalDamage = Mathf.Clamp(damage * (1 + damagePercent) * ratio, 0, int.MaxValue);
        return Mathf.RoundToInt(finalDamage);
    }

    public override void InitProeprty(BaseUnitConfig cfg)
    {
        base.InitProeprty(cfg);
        var mainProperty = RogueManager.Instance.MainPropertyData;

        WeaponConfig _weaponCfg = cfg as WeaponConfig;
        if (_weaponCfg == null)
            return;

        BaseDamage = _weaponCfg.DamageBase;
        DamageRatioMin = _weaponCfg.DamageRatioMin;
        DamageRatioMax = _weaponCfg.DamageRatioMax;
        criticalBase = _weaponCfg.BaseCriticalRate;
        rangeBase = _weaponCfg.BaseRange;

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

        CalculateBaseDamageModify();
        CalculateCriticalRatio();
    }

    private void CalculateBaseDamageModify()
    {
        float finalValue = 0;
        for (int i = 0; i < modifyFrom.Count; i++)
        {
            var value = RogueManager.Instance.MainPropertyData.GetPropertyFinal(modifyFrom[i].PropertyKey);
            value *= modifyFrom[i].Ratio;
            finalValue += value;
        }
        BaseDamageModifyValue = finalValue;
    }

    private void CalculateCriticalRatio()
    {
        var criticalRatio = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Critical);
        CriticalRatio = criticalRatio + criticalBase;
    }

    private void CalculateWeaponRange()
    {
        var weaponRange = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.WeaponRange);
        WeaponRange = rangeBase + weaponRange;
    }
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
    public WeaponAttribute weaponAttribute;
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

    /// <summary>
    /// 武器配置
    /// </summary>
    protected WeaponConfig _weaponCfg;

    public void Initialization(BaseShip m_owner, WeaponConfig weaponConfig)
    {
        base.Initialization(m_owner);
        this._weaponCfg = weaponConfig;
        weaponstate = new StateMachine<WeaponState>(this.gameObject, false, false);
        InitWeaponAttribute();
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
        if(_beforeDelayCounter < 0)
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
        if(weaponAttribute.MagazineBased)
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
        
        if(magazine <= 0 && weaponAttribute.MagazineBased)
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

    private void InitWeaponAttribute()
    {
        weaponAttribute = new WeaponAttribute();
        if (weaponAttribute.MagazineBased)
        {
            magazine = weaponAttribute.MaxMagazineSize;
        }

        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.BetweenDelay;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;

        weaponAttribute.InitProeprty(_weaponCfg);
        baseAttribute = weaponAttribute;
    }

}

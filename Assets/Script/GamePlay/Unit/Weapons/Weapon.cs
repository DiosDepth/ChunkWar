using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Sirenix.Utilities;


public struct DamageResultInfo
{
    public int Damage;
    public bool IsCritical;

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
    /// 暴击伤害, 100百分比单位
    /// </summary>
    public float CriticalDamagePercent { get; protected set; }

    /// <summary>
    /// 武器射程
    /// </summary>
    public float WeaponRange { get; protected set; }

    /// <summary>
    /// 贯穿数
    /// </summary>
    public byte Transfixion { get; protected set; }

    /// <summary>
    /// 贯穿衰减, 100百分比单位
    /// </summary>
    public float TransfixionReduce { get; protected set; }

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
    private float BaseCritical;
    private float BaseWeaponRange;
    private float BaseReloadCD;
    private float BaseFireCD;
    private float BaseCriticalDamagePercent;



    private int BaseMaxMagazineSize;
    private byte BaseTransfixion;
    private float BaseTransfixionReduce;

    /// <summary>
    /// 武器伤害
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
            if (enemy != null)
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
        BaseCritical = _weaponCfg.BaseCriticalRate;
        BaseWeaponRange = _weaponCfg.BaseRange;
        BaseReloadCD = _weaponCfg.CD;
        BaseFireCD = _weaponCfg.FireCD;
        BaseCriticalDamagePercent = _weaponCfg.CriticalDamage;
        BaseMaxMagazineSize = _weaponCfg.TotalDamageCount;
        BaseTransfixion = _weaponCfg.BaseTransfixion;
        BaseTransfixionReduce = _weaponCfg.TransfixionReduce;

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
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionReducePercent, CalculateTransfixionPercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);

            CalculateBaseDamageModify();
            CalculateWeaponRange();
            CalculateCriticalRatio();
            CalculateReloadTime();
            CalculateDamageDeltaTime();
            CalculateMaxMagazineSize();
            CalculateTransfixionCount();
            CalculateTransfixionPercent();
            CalculateCriticalDamagePercent();
        }
        else
        {
            BaseDamageModifyValue = 0;
            CriticalRatio = 0;
            FireCD = BaseFireCD;
            ReloadTime = BaseReloadCD;
            MaxMagazineSize = BaseMaxMagazineSize;
            WeaponRange = BaseWeaponRange;
            Transfixion = BaseTransfixion;
            TransfixionReduce = BaseTransfixionReduce;
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
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.TransfixionReducePercent, CalculateTransfixionPercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
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
        WeaponRange = BaseWeaponRange + weaponRange;
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
        var reducemin = DataManager.Instance.battleCfg.TransfixionReduce_Max;
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.TransfixionReducePercent);
        var newValue = delta + BaseTransfixionReduce;
        TransfixionReduce = Mathf.Clamp(newValue, reducemin, newValue);
    }

    private void CalculateCriticalDamagePercent()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.CriticalDamagePercentAdd);
        var newValue = delta + BaseCriticalDamagePercent;
        CriticalDamagePercent = Mathf.Max(newValue, 100f);
    }
}
public enum WeaponControlType
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

public enum WeaponFireMode
{
    Simultaneous,
    Sequent,
}

public enum WeaponAimingType
{
    Directional,
    TargetDirectional,
    TargetBased,
}
public enum WeaponTargetMode
{
    Single,
    Mutipule,
}

public enum DamageTextType
{
    Normal,
    Critical
}


public class Weapon : Unit
{
    public WeaponControlType weaponmode;
    public StateMachine<WeaponState> weaponstate;
    public WeaponFireMode firemode;
    public WeaponAimingType aimingtype= WeaponAimingType.Directional;
    public AvaliableBulletType bulletType = AvaliableBulletType.None;
    public WeaponTargetMode targetmode = WeaponTargetMode.Single;

    public LayerMask mask = 1 << 7;
    
    public List<GameObject> targetList;
    public int maxTargetCount = 3;
    public Transform[] firePoint;
    public Transform rotationRoot;
    public float roatateSpeed = 180f;

    public WeaponAttribute weaponAttribute;

    [ReadOnly]
    public int magazine;

    protected float _beforeDelayCounter;
    protected float _betweenDelayCounter;
    protected float _afterDelayCounter;
    protected float _chargeCounter;
    protected float _reloadCounter;

    protected bool _isChargeFire;
    protected bool _isWeaponOn;

    protected Collider2D[] _targetcandidates;
  
    protected int _firepointindex = 0;
    protected BulletData _bulletdata;
    public Bullet _lastbullet;

    protected WeaponConfig _weaponCfg;

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        this._weaponCfg = m_unitconfig as WeaponConfig;
        InitWeaponAttribute(m_owner is PlayerShip);
        weaponstate = new StateMachine<WeaponState>(this.gameObject, false, false);
        DataManager.Instance.BulletDataDic.TryGetValue(bulletType.ToString(), out _bulletdata);
        if (_bulletdata == null)
        {
            Debug.LogError("Bullet Data is Invalid");
        }

        if(firePoint.Length<=0)
        {
            Debug.LogError(this.gameObject.name + " has no fire point!");
        }

        base.Initialization(m_owner, m_unitconfig);

        if (_owner is PlayerShip)
        {
            var playerShip = _owner as PlayerShip;
            _isProcess = !playerShip.IsEditorShip;
            if(!playerShip.IsEditorShip && playerShip.playerShipCfg.MainWeaponID == _weaponCfg.ID)
            {
                ///MainCore
                InitCoreData();
            }
        }

        _firepointindex = 0;

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
        weaponAttribute?.Destroy();
        base.OnDestroy();
 

    }

    public override void SetUnitProcess(bool isprocess)
    {
        base.SetUnitProcess(isprocess);
        if(isprocess)
        {
            if (_weaponCfg.unitType != UnitType.MainWeapons)
            {
                WeaponOn();
            }
        }
    }

    

    public virtual void ProcessWeapon()
    {
        if (!_isProcess)
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
        //if (weaponstate.CurrentState != WeaponState.Reload)
        //{
        //    weaponstate.ChangeState(WeaponState.Start);
        //}

    }

    public virtual void WeaponOff()
    {
        _isWeaponOn = false;
        _lastbullet = null;
    }



    public virtual void WeaponReady()
    {
        Debug.Log(this.gameObject + " : WeaponReady");
        if(_isWeaponOn)
        {
            weaponstate.ChangeState(WeaponState.Start);
        }
    }

    public virtual void WeaponStart()
    {
        Debug.Log(this.gameObject + " : WeaponStart");
        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.FireCD;
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
        if(_firepointindex >= firePoint.Length)
        {
            _firepointindex = 0;
        }
        // 搜索合适的target

        if(aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetBased)
        {

            _targetcandidates = Physics2D.OverlapCircleAll(this.transform.position, weaponAttribute.WeaponRange, mask);

            if(_targetcandidates.Length > 0)
            {
                if(targetmode == WeaponTargetMode.Single)
                {
                    GameObject temptarget = null;
                    float tempsqedistance;
                    float shortestdistance = float.MaxValue;
                    // 筛选目标候选
                    for (int i = 0; i < _targetcandidates.Length; i++)
                    {
                        tempsqedistance = transform.position.SqrDistanceXY(_targetcandidates[i].gameObject.transform.position);
                        if (tempsqedistance <= shortestdistance)
                        {
                            tempsqedistance = shortestdistance;
                            temptarget = _targetcandidates[i].gameObject;
                        }
                    }
                    if(temptarget != null)
                    {
                        targetList.Add(temptarget);
                    }
                }
                if(targetmode == WeaponTargetMode.Mutipule)
                {
                    float d1;
                    float d2;
                    // 筛选目标候选,按照距离排序
                    _targetcandidates.Sort((x,y) => 
                    {
                        d1 = transform.position.SqrDistanceXY(x.gameObject.transform.position);
                        d2 = transform.position.SqrDistanceXY(y.gameObject.transform.position);
                        return d1.CompareTo(d2);
                    });

                    for (int i = 0; i < maxTargetCount; i++)
                    {
                        targetList.Add(_targetcandidates[i].gameObject);
                    }
                }
            }
        }





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
        int firecount = 0;
        
        if(weaponAttribute.MagazineBased)
        {
            if (firemode == WeaponFireMode.Sequent)
            {
                firecount = 1;
            }
            if (firemode == WeaponFireMode.Simultaneous)
            {
                if (magazine >= firePoint.Length)
                {
                    firecount = firePoint.Length;
                }
                else
                {
                    firecount = magazine;
                }
            }
        }
        else
        {
            if(firemode == WeaponFireMode.Sequent)
            {
                firecount = 1;
            }
            if(firemode == WeaponFireMode.Simultaneous)
            {
                firecount = firePoint.Length;
            }
        }



        switch (weaponmode)
        {
            case WeaponControlType.SemiAuto:
                Debug.Log(this.gameObject + " : SemiAuto Firing!!!");
                if(firemode == WeaponFireMode.Sequent)
                {
                    FireSequent(_firepointindex);
                    _firepointindex++;
                }

                if(firemode ==WeaponFireMode.Simultaneous)
                {
                    FireSimultaneous(firecount);
                }
                break;
            case WeaponControlType.Manual:
                if (_isChargeFire)
                {
                    Debug.Log(this.gameObject + " : Manual Charge Firing!!!");
                    if (firemode == WeaponFireMode.Sequent)
                    {
                        FireSequent(_firepointindex);
                        _firepointindex++;
                    }
                    if (firemode == WeaponFireMode.Simultaneous)
                    {
                        
                        FireSimultaneous(firecount);
                    }

                }
                else
                {
                    Debug.Log(this.gameObject + " : Manual Firing!!!");
                    if (firemode == WeaponFireMode.Sequent)
                    {
                        FireSequent(_firepointindex);
                        _firepointindex++;
                    }
                    if (firemode == WeaponFireMode.Simultaneous)
                    {

                        FireSimultaneous(firecount);
                    }

                }
                break;
            case WeaponControlType.Autonomy:
                Debug.Log(this.gameObject + " : Autonomy Firing!!!");
                if (firemode == WeaponFireMode.Sequent)
                {
                    FireSequent(_firepointindex);
                    _firepointindex++;
                }
                if (firemode == WeaponFireMode.Simultaneous)
                {
                    FireSimultaneous(firecount);
                }

                break;
        }
        if (weaponAttribute.MagazineBased)
        {
          magazine -= firecount;
        }
    }

    public virtual void FireSimultaneous(int firecount, UnityAction<Bullet> callback = null)
    {
        for (int i = 0; i < firecount; i++)
        {
            PoolManager.Instance.GetObjectAsync(_bulletdata.PrefabPath, false, firePoint[i], (obj,trs) =>
            {
                obj.transform.SetTransform(trs);
                _lastbullet = obj.GetComponent<Bullet>();
                _lastbullet.InitialmoveDirection = trs.up;

                _lastbullet.PoolableSetActive();
                _lastbullet.Initialization();
                _lastbullet.SetOwner(this);
                _lastbullet.Shoot();
            }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);
        }
    }

    public virtual void FireSequent(int firepointindex)
    {
        PoolManager.Instance.GetObjectAsync(_bulletdata.PrefabPath, false, firePoint[firepointindex],(obj,trs) =>
        {
            obj.transform.SetTransform(trs);
            _lastbullet = obj.GetComponent<Bullet>();
            _lastbullet.InitialmoveDirection = trs.up;

            _lastbullet.PoolableSetActive();
            _lastbullet.Initialization();
            _lastbullet.SetOwner(this);
            _lastbullet.Shoot();
        }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);

    }


    public virtual void WeaponBetweenDelay()
    {
        Debug.Log(this.gameObject + " : WeaponBetweenDelay");
        if(!_isWeaponOn)
        {
            weaponstate.ChangeState(WeaponState.AfterDelay);
        }
        _betweenDelayCounter -= Time.deltaTime;
        if (_betweenDelayCounter < 0)
        {
            _betweenDelayCounter = weaponAttribute.FireCD;
            if(_isWeaponOn)
            {
                FireRequest();
            }
            else
            {
                weaponstate.ChangeState(WeaponState.End);
            }
 
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
            case WeaponControlType.Autonomy:
                if (_isWeaponOn)
                {
                    weaponstate.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponstate.ChangeState(WeaponState.AfterDelay);
                }
                break;
            case WeaponControlType.SemiAuto:
                if (_isWeaponOn)
                {
                    weaponstate.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponstate.ChangeState(WeaponState.AfterDelay);
                }
                break;
            case WeaponControlType.Manual:
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

        if(_betweenDelayCounter >0)
        {
            _betweenDelayCounter -= Time.deltaTime;
        }
        if (_betweenDelayCounter <= 0)
        {
            _betweenDelayCounter = weaponAttribute.FireCD;
            if (magazine <= 0 && weaponAttribute.MagazineBased)
            {
                weaponstate.ChangeState(WeaponState.Reload);
            }
            else
            {
                weaponstate.ChangeState(WeaponState.Recover);
            }
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
                if (weaponmode == WeaponControlType.Autonomy)
                {
                    weaponstate.ChangeState(WeaponState.Start);
                }
                if (weaponmode == WeaponControlType.Manual)
                {
                    weaponstate.ChangeState(WeaponState.Recover);
                }
                if (weaponmode == WeaponControlType.SemiAuto)
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

    public override void Death()
    {
        base.Death();
    }

    public override void Restore()
    {
        base.Restore();
    }

    public override bool TakeDamage(int value)
    {
        return base.TakeDamage(value);
    }

    public virtual void InitWeaponAttribute(bool isPlayerShip)
    {
        weaponAttribute.InitProeprty(this, _weaponCfg, isPlayerShip);
        baseAttribute = weaponAttribute;

        if (weaponAttribute.MagazineBased)
        {
            magazine = weaponAttribute.MaxMagazineSize;
        }
        else
        {
            ///Fix Value
            magazine = 1;
        }

        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.FireCD;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;
    }

    /// <summary>
    /// 初始化核心信息
    /// </summary>
    private void InitCoreData()
    {
        HpComponent.BindHPChangeAction(OnPlayerCoreHPChange, false);
    }

    private void OnPlayerCoreHPChange()
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.CoreHPChange);
    }
}

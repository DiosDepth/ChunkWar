using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

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
    Linked,
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
public enum WeaponInterceptType
{
    Unit,
    Other,
}

/// <summary>
/// 这里的index指的是，当前的target 在ECSManager中 ActiveUnit的位置
/// distance 和 direction都是当前的weapon 对于target位置来说的
/// </summary>
public class UnitTargetInfo
{
    public GameObject target;
    public int index;
    public float distance;
    public Vector3 direction;

    public UnitTargetInfo()
    {
        target = null;
        index = -1;
        distance = 0;
        direction = Vector3.zero;
    }
    public UnitTargetInfo(GameObject m_target, int m_index, float m_distance, Vector3 m_direction )
    {
        target = m_target;
        index = m_index;
        distance = m_distance;
        direction = m_direction;
    }


}



public class Weapon : Unit
{
    public WeaponControlType weaponmode;
    public StateMachine<WeaponState> weaponState;
    public WeaponFireMode firemode;
    public WeaponAimingType aimingtype= WeaponAimingType.Directional;
    public WeaponInterceptType intercepttype = WeaponInterceptType.Unit;


    [ValueDropdown("GetBulletNames")]
    public string bulletName = string.Empty;
    public WeaponTargetMode targetmode = WeaponTargetMode.Single;

    public LayerMask mask = 1 << 7;
    public Transform[] firePoint;
    public float scatter = 0f;
    public float roatateSpeed = 180f;


    [HideInInspector]
    public WeaponAttribute weaponAttribute;

    [Sirenix.OdinInspector.ReadOnly]
    public int magazine;

    /// <summary>
    /// CD百分比
    /// </summary>
    public float ReloadTimePercent
    {
        get
        {
            return _reloadCounter / weaponAttribute.ReloadTime;
        }
    }

    protected float _beforeDelayCounter;
    protected float _betweenDelayCounter;
    protected float _afterDelayCounter;
    protected float _chargeCounter;
    protected float _reloadCounter;

    protected bool _isChargeFire;
    protected bool _isWeaponOn;

    protected Collider2D[] _targetcandidates;
  
    protected int _firepointindex = 0;
    protected int _targetindex = 0;
    protected BulletConfig _bulletdata;
    public Bullet _lastbullet;

    public WeaponConfig WeaponCfg { get { return _weaponCfg; } }
    protected WeaponConfig _weaponCfg;

    /* Call Back */
    public Action<float> OnReloadCDUpdate;
    public Action<int> OnMagazineChange;


    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _baseUnitConfig = m_unitconfig;
        _weaponCfg = m_unitconfig as WeaponConfig;
        _owner = m_owner;
        InitWeaponAttribute(GameHelper.GetOwnerShipType(_owner));
        base.Initialization(m_owner, m_unitconfig);

        weaponState = new StateMachine<WeaponState>(this.gameObject, false, false);
        _bulletdata = DataManager.Instance.GetBulletConfigByType(bulletName);
        if (_bulletdata == null)
        {
            Debug.LogError("Bullet Data is Invalid");
        }

        if(firePoint.Length<=0)
        {
            Debug.LogError(this.gameObject.name + " has no fire point!");
        }

        _firepointindex = 0;
        _targetindex = 0;
    }

    public override void InitializationEditorUnit(PlayerEditShip ship, BaseUnitConfig m_unitconfig)
    {
        this._weaponCfg = m_unitconfig as WeaponConfig;
        base.InitializationEditorUnit(ship, m_unitconfig);
        InitWeaponAttribute(OwnerShipType.PlayerShip);
    }

    protected override void OnDestroy()
    {
        weaponAttribute?.Destroy();
        base.OnDestroy();
    }




    public struct FindMutipleUnitTargetsJob : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public NativeArray<UnitJobData> job_unitJobData;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetsPos;
        //这里返回的时对应的target在list中的index

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<UnitTargetJobData> rv_targetsInfo;
        UnitTargetJobData tempinfo;
        int index;
        public void Execute(int startIndex, int count)
        {
            NativeList<UnitTargetJobData> tempinfolist; 
            for (int i = startIndex; i < startIndex + count; i++)
            {
                tempinfolist  = new NativeList<UnitTargetJobData>(Allocator.Temp);
                //找到所有在范围内的target
                for (int n = 0; n < job_targetsPos.Length; n++)
                {
                    var targetPos = job_targetsPos[n];
                    if (math.distance(job_unitJobData[i].position, targetPos) <= job_unitJobData[i].range)
                    {
                        tempinfo.targetPos = targetPos;
                        tempinfo.targetIndex = n;
                        tempinfo.targetDirection = math.normalize(targetPos - job_unitJobData[i].position);
                        tempinfo.distanceToTarget = math.distance(targetPos, job_unitJobData[i].position);
                        tempinfolist.Add(tempinfo);
                    }
                }

                index = 0;
                for (int c = 0; c < i; c++)
                {
                    index += job_unitJobData[c].targetCount;
                }

               
                if(tempinfolist.Length > 0)
                {
                    //全部找完了之后进行排序
                    tempinfolist.Sort();
                    //排序之后按照最大目标数量进行结果装填
                    for (int s = 0; s < job_unitJobData[i].targetCount; s++)
                    {
                        //当前的index不大于 临时目标列表的长度
                        if (s <= tempinfolist.Length - 1)
                        {
                            rv_targetsInfo[index + s] = tempinfolist[s];
                        }
                        else
                        {
                            rv_targetsInfo[index + s] = tempinfolist[s % tempinfolist.Length];
                        }
                    }
                }
                else
                {
                    for (int s = 0; s < job_unitJobData[i].targetCount; s++)
                    {
                        tempinfo.targetIndex = -1;
                        rv_targetsInfo[index + s] = tempinfo;
                    }
                }
                tempinfolist.Dispose();
            }
      
        }
    }


    public virtual void HandleOtherWeaponRotation()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (rotationRoot == null)
        {
            return;
        }
        if (targetList.Count > 0)
        {
            rotationRoot.rotation = MathExtensionTools.CalculateRotation(transform.up, targetList[0].direction, roatateSpeed);
        }
        else
        {
            rotationRoot.rotation = MathExtensionTools.CalculateRotation(transform.up, _owner.transform.up, roatateSpeed);
        }
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

    

    public virtual bool RefreshValidTarget()
    {
        targetList.RemoveAll(x => x == null 
        || x.target.activeInHierarchy == false 
        || x.distance > weaponAttribute.Range);

        if(targetList.Count != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public virtual void ProcessWeapon()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!_isProcess) { return; }
         
        HandleOtherWeaponRotation();
        switch (weaponState.CurrentState)
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
    }

    public virtual void WeaponOff()
    {
        _isWeaponOn = false;
        _lastbullet = null;

    }



    public virtual void WeaponReady()
    {

        //Debug.Log(this.gameObject + " : WeaponReady");

        weaponState.ChangeState(WeaponState.Start);
        
    }

    public virtual void WeaponStart()
    {
        if (!_isWeaponOn) { return; }
    
        if (magazine <= 0 && weaponAttribute.MagazineBased)
        {
            weaponState.ChangeState(WeaponState.End);
            return;
        }
        //Debug.Log(this.gameObject + " : WeaponStart");
        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.FireCD;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;

        weaponState.ChangeState(WeaponState.BeforeDelay);
    }
    public virtual void WeaponBeforeDelay()
    {
        //Debug.Log(this.gameObject + " : WeaponBeforeDelay");
        _beforeDelayCounter -= Time.deltaTime;
        if (_beforeDelayCounter < 0)
        {
            _beforeDelayCounter = weaponAttribute.BeforeDelay;
            FireRequest();
        }
    }
    public virtual void FireRequest()
    {
        if(weaponAttribute.MagazineBased)
        {
            if(magazine <=0 )
            {
                weaponState.ChangeState(WeaponState.End);
                return;
                //ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDStart, UID);
            }
        }

        if(aimingtype != WeaponAimingType.Directional)
        {
            RefreshValidTarget();
            if (targetList == null || targetList.Count == 0)
            {
                weaponState.ChangeState(WeaponState.End);
                return;
            }
        }

        if (_firepointindex >= firePoint.Length)
        {
            _firepointindex = 0;
        }
        weaponState.ChangeState(WeaponState.Firing);
    }

    public virtual void WeaponFiring()
    {
        DoFire();
        weaponState.ChangeState(WeaponState.Fired);
    }

    public virtual void DoFire()
    {
        int firecount = CalculateFireCount();
        SoundManager.Instance.PlayBattleSound(_weaponCfg.FireAudioClip, transform);
        if (_weaponCfg.FireShakeCfg != null)
        {
            var cfg = _weaponCfg.FireShakeCfg;
            if (cfg.CameraShake)
            {
                CameraManager.Instance.ShakeBattleCamera(cfg);
            }
        }
        switch (weaponmode)
        {
            case WeaponControlType.SemiAuto:
                Debug.Log(this.gameObject + " : SemiAuto Firing!!!");
                if(firemode == WeaponFireMode.Sequent)
                {
                    if(_targetindex >= targetList.Count)
                    {
                        _targetindex = 0;
                    }
                    FireSequent(_firepointindex, _targetindex);
                    _firepointindex++;
                    _targetindex++;
                }

                if(firemode ==WeaponFireMode.Simultaneous)
                {
                    FireSimultaneous(firecount);
                }

                if(firemode == WeaponFireMode.Linked)
                {
                    FireLinked(firecount);
                }
                break;
            case WeaponControlType.Manual:
                if (_isChargeFire)
                {
                    Debug.Log(this.gameObject + " : Manual Charge Firing!!!");
                    if (firemode == WeaponFireMode.Sequent)
                    {
                        if (_targetindex >= targetList.Count)
                        {
                            _targetindex = 0;
                        }
                        FireSequent(_firepointindex, _targetindex);
                        _firepointindex++;
                        _targetindex++;
                    }
                    if (firemode == WeaponFireMode.Simultaneous)
                    {
                        
                        FireSimultaneous(firecount);
                    }

                    if (firemode == WeaponFireMode.Linked)
                    {
                        FireLinked(firecount);
                    }
                    _isChargeFire = false;
                }
                else
                {
                    Debug.Log(this.gameObject + " : Manual Firing!!!");
                    if (firemode == WeaponFireMode.Sequent)
                    {
                        if (_targetindex >= targetList.Count)
                        {
                            _targetindex = 0;
                        }
                        FireSequent(_firepointindex, _targetindex);
                        _firepointindex++;
                        _targetindex++;
                    }
                    if (firemode == WeaponFireMode.Simultaneous)
                    {
                        FireSimultaneous(firecount);
                    }
                    if (firemode == WeaponFireMode.Linked)
                    {
                        FireLinked(firecount);
                    }

                }
                break;
            case WeaponControlType.Autonomy:
                //Debug.Log(this.gameObject + " : Autonomy Firing!!!");
                if (firemode == WeaponFireMode.Sequent)
                {
                    if (_targetindex >= targetList.Count)
                    {
                        _targetindex = 0;
                    }
                    FireSequent(_firepointindex, _targetindex);
                    _firepointindex++;
                    _targetindex++;
                }
                if (firemode == WeaponFireMode.Simultaneous)
                {
                    FireSimultaneous(firecount);
                }
                if (firemode == WeaponFireMode.Linked)
                {
                    FireLinked(firecount);
                }

                break;
        }
        if (weaponAttribute.MagazineBased)
        {
            magazine -= firecount;
            OnMagazineChange?.Invoke(magazine);
        }
    }

    public virtual int CalculateFireCount()
    {
        int count = 1;
        if (weaponAttribute.MagazineBased)
        {
            if (firemode == WeaponFireMode.Sequent)
            {
                count = 1;
            }
            if (firemode == WeaponFireMode.Simultaneous)
            {
                if (magazine >= firePoint.Length)
                {
                    count = firePoint.Length;
                }
                else
                {
                    count = magazine;
                }
            }
        }
        else
        {
            if (firemode == WeaponFireMode.Sequent)
            {
                count = 1;
            }
            if (firemode == WeaponFireMode.Simultaneous)
            {
                count = firePoint.Length;
            }
        }

        return count;

    }

    public virtual void FireSimultaneous(int firecount, UnityAction<Bullet> callback = null)
    {
        if ((aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetDirectional) && targetList?.Count > 0)
        {
            _targetindex = 0;
            for (int i = 0; i < firecount; i++)
            {
                if (_targetindex >= targetList.Count)
                {
                    _targetindex = 0;
                }
                PoolManager.Instance.GetBulletAsync(_bulletdata.PrefabPath, false, firePoint[i], targetList[_targetindex].target, (obj, trs, target) =>
                {
                    obj.transform.SetTransform(trs);
                    _lastbullet = obj.GetComponent<Bullet>();
                    _lastbullet.SetUp(_bulletdata, OnBulletHitAction);
                    _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(target);
                    //_lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
                 
                    _lastbullet.Shoot();
                }, LevelManager.BulletPool);
                CreateFireEffect(firePoint[i]);
                _targetindex++;
            }
        }
        else
        {
            WeaponAimingType temptype = aimingtype;
            aimingtype = WeaponAimingType.Directional;
            if (aimingtype == WeaponAimingType.Directional)
            {
                for (int i = 0; i < firecount; i++)
                {
                    PoolManager.Instance.GetBulletAsync(_bulletdata.PrefabPath, false, firePoint[i], (obj, trs) =>
                    {
                        obj.transform.SetTransform(trs);
                        _lastbullet = obj.GetComponent<Bullet>();
                        _lastbullet.SetUp(_bulletdata, OnBulletHitAction);
                        _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                        _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                        _lastbullet.SetFirePoint(trs.gameObject);
                        _lastbullet.SetTarget(null);
                        _lastbullet.SetOwner(this);
                        // _lastbullet.PoolableSetActive();
                        _lastbullet.Initialization();
                       
                        _lastbullet.Shoot();
                    }, LevelManager.BulletPool);
                    CreateFireEffect(firePoint[i]);
                }
            }
            aimingtype = temptype;

        }
    }

    public virtual void FireSequent(int firepointindex, int targetindex)
    {
        if ((aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetDirectional) && targetList?.Count > 0)
        {
            {
                PoolManager.Instance.GetBulletAsync(_bulletdata.PrefabPath, false, firePoint[firepointindex], targetList[targetindex].target, (obj, trs, target) =>
                {
                    obj.transform.SetTransform(trs);
                    _lastbullet = obj.GetComponent<Bullet>();
                    _lastbullet.SetUp(_bulletdata, OnBulletHitAction);
                    _lastbullet.InitialmoveDirection  = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(target);
                    // _lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
                  
                    _lastbullet.Shoot();
                }, LevelManager.BulletPool);

                CreateFireEffect(firePoint[firepointindex]);
            }
        }
        else
        {
            WeaponAimingType temptype = aimingtype;
            aimingtype = WeaponAimingType.Directional;
            if (aimingtype == WeaponAimingType.Directional)
            {
                PoolManager.Instance.GetBulletAsync(_bulletdata.PrefabPath, false, firePoint[firepointindex], (obj, trs) =>
                {
                    obj.transform.SetTransform(trs);
                    _lastbullet = obj.GetComponent<Bullet>();
                    _lastbullet.SetUp(_bulletdata, OnBulletHitAction);
                    _lastbullet.InitialmoveDirection  = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(null);
                    //_lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
            
                    _lastbullet.Shoot();
                }, LevelManager.BulletPool);
            }
            aimingtype = temptype;
            CreateFireEffect(firePoint[firepointindex]);
        }
    }

    public virtual void FireLinked(int firecount, UnityAction<Bullet> callback = null)
    {
        if ((aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetDirectional) && targetList?.Count > 0)
        {
            _targetindex = 0;
            for (int i = 0; i < firecount; i++)
            {
                if (_targetindex >= targetList.Count)
                {
                    _targetindex = 0;
                }
                PoolManager.Instance.GetBulletAsync(_bulletdata.PrefabPath, false, firePoint[i], targetList[_targetindex].target, (obj, trs, target) =>
                {
                    obj.transform.SetTransform(trs);
                    _lastbullet = obj.GetComponent<Bullet>();
                    _lastbullet.SetUp(_bulletdata, OnBulletHitAction);
                    _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(target);
                    //_lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();

                    _lastbullet.Shoot();
                }, LevelManager.BulletPool);
                _targetindex++;
                CreateFireEffect(firePoint[i]);
            }
        }
    }


    public virtual void WeaponBetweenDelay()
    {
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
                weaponState.ChangeState(WeaponState.AfterDelay);
            }
 
        }
    }
    public virtual void WeaponCharging()
    {
        //Debug.Log(this.gameObject + " : WeaponCharging");

        _chargeCounter -= Time.deltaTime;
        if (_chargeCounter < 0)
        {
            _chargeCounter = weaponAttribute.ChargeTime;
            weaponState.ChangeState(WeaponState.Charged);
        }
        if(_isWeaponOn)
        {
            weaponState.ChangeState(WeaponState.Start);
        }
    }



    public virtual void WeaponCharged()
    {
        //Debug.Log(this.gameObject + " : WeaponCharged");
        _isChargeFire = true;
        if(_isWeaponOn)
        {
            weaponState.ChangeState(WeaponState.Start);
        }
    }



    public virtual void WeaponFired()
    {
        //Debug.Log(this.gameObject + " : WeaponFired");
        switch (weaponmode)
        {
            case WeaponControlType.Autonomy:
                if (_isWeaponOn)
                {
                    weaponState.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponState.ChangeState(WeaponState.AfterDelay);
                }
                break;
            case WeaponControlType.SemiAuto:
                if (_isWeaponOn)
                {
                    weaponState.ChangeState(WeaponState.BetweenDelay);
                }
                else
                {
                    weaponState.ChangeState(WeaponState.AfterDelay);
                }
                break;
            case WeaponControlType.Manual:
                weaponState.ChangeState(WeaponState.AfterDelay);
                break;
        }
    }

    public virtual void WeaponAfterDelay()
    {
        //Debug.Log(this.gameObject + " : WeaponAfterDelay");
        _afterDelayCounter -= Time.deltaTime;
        if (_afterDelayCounter < 0)
        {
            _afterDelayCounter = weaponAttribute.AfterDelay;
            weaponState.ChangeState(WeaponState.End);
        }

    }

    public virtual void WeaponEnd()
    {
        //Debug.Log(this.gameObject + " : WeaponEnd");
        _firepointindex = 0;
        _targetindex = 0;
       
        if(_betweenDelayCounter >0)
        {
            _betweenDelayCounter -= Time.deltaTime;
        }
        if (_betweenDelayCounter <= 0)
        {
            _betweenDelayCounter = weaponAttribute.FireCD;
            if (magazine <= 0 && weaponAttribute.MagazineBased)
            {
                weaponState.ChangeState(WeaponState.Reload);
                if(_owner is PlayerShip)
                {
                    ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDStart, UID);
                    LevelManager.Instance.PlayerWeaponReload(UID, true);
                }

            }
            else
            {
                weaponState.ChangeState(WeaponState.Recover);
            }
        }


    }


    public virtual void WeaponReload()
    {
        //Debug.Log(this.gameObject + " : WeaponReload");

        _reloadCounter -= Time.deltaTime;
        OnReloadCDUpdate?.Invoke(_reloadCounter);
        if (_reloadCounter < 0)
        {
            _reloadCounter = weaponAttribute.ReloadTime;
            magazine = weaponAttribute.MaxMagazineSize;
            if (_isWeaponOn)
            {
                if (weaponmode == WeaponControlType.Autonomy)
                {
                    weaponState.ChangeState(WeaponState.Start);
                    return;
                }
                if (weaponmode == WeaponControlType.Manual)
                {
                    weaponState.ChangeState(WeaponState.Recover);
                    return;
                }
                if (weaponmode == WeaponControlType.SemiAuto)
                {
                    weaponState.ChangeState(WeaponState.Start);
                    return;
                }

            }
            else
            {
                weaponState.ChangeState(WeaponState.Recover);
                if(_owner is PlayerShip)
                {
                    ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDEnd, UID);
                    LevelManager.Instance.PlayerWeaponReload(UID, false);
                }
            }
        }
    }
    public virtual void WeaponRecover()
    {
        //Debug.Log(this.gameObject + " : WeaponRecover");
        _isChargeFire = false;
        WeaponOff();
        weaponState.ChangeState(WeaponState.Ready);
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
    }

    public override void Restore()
    {
        base.Restore();
    }

    public override bool TakeDamage(DamageResultInfo info)
    {
        return base.TakeDamage(info);
    }

    public virtual void InitWeaponAttribute(OwnerShipType type)
    {
        weaponAttribute.InitProeprty(this, _weaponCfg, type);
        baseAttribute = weaponAttribute;
        scatter = _weaponCfg.Scatter;
        _beforeDelayCounter = weaponAttribute.BeforeDelay;
        _betweenDelayCounter = weaponAttribute.FireCD;
        _afterDelayCounter = weaponAttribute.AfterDelay;
        _chargeCounter = weaponAttribute.ChargeTime;
        _reloadCounter = weaponAttribute.ReloadTime;
    }

    /// <summary>
    /// 初始化核心信息
    /// </summary>
    public void InitCoreData()
    {
        HpComponent.BindHPChangeAction(OnPlayerCoreHPChange, false, 0, 0);
    }

    private void OnPlayerCoreHPChange(int oldValue, int newValue)
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.CoreHPChange);
        var percent = HpComponent.HPPercent;
        LevelManager.Instance.OnPlayerShipCoreHPPercentChange(percent, oldValue, newValue);
    }

    public override void PauseGame()
    {
        base.PauseGame();

    }
    public override void UnPauseGame()
    {
        base.UnPauseGame();
    }

    private void CreateFireEffect(Transform targetTrans)
    {
        if (_weaponCfg.FireEffect == null || string.IsNullOrEmpty(_weaponCfg.FireEffect.EffectName))
            return;

        EffectManager.Instance.CreateEffectAndFollow(_weaponCfg.FireEffect, targetTrans);
    }

    /// <summary>
    /// 创建打点信息
    /// </summary>
    /// <returns></returns>
    public override UnitLogData GetUnitLogData()
    {
        var statistics = AchievementManager.Instance.GetOrCreateRuntimeUnitStatisticsData(UID);
        UnitLogData data = new UnitLogData();
        data.UnitID = UnitID;
        data.UID = UID;
        data.UnitName = LocalizationManager.Instance.GetTextValue(_baseUnitConfig.GeneralConfig.Name);
        data.CreateWave = statistics.CreateWaveIndex;
        data.RemoveWave = statistics.RemoveWaveIndex;
        data.DamageCreateTotal = statistics.DamageCreateTotal;
        data.DamageTakeTotal = statistics.DamageTakeTotal;
        return data;
    }

    /// <summary>
    /// 子弹命中效果
    /// </summary>
    private void OnBulletHitAction()
    {
        if (_weaponCfg == null)
            return;

        if(_weaponCfg.HitShakeCfg != null)
        {
            var cfg = _weaponCfg.HitShakeCfg;
            if (cfg.CameraShake)
            {
                CameraManager.Instance.ShakeBattleCamera(cfg);
            }
        }
    }

#if UNITY_EDITOR

    private ValueDropdownList<string> GetBulletNames()
    {
        ValueDropdownList<string> result = new ValueDropdownList<string>();
        var allBullets = DataManager.Instance.GetAllBulletConfig();
        allBullets = allBullets.OrderBy(x => x.ID).ToList();

        var targetowner = this is AIAdditionalWeapon ? OwnerType.AI : OwnerType.Player;
        for(int i = 0; i < allBullets.Count; i++)
        {
            if(allBullets[i].Owner == targetowner)
            {
                result.Add(string.Format("[{0}]{1}", allBullets[i].ID, allBullets[i].BulletName), allBullets[i].BulletName);
            }
        }
        return result;
    }

    [OnInspectorInit]
    private void OnInitData()
    {
        DataManager.Instance.LoadBulletConfig_Editor();
    }

#endif
}

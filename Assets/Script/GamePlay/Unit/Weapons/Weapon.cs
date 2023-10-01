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
using Unity.Burst;


public class DamageResultInfo
{
    public int Damage;
    public bool IsCritical;
    public bool IsPlayerAttack;
    public WeaponDamageType DamageType;
    public float ShieldDamagePercent;

    public int DamageTemp;
}


[System.Serializable]
public class WeaponAttribute : UnitBaseAttribute
{
    /// <summary>
    /// 基础伤害
    /// </summary>
    public float BaseDamage { get; protected set; }

    public WeaponDamageType DamageType { get; protected set; }

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
    /// 贯穿数
    /// </summary>
    public byte Transfixion { get; protected set; }

    /// <summary>
    /// 贯穿衰减, 100百分比单位
    /// </summary>
    public float TransfixionReduce { get; protected set; }

    public float ShieldDamagePercent { get; protected set; }

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
    private bool UseDamageRatio;
    private float BaseCritical;
    private float BaseWeaponRange;
    private float BaseReloadCD;
    private float BaseFireCD;
    private float BaseCriticalDamagePercent;
    private int BaseMaxMagazineSize;
    private byte BaseTransfixion;
    private float BaseTransfixionReduce;
    private float BaseShieldDamagePercent;

    /// <summary>
    /// 武器伤害
    /// </summary>
    /// <returns></returns>
    public DamageResultInfo GetDamage()
    {
        int Damage = 0;
        bool isCritical = false;
        if (isPlayerShip)
        {
            var damage = BaseDamage + BaseDamageModifyValue;
            var damagePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.DamagePercent);
            var ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            var finalDamage = Mathf.Clamp(damage * (1 + damagePercent / 100f) * ratio, 0, int.MaxValue);
            bool critical = Utility.RandomResult(0, CriticalRatio);
            if (critical)
            {
                Damage = Mathf.RoundToInt(finalDamage * (1 + CriticalDamagePercent / 100f));
            }
            else
            {
                Damage = Mathf.RoundToInt(finalDamage);
            }
        }
        else
        {
            float ratio = 1;
            if (UseDamageRatio)
            {
                ///伤害浮动
                ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            }
            var damageRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyDamagePercent);
            int hardLevelAdd = 0;
            hardLevelAdd = _hardLevelItem != null ? _hardLevelItem.ATKAdd : 0;
            var damage = Mathf.Clamp((BaseDamage + hardLevelAdd) * (1 + damageRatio) * ratio, 0, int.MaxValue);
            Damage =  Mathf.RoundToInt(damage);
        }

        return new DamageResultInfo
        {
            Damage = Damage,
            IsCritical = isCritical,
            IsPlayerAttack = _parentUnit._owner is PlayerShip,
            DamageType = DamageType,
            ShieldDamagePercent = ShieldDamagePercent
        };
    }

    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, bool isPlayerShip)
    {
        base.InitProeprty(parentUnit, cfg, isPlayerShip);

        WeaponConfig _weaponCfg = cfg as WeaponConfig;
        if (_weaponCfg == null)
            return;

        UseDamageRatio = _weaponCfg.UseDamageRatio;
        DamageType = _weaponCfg.DamageType;
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
        BaseShieldDamagePercent = _weaponCfg.ShieldDamage;

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
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.FireSpeed, CalculateDamageDeltaTime);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);

            CalculateBaseDamageModify();
            CalculateWeaponRange();
            CalculateCriticalRatio();
            CalculateReloadTime();
            CalculateDamageDeltaTime();
            CalculateMaxMagazineSize();
            CalculateTransfixionCount();
            CalculateTransfixionPercent();
            CalculateCriticalDamagePercent();
            CalculateShieldDamagePercent();
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
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.FireSpeed, CalculateDamageDeltaTime);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.MagazineSize, CalculateMaxMagazineSize);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Transfixion, CalculateTransfixionCount);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.TransfixionDamagePercent, CalculateTransfixionPercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);
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
        WeaponRange = Mathf.Clamp(BaseWeaponRange + weaponRange / 10f, 0, float.MaxValue);
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
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.TransfixionDamagePercent);
        var newValue = delta + BaseTransfixionReduce;
        TransfixionReduce = Mathf.Clamp(newValue, reducemin, newValue);
    }

    private void CalculateCriticalDamagePercent()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.CriticalDamagePercentAdd);
        var newValue = delta + BaseCriticalDamagePercent;
        CriticalDamagePercent = Mathf.Max(newValue, 100f);
    }

    private void CalculateShieldDamagePercent()
    {
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldDamageAdd);
        var newValue = delta + BaseShieldDamagePercent;
        ShieldDamagePercent = Mathf.Max(-100, newValue);
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

/// <summary>
/// 这里的index指的是，当前的target在Ai manager.targetActiveUnitList中的位置
/// distance 和 direction都是当前的weapon 对于target位置来说的
/// </summary>
public class WeaponTargetInfo
{
    public GameObject target;
    public int index;
    public float distance;
    public Vector3 direction;

    public WeaponTargetInfo()
    {
        target = null;
        index = -1;
        distance = 0;
        direction = Vector3.zero;
    }
    public WeaponTargetInfo(GameObject m_target, int m_index, float m_distance, Vector3 m_direction )
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
    public StateMachine<WeaponState> weaponstate;
    public WeaponFireMode firemode;
    public WeaponAimingType aimingtype= WeaponAimingType.Directional;
    public AvaliableBulletType bulletType = AvaliableBulletType.None;
    public WeaponTargetMode targetmode = WeaponTargetMode.Single;

    public LayerMask mask = 1 << 7;



    public Transform[] firePoint;
    public float scatter = 0f;

    public float roatateSpeed = 180f;

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
    protected BulletData _bulletdata;
    public Bullet _lastbullet;

    public WeaponConfig WeaponCfg { get { return _weaponCfg; } }
    protected WeaponConfig _weaponCfg;

    /* Call Back */
    public Action<float> OnReloadCDUpdate;
    public Action<int> OnMagazineChange;

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        this._weaponCfg = m_unitconfig as WeaponConfig;
        _baseUnitConfig = m_unitconfig;
        InitWeaponAttribute(m_owner is PlayerShip);
        base.Initialization(m_owner, m_unitconfig);
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

        _firepointindex = 0;
        _targetindex = 0;

    }

    public override void InitializationEditorUnit(PlayerEditShip ship, BaseUnitConfig m_unitconfig)
    {
        this._weaponCfg = m_unitconfig as WeaponConfig;
        base.InitializationEditorUnit(ship, m_unitconfig);
        InitWeaponAttribute(true);
    }

    public override void Start()
    {
        base.Start();

    }

    public override void Update()
    {
        base.Update();
        
        //ProcessWeapon();
    }

    protected override void OnDestroy()
    {
        weaponAttribute?.Destroy();
        base.OnDestroy();
    }

    public struct RV_WeaponTargetInfo : IComparable<RV_WeaponTargetInfo>
    {
        public int targetIndex;
        public float distanceToTarget;
        public float3 targetPos;
        public float3 targetDirection;

        public int CompareTo(RV_WeaponTargetInfo other)
        {
            return distanceToTarget.CompareTo(other.distanceToTarget);
        }
    }

    [BurstCompile]
    public struct FindWeaponTargetsJob : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public NativeArray<float> job_attackRange;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_selfPos;
        [Unity.Collections.ReadOnly] public NativeArray<int> job_maxTargetCount;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetsPos;
        //这里返回的时对应的target在list中的index

        public NativeArray<RV_WeaponTargetInfo> rv_targetsInfo;


    
        RV_WeaponTargetInfo tempinfo;
        int index;
        public void Execute(int startIndex, int count)
        {
            NativeList<RV_WeaponTargetInfo> tempinfolist; 
            for (int i = startIndex; i < startIndex + count; i++)
            {
                tempinfolist  = new NativeList<RV_WeaponTargetInfo>(Allocator.Temp);
                //找到所有在范围内的target
                for (int n = 0; n < job_targetsPos.Length; n++)
                {
                    if (math.distance(job_selfPos[i], job_targetsPos[n]) <= job_attackRange[i])
                    {
                        tempinfo.targetPos = job_targetsPos[n];
                        tempinfo.targetIndex = n;
                        tempinfo.targetDirection = math.normalize(job_targetsPos[n] - job_selfPos[i]);
                        tempinfo.distanceToTarget = math.distance(job_targetsPos[n], job_selfPos[i]);
                        tempinfolist.Add(tempinfo);
                    }
                }

                index = 0;
                for (int c = 0; c < i; c++)
                {
                    index += job_maxTargetCount[c];
                }

               
                if(tempinfolist.Length > 0)
                {
                    //全部找完了之后进行排序
                    tempinfolist.Sort();
                    //排序之后按照最大目标数量进行结果装填
                    for (int s = 0; s < job_maxTargetCount[i]; s++)
                    {
                        //当前的index不大于 临时目标列表的长度
                        if (s <= tempinfolist.Length - 1)
                        {
                            rv_targetsInfo[index + s] = tempinfolist[s];
                        }
                        else
                        {
                            rv_targetsInfo[index + s] = tempinfolist[s - tempinfolist.Length];
                        }
                    }
                }
                else
                {
                    for (int s = 0; s < job_maxTargetCount[i]; s++)
                    {
                        tempinfo.targetIndex = -1;
                        rv_targetsInfo[index + s] = tempinfo;
                    }
                }
                tempinfolist.Dispose();
            }
      
        }
    }



    public struct CalculateWeaponRotateJob : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetPos;


        public NativeArray<float> rv_rotation;
        public void Execute(int startIndex, int count)
        {
            

        }
    }


    public virtual void HandleOtherWeaponRotation()
    {

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

    

    public virtual void ProcessWeapon()
    {
        if (!_isProcess)
            return;
        HandleOtherWeaponRotation();
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
    }

    public virtual void WeaponOff()
    {
        _isWeaponOn = false;
        _lastbullet = null;

    }



    public virtual void WeaponReady()
    {
        //Debug.Log(this.gameObject + " : WeaponReady");
        if(_isWeaponOn)
        {
            weaponstate.ChangeState(WeaponState.Start);
        }
    }

    public virtual void WeaponStart()
    {
        //Debug.Log(this.gameObject + " : WeaponStart");
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
                weaponstate.ChangeState(WeaponState.End);
                return;
                //ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDStart, UID);
            }
        }
        //if(targetmode == WeaponTargetMode.Mutipule)
        //{
        //    float d1;
        //    float d2;
        //    // 筛选目标候选,按照距离排序
        //    _targetcandidates.Sort((x,y) => 
        //    {
        //        d1 = transform.position.SqrDistanceXY(x.gameObject.transform.position);
        //        d2 = transform.position.SqrDistanceXY(y.gameObject.transform.position);
        //        return d1.CompareTo(d2);
        //    });
        if (_firepointindex >= firePoint.Length)
        {
            _firepointindex = 0;
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
        int firecount = CalculateFireCount();
        SoundManager.Instance.Play(_weaponCfg.FireAudioClip, SoundManager.SoundType.SFX);
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

                }
                break;
            case WeaponControlType.Autonomy:
                Debug.Log(this.gameObject + " : Autonomy Firing!!!");
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
                    _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(target);
                    //_lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
                 
                    _lastbullet.Shoot();
                }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);
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
                        _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                        _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                        _lastbullet.SetFirePoint(trs.gameObject);
                        _lastbullet.SetTarget(null);
                        _lastbullet.SetOwner(this);
                        // _lastbullet.PoolableSetActive();
                        _lastbullet.Initialization();
                       
                        _lastbullet.Shoot();
                    }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);
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
                    _lastbullet.InitialmoveDirection  = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(target);
                    // _lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
                  
                    _lastbullet.Shoot();
                }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);
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
                    _lastbullet.InitialmoveDirection  = MathExtensionTools.GetRandomDirection(trs.up, scatter);
                    _lastbullet.transform.rotation = Quaternion.LookRotation(_lastbullet.transform.forward, _lastbullet.InitialmoveDirection);
                    _lastbullet.SetFirePoint(trs.gameObject);
                    _lastbullet.SetTarget(null);
                    //_lastbullet.PoolableSetActive();
                    _lastbullet.SetOwner(this);
                    _lastbullet.Initialization();
            
                    _lastbullet.Shoot();
                }, (LevelManager.Instance.currentLevel as BattleLevel).BulletPool.transform);
            }
            aimingtype = temptype;

        }



    }


    public virtual void WeaponBetweenDelay()
    {
        //Debug.Log(this.gameObject + " : WeaponBetweenDelay");
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
        //Debug.Log(this.gameObject + " : WeaponCharging");

        _chargeCounter -= Time.deltaTime;
        if (_chargeCounter < 0)
        {
            _chargeCounter = weaponAttribute.ChargeTime;
            weaponstate.ChangeState(WeaponState.Charged);
        }
        if(_isWeaponOn)
        {
            weaponstate.ChangeState(WeaponState.Start);
        }
    }



    public virtual void WeaponCharged()
    {
        //Debug.Log(this.gameObject + " : WeaponCharged");
        _isChargeFire = true;
        if(_isWeaponOn)
        {
            weaponstate.ChangeState(WeaponState.Start);
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
        //Debug.Log(this.gameObject + " : WeaponAfterDelay");
        _afterDelayCounter -= Time.deltaTime;
        if (_afterDelayCounter < 0)
        {
            _afterDelayCounter = weaponAttribute.AfterDelay;
            weaponstate.ChangeState(WeaponState.End);
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
                weaponstate.ChangeState(WeaponState.Reload);
                ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDStart, UID);

            }
            else
            {
                weaponstate.ChangeState(WeaponState.Recover);
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
                ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDEnd, UID);
            }
        }
    }
    public virtual void WeaponRecover()
    {
        //Debug.Log(this.gameObject + " : WeaponRecover");
        _isChargeFire = false;
        WeaponOff();
        weaponstate.ChangeState(WeaponState.Ready);
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
    public void InitCoreData()
    {
        HpComponent.BindHPChangeAction(OnPlayerCoreHPChange, false);
    }

    private void OnPlayerCoreHPChange()
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.CoreHPChange);
        var percent = HpComponent.HPPercent;
        LevelManager.Instance.OnPlayerShipCoreHPPercentChange(percent);
    }
}

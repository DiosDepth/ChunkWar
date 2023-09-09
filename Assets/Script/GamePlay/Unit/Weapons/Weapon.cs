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


public struct DamageResultInfo
{
    public int Damage;
    public bool IsCritical;
    public WeaponDamageType DamageType;
    public float ShieldDamagePercent;

    public int DamageTemp;
}


[System.Serializable]
public class WeaponAttribute : UnitBaseAttribute
{
    /// <summary>
    /// �����˺�
    /// </summary>
    public float BaseDamage { get; protected set; }

    public WeaponDamageType DamageType { get; protected set; }

    /// <summary>
    /// �����˺�����
    /// </summary>
    public float BaseDamageModifyValue
    {
        get;
        protected set;
    }

    public float DamageRatioMin { get; protected set; }
    public float DamageRatioMax { get; protected set; }

    /// <summary>
    /// ������
    /// </summary>
    public float CriticalRatio { get; protected set; }

    /// <summary>
    /// �����˺�, 100�ٷֱȵ�λ
    /// </summary>
    public float CriticalDamagePercent { get; protected set; }



    /// <summary>
    /// �ᴩ��
    /// </summary>
    public byte Transfixion { get; protected set; }

    /// <summary>
    /// �ᴩ˥��, 100�ٷֱȵ�λ
    /// </summary>
    public float TransfixionReduce { get; protected set; }

    public float ShieldDamagePercent { get; protected set; }

    public float Rate;

    public float ChargeTime;
    public float ChargeCost;

    public bool MagazineBased = true;


    /// <summary>
    /// �ܵ�ҩ����
    /// </summary>
    public int MaxMagazineSize
    {
        get;
        protected set;
    }

    

    /// <summary>
    /// װ��CD
    /// </summary>
    public float ReloadTime
    {
        get; protected set;
    }

    /// <summary>
    /// ������
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
    private float BaseShieldDamagePercent;

    /// <summary>
    /// �����˺�
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
            var damageRatio = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyDamagePercent);
            var enemy = _parentUnit._owner as AIShip;
            float hardLevelRatio = 0;
            if (enemy != null)
            {
                hardLevelRatio = GameHelper.GetEnemyDamageByHardLevel(enemy.AIShipCfg.HardLevelCfg);
            }

            var ratio = UnityEngine.Random.Range(DamageRatioMin, DamageRatioMax);
            var damage = Mathf.Clamp(BaseDamage * (1 + damageRatio + hardLevelRatio / 100f) * ratio, 0, int.MaxValue);
            Damage =  Mathf.RoundToInt(damage);
        }

        return new DamageResultInfo
        {
            Damage = Damage,
            IsCritical = isCritical,
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
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.TransfixionReducePercent, CalculateTransfixionPercent);
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
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.TransfixionReducePercent, CalculateTransfixionPercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.CriticalDamagePercentAdd, CalculateCriticalDamagePercent);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldDamageAdd, CalculateShieldDamagePercent);
        }
    }

    /// <summary>
    /// ˢ���˺�����
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
/// �����indexָ���ǣ���ǰ��target��Ai manager.targetActiveUnitList�е�λ��
/// distance �� direction���ǵ�ǰ��weapon ����targetλ����˵��
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
    /// CD�ٷֱ�
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

    protected WeaponConfig _weaponCfg;

    /* Call Back */
    public Action<float> OnReloadCDUpdate;
    public Action<int> OnMagazineChange;

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
        _targetindex = 0;

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
        //���ﷵ�ص�ʱ��Ӧ��target��list�е�index
        public NativeArray<RV_WeaponTargetInfo> rv_targetsInfo;


    
        RV_WeaponTargetInfo tempinfo;
        int index;
        public void Execute(int startIndex, int count)
        {
            NativeList<RV_WeaponTargetInfo>  tempinfolist = new NativeList<RV_WeaponTargetInfo>(Allocator.Temp);
            for (int i = startIndex; i < startIndex + count; i++)
            {
    
                //�ҵ������ڷ�Χ�ڵ�target
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
                for (int c = 0; c < startIndex; c++)
                {
                    index += job_maxTargetCount[c];
                }

               
                if(tempinfolist.Length > 0)
                {
                    //ȫ��������֮���������
                    tempinfolist.Sort();
                    //����֮�������Ŀ���������н��װ��
                    for (int s = 0; s < job_maxTargetCount[i]; s++)
                    {
                        //��ǰ��index������ ��ʱĿ���б�ĳ���
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
            }
            tempinfolist.Dispose();
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
                //ShipPropertyEvent.Trigger(ShipPropertyEventType.ReloadCDStart, UID);
            }
        }


        // �������ʵ�target
        /*
        if(aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetBased)
        {
            targetList.Clear();

            _targetcandidates = Physics2D.OverlapCircleAll(this.transform.position, weaponAttribute.WeaponRange, mask);

            if(_targetcandidates.Length > 0)
            {
                if(targetmode == WeaponTargetMode.Single)
                {
                    GameObject temptarget = null;
                    float tempsqedistance;
                    float shortestdistance = float.MaxValue;
                    // ɸѡĿ���ѡ
                    for (int i = 0; i < _targetcandidates.Length; i++)
                    {
                        if(_targetcandidates[i].tag == this.tag)
                        {
                            continue;
                        }
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
                    // ɸѡĿ���ѡ,���վ�������
                    _targetcandidates.Sort((x,y) => 
                    {
                        d1 = transform.position.SqrDistanceXY(x.gameObject.transform.position);
                        d2 = transform.position.SqrDistanceXY(y.gameObject.transform.position);
                        return d1.CompareTo(d2);
                    });
                    int tempcount = 0;
                    for (int i = 0; i < _targetcandidates.Length; i++)
                    {
                        if(_targetcandidates[i].tag == this.tag)
                        {
                            continue;
                        }
                        else
                        {
                            
                            targetList.Add(_targetcandidates[i].gameObject);
                            tempcount++;
                            if( tempcount >= maxTargetCount)
                            {
                                break;
                            }
                        }
              
                    }
                }
            }
        }
        */



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
                    _lastbullet.InitialmoveDirection = _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);
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
                    _lastbullet.InitialmoveDirection = _lastbullet.InitialmoveDirection = MathExtensionTools.GetRandomDirection(trs.up, scatter);

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
    }



    public virtual void WeaponCharged()
    {
        //Debug.Log(this.gameObject + " : WeaponCharged");
        _isChargeFire = true;
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

    public override void Death()
    {
        base.Death();
    }

    public override void Restore()
    {
        base.Restore();
    }

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        return base.TakeDamage(ref info);
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
    /// ��ʼ��������Ϣ
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

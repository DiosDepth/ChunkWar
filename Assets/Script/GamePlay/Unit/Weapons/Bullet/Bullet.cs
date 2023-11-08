
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;


public enum BulletType
{
    Missile,
    Ballistic,
    Energy,
}

public class Bullet : MonoBehaviour,IPoolable,IPauseable
{
    public BulletType type;
    public OwnerType ownertype;
    public DamagePattern damagePattern = DamagePattern.Target;

    

    [Header("---VFXSettings---")]
    public string DeathVFX = "HitVFX";

    [Header("---IndicatorSettings---")]
    public bool hasIndicator;

  
    [BoxGroup("Indicator")]
    public IndicatorShape shape;

    [ShowIf("shape", IndicatorShape.Circle)]
    [BoxGroup("Indicator")]
    public float angle = 360;

    [ShowIf("shape", IndicatorShape.Circle)]
    [BoxGroup("Indicator")]
    public int quality = 16;

   
    [HideInInspector]
    protected string IndicatorPath = "Prefab/Unit/Indicator/DamageIndicator";
    protected DamageIndicator _indicator;

    [HideInInspector]
    public Unit Owner { get { return _owner; } }
    protected Unit _owner;
    protected GameObject firepoint;

    [HideInInspector]
    public GameObject initialTarget;
    [HideInInspector]
    public List<IDamageble> prepareDamageTargetList = new List<IDamageble>();
    protected List<IDamageble> damagedTargetList = new List<IDamageble>();

    [HideInInspector]
    public Vector2 InitialmoveDirection { get { return _initialmoveDirection; } set { _initialmoveDirection = value; } }
    protected Vector2 _initialmoveDirection = Vector2.up;
    [HideInInspector]
    public bool IsApplyDamageAtThisFrame { get { return _isApplyDamageAtThisFrame; } }
    protected bool _isApplyDamageAtThisFrame;
    public bool IsDeathAtThisFrame { get { return _isDeathAtThisFrame; } }
    protected bool _isDeathAtThisFrame;
    protected bool _isUpdate;

    /// <summary>
    /// 最大穿透数
    /// </summary>
    public int TransFixionCount { get { return transfixionCount; } }
    protected int transfixionCount;
    protected BulletConfig _bulletCfg;

    private Action onHitAction;

    protected virtual void Awake()
    {

    }

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
        SetUpdate(true);

        if (_owner is Weapon)
        {
            transfixionCount = (_owner as Weapon).weaponAttribute.Transfixion;
        }
    }

    public virtual void SetUp(BulletConfig cfg, Action hitAction = null)
    {
        this._bulletCfg = cfg;
        onHitAction = hitAction;
    }

    public struct FindBulletDamageTargetJob : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetsPos;
        [Unity.Collections.ReadOnly] public int job_targesTotalCount;
        [Unity.Collections.ReadOnly] public NativeArray<ProjectileJobInitialInfo> job_JobInfo;

        public NativeArray<int> rv_findedTargetsCount;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> rv_findedTargetIndex;
        private int length;
        public void Execute(int startIndex, int count)
        {
    
            NativeList<int> targetindexlist;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                int index = 0;
                targetindexlist = new NativeList<int>(Allocator.Temp);

                if (job_JobInfo[i].damageType == 1)
                {
                    for (int n = 0; n < job_targetsPos.Length; n++)
                    {
                        if (math.distance(job_JobInfo[i].update_targetPos, job_targetsPos[n]) <= 0.01f)
                        {
                            rv_findedTargetIndex[i * job_targesTotalCount + index] = n;
                            index++;
                            rv_findedTargetsCount[i] = index;
                            break;
                        }
                    }
                }

                if (job_JobInfo[i].damageType == 2)
                {
                    for (int n = 0; n < job_targetsPos.Length; n++)
                    {
                        if (math.distance(job_JobInfo[i].update_selfPos, job_targetsPos[n]) <= job_JobInfo[i].damageRadius)
                        {
                            rv_findedTargetIndex[i * job_targesTotalCount + index] = n;
                            index++;
                            rv_findedTargetsCount[i] = index;
                        }
                    }
                }


                if (job_JobInfo[i].damageType ==3)
                {
                    for (int n = 0; n < job_targetsPos.Length; n++)
                    {
                        if (math.distance(job_JobInfo[i].update_targetPos, job_targetsPos[n]) <= 0.01f)
                        {
                            rv_findedTargetIndex[i * job_targesTotalCount + index] = n;
                            index++;
                            rv_findedTargetsCount[i] = index;
                            break;
                        }
                    }
                }


            }
        }
    }

    public virtual void SetUpdate(bool isupdate)
    {
        PoolableSetActive(isupdate);
        _isUpdate = isupdate;
    }
    public virtual void SetOwner(Unit owner)
    {
        _owner = owner;

    }

    public virtual void SetTarget(GameObject m_target)
    {
        initialTarget = m_target;
    }
    public virtual void SetFirePoint(GameObject m_firepoint)
    {
        firepoint = m_firepoint;
    }
    public virtual void Shoot()
    {
        if(hasIndicator)
        {
            ShowIndicator();
        }
    }

    public virtual void ShowIndicator()
    {
        if (_indicator == null) { return; }
        _indicator.ShowIndicator();
    }

    public virtual void UpdateIndicator()
    {
        if(_indicator== null) { return; }
        _indicator.UpdateIndicator();

    }
    public virtual void RemoveIndicator()
    {
        if (_indicator == null) { return; }
        _indicator.RemoveIndicator();
  
    }

    public virtual void UpdateBullet()
    {
        if(hasIndicator)
        {
            UpdateIndicator();
        }
    }

    protected virtual void OnEnable()
    {
 
    }
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }
    protected virtual void OnDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
    }

    public virtual void Death(UnitDeathInfo info)
    {

        if (ownertype == OwnerType.AI && _owner is AIAdditionalWeapon)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
            //AIManager.Instance.RemoveBullet(this);
        }
        if (ownertype == OwnerType.Player && (_owner is ShipMainWeapon || _owner is ShipAdditionalWeapon))
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
            //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveBullet(this);
        }

        if(hasIndicator)
        {
            RemoveIndicator();
        }
        //PlayVFX(DeathVFX, this.transform.position);
        PoolableDestroy();
    }

    public virtual void PlayVFX(GeneralEffectConfig cfg, Vector2 pos)
    {
        if(cfg == null || string.IsNullOrEmpty(cfg.EffectName))
            return;

        EffectManager.Instance.CreateEffect(cfg, pos);
    }


    public virtual void PoolableReset()
    {
        SetUpdate(false);
        onHitAction = null;
        _isApplyDamageAtThisFrame = false;
        _isDeathAtThisFrame = false;
        prepareDamageTargetList.Clear();
        initialTarget = null;
    }

    public virtual void PoolableDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void PoolableSetActive(bool isactive = true)
    {

        this.gameObject.SetActive(isactive);
    }

    public virtual bool ApplyDamageAllTarget()
    {
        if (prepareDamageTargetList == null || prepareDamageTargetList.Count == 0) { return false; }


        if (damagePattern == DamagePattern.Target && initialTarget != null)
        {
            ApplyDamage(initialTarget.GetComponent<IDamageble>());
        }

        if(damagePattern == DamagePattern.PointRadius)
        {
            ///Explode Damage
            LevelManager.Instance.PlayerCreateExplode();
            for (int i = 0; i < prepareDamageTargetList.Count; i++)
            {
                ApplyDamage(prepareDamageTargetList[i]);
            }
        }

        if(damagePattern == DamagePattern.Touched)
        {
            ApplyDamage(prepareDamageTargetList[0]);
        }
        prepareDamageTargetList.Clear();
        return true;
    }

    public virtual void ApplyDamage(IDamageble damageble)
    {
        _isApplyDamageAtThisFrame = false;
        var damage = (_owner as Weapon).weaponAttribute.GetDamage();
        damageble.TakeDamage(damage);
        damagedTargetList.Add(damageble);
    }

    /// <summary>
    /// 命中的效果，例如屏幕震动，如果同时命中多个目标或者爆炸，只触发一次
    /// </summary>
    protected virtual void OnHitEffect()
    {
        onHitAction?.Invoke();
    }

    public virtual void GameOver()
    {
        PoolableDestroy();
    }

    public virtual void PauseGame()
    {
       
        
    }

    public virtual void UnPauseGame()
    {
       
    }
}

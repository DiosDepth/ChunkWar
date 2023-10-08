
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;


public enum BulletType
{
    Missile,
    Ballistic,
    Energy,
}


public enum AvaliableBulletType
{
    None,
    BaseBeam_AI,
    BaseBullet_AI,
    BaseChainBeam_AI,
    BaseHommingBullet_AI,
    BaseInstanceHit_AI,
    BasePassThroughMissile_AI,
    BaseBullet_Player,
    BaseBullet02_Player,
    BaseBeam_Player,
    BaseInstanceHit_Player,
    BaseHommingBullet_Player,
    BaseChainBeam_Player,
    BasePassThroughMissile_Player,

}
public class Bullet : MonoBehaviour,IPoolable,IPauseable
{
    public BulletType type;
    public OwnerType ownertype;
    public DamagePattern damagePattern = DamagePattern.Target;

    [ShowIf("damagePattern", DamagePattern.PointRadius)]
    public float damageRadius = 5;

    [Header("---VFXSettings---")]
    public string ShootVFX = "HitVFX";
    public string HitVFX = "HitVFX";
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
    protected bool _isUpdate;
   
    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
        SetUpdate(true);
        if(ownertype == OwnerType.AI && _owner is AIAdditionalWeapon)
        {
            AIManager.Instance.AddBullet(this);
        }
        if(ownertype == OwnerType.Player && (_owner is ShipMainWeapon || _owner is ShipAdditionalWeapon))
        {
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddBullet(this);
        }
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
            AIManager.Instance.RemoveBullet(this);
        }
        if (ownertype == OwnerType.Player && (_owner is ShipMainWeapon || _owner is ShipAdditionalWeapon))
        {
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveBullet(this);
        }

        if(hasIndicator)
        {
            RemoveIndicator();
        }
        PlayVFX(DeathVFX, this.transform.position);
        PoolableDestroy();
    }

    public virtual void PlayVFX(string m_vfxname, Vector3 pos)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + m_vfxname, true, (obj) =>
        {
            obj.transform.position = pos;
            obj.GetComponent<ParticleController>().PoolableSetActive();
            obj.GetComponent<ParticleController>().PlayVFX();
        });
    }


    public virtual void PoolableReset()
    {
        SetUpdate(false);
        _isApplyDamageAtThisFrame = false;
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

    public virtual void ApplyDamageAllTarget()
    {
        if (prepareDamageTargetList == null || prepareDamageTargetList.Count == 0) { return; }


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
    }

    public virtual void ApplyDamage(IDamageble damageble)
    {
        _isApplyDamageAtThisFrame = false;
        var damage = (_owner as Weapon).weaponAttribute.GetDamage();
        damageble.TakeDamage(damage);
        damagedTargetList.Add(damageble);
    }

    public virtual void GameOver()
    {

    }

    public virtual void PauseGame()
    {
       
        
    }

    public virtual void UnPauseGame()
    {
       
    }
}

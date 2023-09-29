using Unity.Burst;
using Unity.Collections;
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
    BaseBullet_AI,
    BaseHommingBullet_AI,
    BaseBullet_Player,
    BaseBullet02_Player,
    BaseBeam_Player,
    BaseInstanceHit_Player,
    BaseHommingBullet_Player,
    BaseChainBeam_Player,
}
public class Bullet : MonoBehaviour,IPoolable
{
    public BulletType type;
    public OwnerType ownertype;
    public DamageType damageType = DamageType.Point;
    public float damageRadius = 5;

    public string ShootVFX = "HitVFX";
    public string HitVFX = "HitVFX";
    public string DeathVFX = "HitVFX";
    protected Unit _owner;
    protected GameObject firepoint;

    public GameObject target;
    public Vector2 InitialmoveDirection { get { return _initialmoveDirection; } set { _initialmoveDirection = value; } }
    protected Vector2 _initialmoveDirection = Vector2.up;

    public bool IsApplyDamageAtThisFrame { get { return _isApplyDamageAtThisFrame; } }
    protected bool _isApplyDamageAtThisFrame;
    protected bool _isUpdate;
    public virtual void Initialization()
    {
        if(ownertype == OwnerType.AI && _owner is AIWeapon)
        {
            AIManager.Instance.AddBullet(this);
        }
        if(ownertype == OwnerType.Player && _owner is ShipWeapon)
        {
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddBullet(this);
        }
    }

    public struct FindBulletDamageTargetJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float3> job_targetsPos;
        [ReadOnly] public int job_targesTotalCount;
        [ReadOnly] public NativeArray<ProjectileJobInitialInfo> job_JobInfo;

        public NativeArray<int> rv_findedTargetsCount;
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
                

            }
        }
    }


    public virtual void SetOwner(Unit owner)
    {
        _owner = owner;
    }

    public virtual void SetTarget(GameObject m_target)
    {
        target = m_target;
    }
    public virtual void SetFirePoint(GameObject m_firepoint)
    {
        firepoint = m_firepoint;
    }
    public virtual void Shoot()
    {

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

    public virtual void Death(UnitDeathInfo info)
    {
        if (ownertype == OwnerType.AI && _owner is AIWeapon)
        {
            AIManager.Instance.RemoveBullet(this);
        }
        if (ownertype == OwnerType.Player && _owner is ShipWeapon)
        {
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveBullet(this);
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
        _isApplyDamageAtThisFrame = false;
        target = null;
    }

    public virtual void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void PoolableSetActive(bool isactive = true)
    {

        this.gameObject.SetActive(isactive);
    }

    public virtual void ApplyDamage(IDamageble damageble)
    {

    }




}

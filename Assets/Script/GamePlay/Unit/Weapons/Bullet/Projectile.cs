
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ProjectileMovementType
{
    Straight = 1,
    TowardToTarget = 2,
    FollowTarget = 3,
    StraightToPos = 4,
}
public enum DamagePattern
{
    Target = 1,
    PointRadius = 2,
    Touched = 3,
}

public enum DamageTriggerPattern
{
    Collider,
    PassTrough,
    Point,
    Target,
}

public struct ProjectileJobInitialInfo
{
    public float3 update_targetPos;
    public float3 update_selfPos;
    public float lifeTime;
    public float update_lifeTimeRemain;
    public float maxSpeed;
    public float initialSpeed;
    public float update_currentSpeed;
    public float acceleration;
    public float rotSpeed;
    public float3 initialDirection;
    public float3 initialTargetPos;
    public float3 update_moveDirection;
    public int movementType;
    public int damageType;
    public float damageRadius;

    /// <summary>
    /// ����Jobʱ��Ҫ���Ĳ����� �󲿷��Ǿ�̬�� С������Ҫ����
    /// </summary>
    /// <param name="m_update_selfPos"></param>
    /// <param name="m_lifttime"></param>
    /// <param name="m_maxspeed"></param>
    /// <param name="m_initialspeed"></param>
    /// <param name="m_acceleration"></param>
    /// <param name="m_rotspeed"></param>
    /// <param name="m_initialdirection"></param>
    public ProjectileJobInitialInfo(float3 m_update_targetPos, float3 m_update_selfPos, float m_lifttime, float m_maxspeed, float m_initialspeed, float m_acceleration, float m_rotspeed, float3 m_initialdirection, float3 m_initialTargetPos, int m_movementType , int m_damageType, float m_damageRadius)
    {
        update_targetPos = m_update_targetPos;
        update_selfPos = m_update_selfPos;
        lifeTime = m_lifttime;
        update_lifeTimeRemain = lifeTime;
        maxSpeed = m_maxspeed;
        initialSpeed = m_initialspeed;
        update_currentSpeed = initialSpeed;
        acceleration = m_acceleration;
        rotSpeed = m_rotspeed;
        initialDirection = m_initialdirection;
        initialTargetPos = m_initialTargetPos;
        update_moveDirection = initialDirection;
        movementType = m_movementType;
        damageType = m_damageType;
        damageRadius = m_damageRadius;
    }
}

/// <summary>
/// ��Ҫÿһ֡���µĲ����� �ӵ��ƶ�������һ��������������ݣ� ͬʱҲ����µ�BulletJobInitialInfo��
/// </summary>
public struct ProjectileJobRetrunInfo
{
    public float lifeTimeRemain;
    public float3 moveDirection;
    public float currentSpeed;
    public float3 deltaMovement;
    public bool islifeended;
    public float rotation;
}

public class Projectile : Bullet, IDamageble
{
    [Header("---ProjectileSettings---")]
    public Rigidbody2D rb;
    public Collider2D bulletCollider;
    
    public ProjectileMovementType movementType = ProjectileMovementType.Straight;
    public DamageTriggerPattern damageTriggerPattern = DamageTriggerPattern.Collider;

    [Header("---CommanSettings---")]
    public float lifeTime = 10;
    public float maxSpeed = 2.5f;
    public float rotSpeed = 180f;
    public float initialSpeed = 20f;
    public float acceleration = 0.25f;
    public int MissileHPBase;

    [Header("---PassThroughSettings---")]
    [ShowIf("damageTriggerPattern", DamageTriggerPattern.PassTrough)]
    public int maxPassThroughCount = 5;


    [HideInInspector]
    public  int PassThroughCount { get { return passThroughCount; } }
    protected int passThroughCount = 0;

    /// <summary>
    /// �Ƿ���Ա����أ���ӵ��HP
    /// </summary>
    public bool IsinterceptTarget = true;

    /// <summary>
    /// Ѫ���������
    /// </summary>
    public GeneralHPComponet HpComponent;

    //private Coroutine startmovingCoroutine;
    protected float _beforemovetimestamp;

    private BulletProjectileConfig _projectileCfg;

    public override void SetUp(BulletConfig cfg)
    {
        base.SetUp(cfg);
        _projectileCfg = cfg as BulletProjectileConfig;
        lifeTime = _projectileCfg.LifeTime;
        maxSpeed = _projectileCfg.MaxSpeed;
        rotSpeed = _projectileCfg.RotationSpeed;
        initialSpeed = _projectileCfg.InitialSpeed;
        acceleration = _projectileCfg.Acceleration;
        MissileHPBase = _projectileCfg.MissileHP;
    }

    public override void Shoot()
    {
        base.Shoot();

        PoolableSetActive();
    }

    public override void ShowIndicator()
    {
        if (damagePattern == DamagePattern.PointRadius && damageTriggerPattern == DamageTriggerPattern.Point)
        {
            if( shape == IndicatorShape.Circle)
            {
               
                PoolManager.Instance.GetObjectSync(IndicatorPath, true, (obj) => 
                {
                    _indicator = obj.GetComponent<DamageIndicator>();
                    _indicator.Initialization();
                    _indicator.CreateIndicator(shape, Vector3.zero, damageRadius,angle,quality);
                    Vector3 initialtargetpos;
                    if((Owner as Weapon).aimingtype == WeaponAimingType.Directional)
                    {
                        initialtargetpos = transform.position + transform.up * Owner.baseAttribute.WeaponRange;
                    }
                    else
                    {
                        initialtargetpos = initialTarget.transform.position;
                    }
                    _indicator.transform.position = initialtargetpos;
                }, (LevelManager.Instance.currentLevel as BattleLevel).IndicatorPool.transform);
            }
        }
        base.ShowIndicator();
    }

    public override void Initialization()
    {
        base.Initialization();
        bulletCollider = transform.GetComponentInChildren<Collider2D>();
        ///HP
        InitMissileHP();
        passThroughCount = maxPassThroughCount;
    }

    private void InitMissileHP()
    {
        if (!IsinterceptTarget)
            return;

        if (_owner != null || _owner._owner == null)
            return;

        int targetHP = 0;
        if(_owner._owner is PlayerShip)
        {
            var missileHPPercent = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.MissileHPPercent);
            missileHPPercent = Mathf.Clamp(missileHPPercent, -100, float.MaxValue);
            targetHP = Mathf.RoundToInt(MissileHPBase * (1 + missileHPPercent / 100f));
        }
        else if (_owner._owner is AIShip)
        {
            ///������˵���Ѫ��
            var aiShip = _owner._owner as AIShip;
            var hardLevelItem =  GameHelper.GetEnemyHardLevelItem(aiShip.AIShipCfg.HardLevelGroupID);
            var hpadd = hardLevelItem.MissileHPAdd;
            var hpRatio = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnemyHPPercent);
            targetHP = Mathf.RoundToInt((MissileHPBase + hpadd) * (1 + hpRatio / 100f));
            targetHP = Mathf.Max(0, targetHP);
        }
        HpComponent = new GeneralHPComponet(targetHP, targetHP);
    }

    [BurstCompile]
    public struct CalculateProjectileMovementJobJob : IJobParallelForBatch
    {

        [Unity.Collections.ReadOnly] public NativeArray<ProjectileJobInitialInfo> job_jobInfo;
        [Unity.Collections.ReadOnly] public float job_deltatime;


        public NativeArray<ProjectileJobRetrunInfo> rv_bulletJobUpdateInfos;

        private ProjectileJobRetrunInfo bulletJobUpdateInfo;
        private float currentSpeed;
        private float3 deltaMovement;
        private float3 rotateDirection;
        private float3 targetDirection;
        private float3 moveDirection;
        private float targetangle;
        public void Execute(int startIndex, int count)
        {

            for (int i = startIndex; i < startIndex + count; i++)
            {
                //ֱ���˶���Job�㷨 Straight
                if (job_jobInfo[i].movementType == 1)
                {
                    currentSpeed = job_jobInfo[i].update_currentSpeed + job_jobInfo[i].acceleration;
                    currentSpeed = math.clamp(currentSpeed, 0, job_jobInfo[i].maxSpeed);
                    moveDirection = job_jobInfo[i].update_moveDirection;

                    deltaMovement = job_jobInfo[i].update_selfPos + (moveDirection * currentSpeed * job_deltatime);

                    //����BulletJob��ֵ

                    bulletJobUpdateInfo.deltaMovement = deltaMovement;
                    bulletJobUpdateInfo.currentSpeed = currentSpeed;
                    bulletJobUpdateInfo.lifeTimeRemain = job_jobInfo[i].update_lifeTimeRemain - job_deltatime;


                    if (bulletJobUpdateInfo.lifeTimeRemain <= 0)
                    {
                        bulletJobUpdateInfo.lifeTimeRemain = 0;
                        bulletJobUpdateInfo.islifeended = true;
                    }
                    else
                    {
                        bulletJobUpdateInfo.islifeended = false;
                    }
                    bulletJobUpdateInfo.moveDirection = moveDirection;
                }

                //Ŀ�굼���Job�㷨 TowardToTarget
                if (job_jobInfo[i].movementType == 2)
                {
                    targetDirection = math.normalize(job_jobInfo[i].update_targetPos - job_jobInfo[i].update_selfPos);
                    currentSpeed = job_jobInfo[i].update_currentSpeed + job_jobInfo[i].acceleration;
                    currentSpeed = math.clamp(currentSpeed, 0, job_jobInfo[i].maxSpeed);

                    //���ƫת������
                    rotateDirection = math.normalize(targetDirection - job_jobInfo[i].update_moveDirection);
                    //�����ǰ�ƶ�����
                    moveDirection = math.normalize( job_jobInfo[i].update_moveDirection + job_jobInfo[i].rotSpeed * rotateDirection * job_deltatime);
                    //���ߵ�ǰ�ƶ��������deltamovement
                    deltaMovement = job_jobInfo[i].update_selfPos + (moveDirection * currentSpeed * job_deltatime);
                    

                    bulletJobUpdateInfo.deltaMovement = deltaMovement;
                    bulletJobUpdateInfo.currentSpeed = currentSpeed;
                    bulletJobUpdateInfo.lifeTimeRemain = job_jobInfo[i].update_lifeTimeRemain - job_deltatime;


                    if (bulletJobUpdateInfo.lifeTimeRemain <= 0)
                    {
                        bulletJobUpdateInfo.lifeTimeRemain = 0;
                        bulletJobUpdateInfo.islifeended = true;
                    }
                    else
                    {
                        bulletJobUpdateInfo.islifeended = false;
                    }
                    bulletJobUpdateInfo.moveDirection = moveDirection;
                }

                //Ŀ����ٵ�Job�㷨 FollowTarget
                if (job_jobInfo[i].movementType == 3)
                {
                    targetDirection = math.normalize(job_jobInfo[i].update_targetPos - job_jobInfo[i].update_selfPos);
                    currentSpeed = job_jobInfo[i].update_currentSpeed + job_jobInfo[i].acceleration;
                    currentSpeed = math.clamp(currentSpeed, 0, job_jobInfo[i].maxSpeed);

                    moveDirection = targetDirection;
                    //���ߵ�ǰ�ƶ��������deltamovement
                    deltaMovement = job_jobInfo[i].update_selfPos + (moveDirection * currentSpeed * job_deltatime);


                    bulletJobUpdateInfo.deltaMovement = deltaMovement;
                    bulletJobUpdateInfo.currentSpeed = currentSpeed;
                    bulletJobUpdateInfo.lifeTimeRemain = job_jobInfo[i].update_lifeTimeRemain - job_deltatime;


                    if (bulletJobUpdateInfo.lifeTimeRemain <= 0)
                    {
                        bulletJobUpdateInfo.lifeTimeRemain = 0;
                        bulletJobUpdateInfo.islifeended = true;
                    }
                    else
                    {
                        bulletJobUpdateInfo.islifeended = false;
                    }
                    bulletJobUpdateInfo.moveDirection = moveDirection;
                }


                //ֱ�ߵ�Ŀ��λ�õ�Job�㷨 StraightToPos
                if (job_jobInfo[i].movementType == 4)
                {
                    targetDirection = math.normalize(job_jobInfo[i].initialTargetPos - job_jobInfo[i].update_selfPos);

                    currentSpeed = job_jobInfo[i].update_currentSpeed + job_jobInfo[i].acceleration;
                    currentSpeed = math.clamp(currentSpeed, 0, job_jobInfo[i].maxSpeed);

                    moveDirection = targetDirection;

                    deltaMovement = job_jobInfo[i].update_selfPos + (moveDirection * currentSpeed * job_deltatime);

                    //����BulletJob��ֵ
                    bulletJobUpdateInfo.deltaMovement = deltaMovement;
                    bulletJobUpdateInfo.currentSpeed = currentSpeed;
                    bulletJobUpdateInfo.lifeTimeRemain = job_jobInfo[i].update_lifeTimeRemain - job_deltatime;


                    if (bulletJobUpdateInfo.lifeTimeRemain <= 0 || math.distance(deltaMovement, job_jobInfo[i].initialTargetPos) <= 0.1f)
                    {
                        bulletJobUpdateInfo.lifeTimeRemain = 0;
                        bulletJobUpdateInfo.islifeended = true;
                    }
                    else
                    {
                        bulletJobUpdateInfo.islifeended = false;
                    }


                    bulletJobUpdateInfo.moveDirection = moveDirection;
                }

                targetangle = math.degrees(math.atan2(moveDirection.y, moveDirection.x)) - 90;
                //����moveDirection����rotation
                bulletJobUpdateInfo.rotation = targetangle;
                rv_bulletJobUpdateInfos[i] = bulletJobUpdateInfo;
            }
        }
    }


    public override void UpdateBullet()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!_isUpdate) { return; }
        base.UpdateBullet();
    }
    public void Move(Vector3 movement)
    {
        rb.MovePosition(movement);
    }


    public override void PoolableReset()
    {
        base.PoolableReset();
        passThroughCount = maxPassThroughCount;
        damagedTargetList.Clear();
        prepareDamageTargetList.Clear();
    }

    public override void PoolableSetActive(bool isactive = true)
    {
        base.PoolableSetActive(isactive);
    }

    public override void PoolableDestroy()
    {
        base.PoolableDestroy();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTriggerPattern == DamageTriggerPattern.Collider)
        {
            if (collision.tag == this.tag && !_isApplyDamageAtThisFrame)
            {
                return;
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                prepareDamageTargetList.Add(collision.gameObject.GetComponent<IDamageble>());
                _isApplyDamageAtThisFrame = true;

            }
            return;
        }
        if (damageTriggerPattern == DamageTriggerPattern.Point) { return; }
        if(damageTriggerPattern == DamageTriggerPattern.Target)
        {
            if (collision.tag == this.tag && !_isApplyDamageAtThisFrame)
            {
                return;
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                if(initialTarget.Equals(collision.gameObject))
                {
                    prepareDamageTargetList.Add(collision.gameObject.GetComponent<IDamageble>());
                    _isApplyDamageAtThisFrame = true;
                }
            }
            return;
        }
        if(damageTriggerPattern == DamageTriggerPattern.PassTrough)
        {
            if (collision.tag == this.tag )
            {
                return;
            }
            if(damagedTargetList.Contains(collision.GetComponent<IDamageble>()))
            {
                return;
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                prepareDamageTargetList.Add(collision.gameObject.GetComponent<IDamageble>());
                _isApplyDamageAtThisFrame = true;

            }
            return;
        }
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
    }

    public override void ApplyDamageAllTarget()
    {
        base.ApplyDamageAllTarget();
    }

    public override void ApplyDamage(IDamageble damageble)
    {
        base.ApplyDamage(damageble);
        if(damageTriggerPattern == DamageTriggerPattern.PassTrough)
        {
            passThroughCount--;
        }
    }
    public bool TakeDamage(DamageResultInfo info)
    {
        if (HpComponent == null || !IsinterceptTarget)
            return false;

        bool isDie = HpComponent.ChangeHP(-info.Damage);
        if (isDie)
        {
            Death(null);
        }
        return isDie;
    }
}

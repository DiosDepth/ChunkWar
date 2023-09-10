using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ParticleSystemJobs;
using Unity.Collections;
using Unity.Mathematics;

public enum ProjectileMovementType
{
    Straight = 1,
    StraightToPos = 2,
    FollowTarget = 3,
}
public struct BulletJobInitialInfo
{
    
    public float3 update_selfPos;
    public float lifeTime;
    public float update_liftTimeRemain;
    public float maxSpeed;
    public float initialSpeed;
    public float update_currentSpeed;
    public float acceleration;
    public float rotSpeed;
    public float3 initialDirection;
    public float3 update_moveDirection;
    public int movementType;

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
    public BulletJobInitialInfo(float3 m_update_selfPos,float m_lifttime, float m_maxspeed, float m_initialspeed , float m_acceleration, float m_rotspeed, float3 m_initialdirection, int m_movementType)
    {
        update_selfPos = m_update_selfPos;
        lifeTime = m_lifttime;
        update_liftTimeRemain = lifeTime;
        maxSpeed = m_maxspeed;
        initialSpeed = m_initialspeed;
        update_currentSpeed = initialSpeed;
        acceleration = m_acceleration;
        rotSpeed = m_rotspeed;
        initialDirection = m_initialdirection;
        update_moveDirection = initialDirection;
        movementType = m_movementType;
    }
}

/// <summary>
/// ��Ҫÿһ֡���µĲ����� �ӵ��ƶ�������һ��������������ݣ� ͬʱҲ����µ�BulletJobInitialInfo��
/// </summary>
public struct BulletJobUpdateInfo
{
    public float selfPos;
    public float lifeTimeRemain;
    public float3 moveDirection;
    public float currentSpeed;
    public float3 deltaMovement;
    public bool islifeended;
}

public class Projectile : Bullet, IDamageble
{

    public Rigidbody2D rb;
    public Collider2D bulletCollider;
    public ProjectileMovementType movementType = ProjectileMovementType.Straight;
    public float lifeTime = 10;
    public float maxSpeed = 2.5f;
    public float rotSpeed = 180f;
    public float initialSpeed = 20f;
    public float acceleration = 0.25f;
    public bool IsinterceptTarget = true;

    /// <summary>
    /// Ѫ���������
    /// </summary>
    public GeneralHPComponet HpComponent;

    //private Coroutine startmovingCoroutine;
    protected float _beforemovetimestamp;


    // Start is called before the first frame update
    protected override void Start()
    {
       
    }

    // Update is called once per frame
    protected override void Update()
    {
        //while (Time.time < _movetimestamp)
        //{
        //    _tempmovement = new NativeArray<float3>(1, Allocator.TempJob);

        //    switch (movementType)
        //    {
        //        case ProjectileMovementType.Straight:

        //            //StaightJob staightJob = new StaightJob
        //            //{
        //            //    job_selfPos = transform.position,
        //            //    job_deltatime = Time.deltaTime,
        //            //    job_maxSpeed = maxSpeed,
        //            //    job_moveDirection = _movedirection.ToVector3(),

        //            //    JRD_movement = _tempmovement,
        //            //};

        //            break;
        //        case ProjectileMovementType.StraightToPos:

        //            break;
        //        case ProjectileMovementType.FollowTarget:

        //            break;
        //    }
        //}
    }

    public override void Shoot()
    {
        base.Shoot();

        PoolableSetActive();


    }

    public override void Initialization()
    {
        base.Initialization();
        bulletCollider = transform.GetComponentInChildren<Collider2D>();
        HpComponent = new GeneralHPComponet(100, 100);
     
    }


    public struct StaightCalculateBulletMovementJobJob : IJobParallelForBatch
    {
       
        [ReadOnly] public NativeArray<BulletJobInitialInfo> job_jobInfo;
        [ReadOnly] public float job_deltatime;


        public NativeArray<BulletJobUpdateInfo> rv_bulletJobUpdateInfos;

        private BulletJobUpdateInfo bulletJobUpdateInfo;
        private float currentSpeed;
        private float3 deltaMovement;
        public void Execute(int startIndex, int count)
        {

            for (int i = startIndex; i < startIndex + count; i++)
            {
                //ֱ���˶���Job�㷨
                if(job_jobInfo[i].movementType == 1)
                {
                    currentSpeed = job_jobInfo[i].update_currentSpeed + job_jobInfo[i].acceleration;
                    currentSpeed = math.clamp(currentSpeed, 0, job_jobInfo[i].maxSpeed);

                    deltaMovement = job_jobInfo[i].update_selfPos + (job_jobInfo[i].update_moveDirection * currentSpeed * job_deltatime);

                    //����BulletJob��ֵ
                    bulletJobUpdateInfo.deltaMovement = deltaMovement;
                    bulletJobUpdateInfo.currentSpeed = currentSpeed;
                    bulletJobUpdateInfo.lifeTimeRemain = job_jobInfo[i].update_liftTimeRemain - job_deltatime;
                    if(bulletJobUpdateInfo.lifeTimeRemain <= 0)
                    {
                        bulletJobUpdateInfo.islifeended = true;
                    }
                    else
                    {
                        bulletJobUpdateInfo.islifeended = false;
                    }
                    bulletJobUpdateInfo.moveDirection = job_jobInfo[i].update_moveDirection;


                    rv_bulletJobUpdateInfos[i] = bulletJobUpdateInfo;
                }

            }
        }
    }



    public void Move(Vector3 movement)
    {
        rb.MovePosition(movement);
    }


    //public IEnumerator FollowTarget(UnityAction callback)
    //{
    //    _beforemovetimestamp = Time.time + 0.5f;
    //    float currentspeed = initialSpeed;


    //    while (Time.time < _beforemovetimestamp)
    //    {
    //        currentspeed += acceleration;
    //        _tempmovement = transform.position.ToVector2() + (_movedirection * currentspeed * Time.deltaTime);

    //        //transform.rotation = MathExtensionTools.CalculateRotation(this.transform.up, _movedirection, rotSpeed);
    //        rb.MovePosition(_tempmovement);
    //        yield return null;
    //    }
    //    _movetimestamp = Time.time + lifeTime;


    //    while (Time.time < _movetimestamp)
    //    {
    //        if (target != null)
    //        {
    //            _movedirection = transform.position.DirectionToXY(target.transform.position);
    //        }


    //        currentspeed += acceleration;
    //        transform.rotation = MathExtensionTools.CalculateRotation(this.transform.up, _movedirection, rotSpeed);

    //        _tempmovement = transform.position.ToVector2() + (transform.up.ToVector2() * currentspeed * Time.deltaTime);

    //        rb.MovePosition(_tempmovement);

    //        yield return null;
    //    }
    //    callback?.Invoke();
    //}



    public override void PoolableReset()
    {
        base.PoolableReset();
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
        if(collision.tag == this.tag)
        {
            return;
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            if (_owner is Weapon)
            {
                var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                collision.GetComponent<IDamageble>()?.TakeDamage(ref damage);
            }
            Death();
        }
    }

    public override void Death()
    {
        base.Death(); 
    }


    public bool TakeDamage(ref DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;

        bool isDie = HpComponent.ChangeHP(-info.Damage);
        if (isDie)
        {
            Death();
        }
        return isDie;
    }
}

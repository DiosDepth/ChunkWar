using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;


public class AIManager : Singleton<AIManager>
{
    public const int MaxAICount = 300;
    public bool ProcessAI = false;
    public int ShipCount { get { return aiShipList.Count; } }
    public int BulletCount { get { return aiProjectileList.Count; } }


    //玩家Ship作为目标存储起来，需要按照Unit是否激活来更新targetActiveUnitList，同时更新targetActiveUnitPos
    public NativeList<float3> avoidanceCollisionPos;
    public NativeList<float> avoidanceCollisionRadius;
    public NativeList<float3> avoidanceCollisionVel;

    public IBoid playerBoid;
    public List<Unit> playerActiveUnitList = new List<Unit>();
    public NativeList<float3> playerActiveUnitPos;


    //AI list Info
    public List<AIShip> aiShipList = new List<AIShip>();
    public List<Projectile> aiProjectileList = new List<Projectile>();
    private List<Projectile> aiProjectileDamageList = new List<Projectile>();
    private List<int> _aiProjectileDeathIndex = new List<int>();
    public NativeList<ProjectileJobInitialInfo> aiProjectile_JobInfo;
    public NativeArray<ProjectileJobRetrunInfo> rv_aiProjectile_jobUpdateInfo;
    public NativeArray<int> rv_aiProjectileDamageTargetIndex;
    public NativeArray<int> rv_aiProjectileDamageTargetCountPre;


    public List<AISteeringBehaviorController> aiSteeringBehaviorControllerList = new List<AISteeringBehaviorController>();
    public List<IBoid> aiShipBoidList = new List<IBoid>();

    //需要吧每个AI的Behavior数据单独存下来， 跟随ShipCount
    //public List<ArriveBehavior> arriveBehavior = new List<ArriveBehavior>();
    //public List<FaceBehavior> faceBehavior = new List<FaceBehavior>();
    //public List<CohesionBehavior> cohesionBehavior = new List<CohesionBehavior>();
    //public List<SeparationBehavior> separationBehavior = new List<SeparationBehavior>();
    //public List<AlignmentBehavior> aligmentBehavior = new List<AlignmentBehavior>();




    public List<Unit> aiActiveUnitList = new List<Unit>();
    //需要根据aiship Unit生死来更新的Jobdata
    public NativeList<float3> aiActiveUnitPos;
    public NativeList<float> aiActiveUnitAttackRange;
    public NativeList<int> aiActiveUnitMaxTargetsCount;


  


    //全局的Jobdata， 数量跟随ShipCount ------------------------------------------------
    public NativeList<float3> steeringBehaviorJob_aiShipPos;
    public NativeList<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeList<float> steeringBehaviorJob_aiShipRotationZ;
    public NativeList<float> steeringBehaviorJob_aiShipRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAngularAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipTargetSerchingRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipDrag;



    public NativeArray<SteeringBehaviorInfo> rv_evade_steeringInfo;
    public NativeList<float> evade_maxPrediction;
    public NativeList<float> evade_weight;


    //arrivebehavior return data
    public NativeArray<SteeringBehaviorInfo> rv_arrive_steeringInfo;
    public NativeArray<bool> rv_arrive_isVelZero;
    //arrivebehavior job input data
    public NativeList<float> arrive_arriveRadius;
    public NativeList<float> arrive_slowRadius;
    public NativeList<float> arrive_weight;

    //facebehavior return data
    public NativeArray<SteeringBehaviorInfo> rv_face_steeringInfo;
    //facebehavior job input data
    public NativeList<float> face_facetargetRadius;
    public NativeList<float> face_weight;



    public NativeArray<SteeringBehaviorInfo> rv_cohesion_steeringInfo;
    public NativeList<float> cohesion_viewAngle;
    public NativeList<float> cohesion_weight;

    public NativeArray<SteeringBehaviorInfo> rv_separation_steeringInfo;
    public NativeList<float> separation_threshold;
    public NativeList<float> separation_decayCoefficient;
    public NativeList<float> separation_weight;

    public NativeArray<SteeringBehaviorInfo> rv_alignment_steeringInfo;
    public NativeList<float> alignment_weight;
    public NativeList<float> alignment_alignDistance;

    public NativeArray<SteeringBehaviorInfo> rv_collisionavoidance_steeringInfo;
    public NativeList<float> collisionavoidance_weight;


    public NativeArray<SteeringBehaviorInfo> rv_deltaMovement;

    public NativeArray<float3> rv_serchingTargetsPosPerShip;
    public NativeArray<float3> rv_serchingTargetsVelPerShip;
    public NativeArray<int> rv_serchingTargetsCountPerShip;




    //weapon Update job result data
    NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;

    // Start is called before the first frame update
    public AIManager()
    {

        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddLaterUpdateListener(LaterUpdate);
        MonoManager.Instance.AddFixedUpdateListener(FixedUpdate);
    }
    ~AIManager()
    {

        MonoManager.Instance.RemoveUpdateListener(Update);
        MonoManager.Instance.RemoveUpdateListener(LaterUpdate);
        MonoManager.Instance.RemoveFixedUpdateListener(FixedUpdate);
    }

    public override void Initialization()
    {
        base.Initialization();
        AllocateAIJobData();

        playerBoid = RogueManager.Instance.currentShip.GetComponent<IBoid>();
        playerActiveUnitList.AddRange(RogueManager.Instance.currentShip.UnitList);

        avoidanceCollisionPos.Add(playerBoid.GetPosition());
        avoidanceCollisionRadius.Add(playerBoid.GetRadius());
        avoidanceCollisionVel.Add(playerBoid.GetVelocity());



        ProcessAI = true;
        // allocate all job data 
    

    }

    public virtual void AllocateAIJobData()
    {
        playerActiveUnitPos = new NativeList<float3>(Allocator.Persistent);
        avoidanceCollisionPos = new NativeList<float3>(Allocator.Persistent);
        avoidanceCollisionRadius = new NativeList<float>(Allocator.Persistent);
        avoidanceCollisionVel = new NativeList<float3>(Allocator.Persistent);

        aiActiveUnitPos = new NativeList<float3>(Allocator.Persistent);

        aiActiveUnitAttackRange = new NativeList<float>(Allocator.Persistent);
        aiActiveUnitMaxTargetsCount = new NativeList<int>(Allocator.Persistent);


        aiProjectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);

        steeringBehaviorJob_aiShipPos = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipVelocity = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRotationZ = new NativeList<float>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRadius = new NativeList<float>(Allocator.Persistent);

        aiSteeringBehaviorController_aiShipMaxAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipDrag = new NativeList<float>(Allocator.Persistent);


        evade_maxPrediction = new NativeList<float>(Allocator.Persistent);
        evade_weight = new NativeList<float>(Allocator.Persistent);
        //arrivebehavior return data
        //arrivebehavior job input data
        arrive_arriveRadius = new NativeList<float>(Allocator.Persistent);
        arrive_slowRadius = new NativeList<float>(Allocator.Persistent);
        arrive_weight = new NativeList<float>(Allocator.Persistent);
        //facebehavior return data
    
        //facebehavior job input data
        face_weight = new NativeList<float>(Allocator.Persistent);
        face_facetargetRadius = new NativeList<float>(Allocator.Persistent);


        cohesion_weight = new NativeList<float>(Allocator.Persistent);
        cohesion_viewAngle = new NativeList<float>(Allocator.Persistent);

        separation_weight = new NativeList<float>(Allocator.Persistent);
        separation_threshold = new NativeList<float>(Allocator.Persistent);
        separation_decayCoefficient = new NativeList<float>(Allocator.Persistent);

        alignment_weight = new NativeList<float>(Allocator.Persistent);
        alignment_alignDistance = new NativeList<float>(Allocator.Persistent);

        collisionavoidance_weight = new NativeList<float>(Allocator.Persistent);

    }
    public virtual void UpdateMoveJobData()
    {

        // ship Job data
        if (aiShipBoidList != null && aiShipBoidList.Count != 0)
        {
            for (int i = 0; i < ShipCount; i++)
            {
                steeringBehaviorJob_aiShipPos[i] = aiShipBoidList[i].GetPosition();
                steeringBehaviorJob_aiShipVelocity[i] = aiShipBoidList[i].GetVelocity();
                steeringBehaviorJob_aiShipRadius[i] = aiShipBoidList[i].GetRadius();
                steeringBehaviorJob_aiShipRotationZ[i] = aiShipBoidList[i].GetRotationZ();
            }
        }

        if(playerBoid != null)
        {
            avoidanceCollisionPos[0] = playerBoid.GetPosition();
            avoidanceCollisionRadius[0] = playerBoid.GetRadius();
            avoidanceCollisionVel[0] = playerBoid.GetVelocity();
        }
        //weapon job data
        if(playerActiveUnitList.Count >0)
        {
            for (int i = 0; i < playerActiveUnitList.Count; i++)
            {
                playerActiveUnitPos[i] = playerActiveUnitList[i].transform.position;
            }
        }

        if (aiActiveUnitList.Count > 0)
        {
            for (int i = 0; i < aiActiveUnitList.Count; i++)
            {
                aiActiveUnitPos[i] = aiActiveUnitList[i].transform.position;
            }
        }



    }
    public virtual void DisposeAIJobData()
    {
        if (playerActiveUnitPos.IsCreated) { playerActiveUnitPos.Dispose(); }
        if (avoidanceCollisionPos.IsCreated) { avoidanceCollisionPos.Dispose(); }
        if (avoidanceCollisionRadius.IsCreated) { avoidanceCollisionRadius.Dispose(); }
        if (avoidanceCollisionVel.IsCreated) { avoidanceCollisionVel.Dispose(); }

        if (aiActiveUnitPos.IsCreated) { aiActiveUnitPos.Dispose(); }
        if(aiActiveUnitAttackRange.IsCreated) { aiActiveUnitAttackRange.Dispose(); }
        if (aiActiveUnitMaxTargetsCount.IsCreated) { aiActiveUnitMaxTargetsCount.Dispose(); }
        if (aiProjectile_JobInfo.IsCreated) { aiProjectile_JobInfo.Dispose(); }

        //ai data dispose
        if (steeringBehaviorJob_aiShipPos.IsCreated) { steeringBehaviorJob_aiShipPos.Dispose(); }
        if (steeringBehaviorJob_aiShipVelocity.IsCreated) { steeringBehaviorJob_aiShipVelocity.Dispose(); }
        if (steeringBehaviorJob_aiShipRadius.IsCreated) { steeringBehaviorJob_aiShipRadius.Dispose(); }
        if (steeringBehaviorJob_aiShipRotationZ.IsCreated) { steeringBehaviorJob_aiShipRotationZ.Dispose(); }

        if (aiSteeringBehaviorController_aiShipMaxAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipMaxAngularAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipTargetSerchingRadius.IsCreated) { aiSteeringBehaviorController_aiShipTargetSerchingRadius.Dispose(); }
        if (aiSteeringBehaviorController_aiShipDrag.IsCreated) { aiSteeringBehaviorController_aiShipDrag.Dispose(); }


        if (evade_maxPrediction.IsCreated) { evade_maxPrediction.Dispose(); }
        if (evade_weight.IsCreated) { evade_weight.Dispose(); }
        //dispose arrive job data

        if (arrive_arriveRadius.IsCreated) { arrive_arriveRadius.Dispose(); }
        if (arrive_slowRadius.IsCreated) { arrive_slowRadius.Dispose(); }
        if (arrive_weight.IsCreated) { arrive_weight.Dispose(); }


        //dispose face job data

        if (face_weight.IsCreated) { face_weight.Dispose(); }
        if (face_facetargetRadius.IsCreated) { face_facetargetRadius.Dispose(); }

        if (separation_weight.IsCreated) { separation_weight.Dispose(); }
        if (separation_threshold.IsCreated) { separation_threshold.Dispose(); }
        if (separation_decayCoefficient.IsCreated) { separation_decayCoefficient.Dispose(); }

        if (cohesion_weight.IsCreated) { cohesion_weight.Dispose(); }
        if (cohesion_viewAngle.IsCreated) { cohesion_viewAngle.Dispose(); }

        if (alignment_weight.IsCreated) { alignment_weight.Dispose(); }
        if (alignment_alignDistance.IsCreated) { alignment_alignDistance.Dispose(); }

        if (collisionavoidance_weight.IsCreated) { collisionavoidance_weight.Dispose(); }
        //weapon job data dispose
    }


    public void Unload()
    {
        //clear all ai
        if ( aiShipList != null)
        {
            for (int i = 0; i < aiShipList.Count; i++)
            {
                aiShipList[i].PoolableDestroy();
            }
            if (aiShipList.Count > 0)
            {
               ClearAI();
            }
        }
        if(playerActiveUnitList != null)
        {
            if(playerActiveUnitList.Count >0)
            {
                ClearTarget();
            }
        }

        playerBoid = null;

        //clear all player
        //todo clear all ai weapon
        aiActiveUnitList.Clear();
       
        //todo clear all bullet
        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            aiProjectileList[i].Death();
            
        }
        aiProjectileList.Clear();


        //Dispose all job data
        DisposeAIJobData();
    }
    public void GameOver()
    {
        ProcessAI = false;
        Stop();
        Unload();
    }
    public void Stop()
    {
        ProcessAI = false;
        for (int i = 0; i < aiShipList.Count; i++)
        {
            for (int n = 0; n < aiShipList[i].UnitList.Count; n++)
            {
               aiShipList[i].UnitList[n].SetUnitProcess(false);
            }
            aiShipList[i].controller.SetControllerUpdate(false);
        }
    }
    private void Update()
    {
        if (!ProcessAI) { return; }
        //update weapon
        UpdateAIWeapon();
        UpdateProjectile();
        ProcessProjectileDamage();






    }

    private void LaterUpdate()
    {
        if (!ProcessAI) { return; }
    }

    private void FixedUpdate()
    {
        if (!ProcessAI) { return; }
        UpdateMoveJobData();
        UpdateAIMovement();
    }


    public void AddAI(AIShip ship)
    {
        if(aiShipList.Contains(ship))
        {
            return;
        }
        aiShipList.Add(ship);
        IBoid boid = ship.GetComponent<IBoid>();
        aiShipBoidList.Add(boid);

        AddUnit(ship);
        AISteeringBehaviorController controller = ship.GetComponent<AISteeringBehaviorController>();
        aiSteeringBehaviorControllerList.Add(controller);

        //更新Job信息

        steeringBehaviorJob_aiShipPos.Add(boid.GetPosition());
        steeringBehaviorJob_aiShipRadius.Add(boid.GetRadius());
        steeringBehaviorJob_aiShipVelocity.Add(boid.GetVelocity());
        steeringBehaviorJob_aiShipRotationZ.Add(boid.GetRotationZ());

        aiSteeringBehaviorController_aiShipMaxAcceleration.Add(controller.maxAcceleration);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Add(controller.maxAngularAcceleration);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius.Add(controller.targetSerchingRadius);
        aiSteeringBehaviorController_aiShipDrag.Add(controller.drag);

        evade_maxPrediction.Add(controller.evadeBehaviorInfo.maxPrediction);
        evade_weight.Add(controller.evadeBehaviorInfo.GetWeight());

        arrive_arriveRadius.Add(controller.arrivelBehaviorInfo.arriveRadius);
        arrive_slowRadius.Add(controller.arrivelBehaviorInfo.slowRadius);
        arrive_weight.Add(controller.arrivelBehaviorInfo.GetWeight());


        face_weight.Add(controller.faceBehaviorInfo.GetWeight());
        face_facetargetRadius.Add(controller.faceBehaviorInfo.facetargetRadius);

        cohesion_weight.Add(controller.cohesionBehaviorInfo.GetWeight());
        cohesion_viewAngle.Add(controller.cohesionBehaviorInfo.viewAngle);

        separation_weight.Add(controller.separationBehaviorInfo.GetWeight());
        separation_threshold.Add(controller.separationBehaviorInfo.threshold);
        separation_decayCoefficient.Add(controller.separationBehaviorInfo.decayCoefficient);

        alignment_weight.Add(controller.alignmentBehaviorInfo.GetWeight());
        alignment_alignDistance.Add(controller.alignmentBehaviorInfo.alignDistance);

        collisionavoidance_weight.Add(controller.collisionAvoidanceBehaviorInfo.GetWeight());
    }

    public void AddAIRange(List<AIShip> shiplist)
    {
        for (int i = 0; i < shiplist.Count; i++)
        {
            AddAI(shiplist[i]);
        }
    }

    public void RemoveAI(AIShip ship)
    {
        int index = aiShipList.IndexOf(ship);
        RemoveReminedUnit(ship);
        RemoveAI(index);
    }
    protected void RemoveAI(int index)
    {
        aiShipList.RemoveAt(index);
        aiShipBoidList.RemoveAt(index);
        aiSteeringBehaviorControllerList.RemoveAt(index);


        steeringBehaviorJob_aiShipPos.RemoveAt(index);
        steeringBehaviorJob_aiShipRadius.RemoveAt(index);
        steeringBehaviorJob_aiShipVelocity.RemoveAt(index);
        steeringBehaviorJob_aiShipRotationZ.RemoveAt(index);

        aiSteeringBehaviorController_aiShipMaxAcceleration.RemoveAt(index);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration.RemoveAt(index);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius.RemoveAt(index);
        aiSteeringBehaviorController_aiShipDrag.RemoveAt(index);

        evade_maxPrediction.RemoveAt(index);
        evade_weight.RemoveAt(index);

        arrive_arriveRadius.RemoveAt(index);
        arrive_slowRadius.RemoveAt(index);
        arrive_weight.RemoveAt(index);


        face_weight.RemoveAt(index);
        face_facetargetRadius.RemoveAt(index);

        cohesion_weight.RemoveAt(index);
        cohesion_viewAngle.RemoveAt(index);

        separation_weight.RemoveAt(index);
        separation_threshold.RemoveAt(index);
        separation_decayCoefficient.RemoveAt(index);

        alignment_weight.RemoveAt(index);
        alignment_alignDistance.RemoveAt(index);

        collisionavoidance_weight.RemoveAt(index);
    }


    public void ClearAI()
    {
        aiShipList.Clear();
        aiShipBoidList.Clear();
        aiActiveUnitList.Clear();
        aiSteeringBehaviorControllerList.Clear();
    }
    public void ClearTarget()
    {

        playerActiveUnitList.Clear();

    }
    public void  AddUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
                AddSingleUnit(ship.UnitList[i]);
        }
    }
    public void AddSingleUnit(Unit unit)
    {
        if(!aiActiveUnitList.Contains(unit) && unit.isActiveAndEnabled)
        {
            aiActiveUnitList.Add(unit);
            aiActiveUnitPos.Add(unit.transform.position);
            aiActiveUnitAttackRange.Add(unit.baseAttribute.WeaponRange);
            aiActiveUnitMaxTargetsCount.Add(unit.maxTargetCount);
        }
    }
    public void RemoveReminedUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            if(ship.UnitList[i].isActiveAndEnabled)
            {
                RemoveSingleUnit(ship.UnitList[i]);
            }
        }
    }

    public void RemoveSingleUnit(Unit unit)
    {
        int index = aiActiveUnitList.IndexOf(unit);
        aiActiveUnitList.RemoveAt(index);
        aiActiveUnitPos.RemoveAt(index);
        aiActiveUnitAttackRange.RemoveAt(index);
        aiActiveUnitMaxTargetsCount.RemoveAt(index);
    }

    public void AddBullet(Bullet bullet )
    {
        if(bullet is Projectile)
        {
            AddProjectileBullet(bullet as Projectile);
        }
    }
    public void AddProjectileBullet(Projectile bullet)
    {
        if(!aiProjectileList.Contains(bullet))
        {
            aiProjectileList.Add(bullet);
            aiProjectile_JobInfo.Add(new ProjectileJobInitialInfo
                (
                    bullet.target? bullet.target.transform.position : bullet.transform.up,
                    bullet.transform.position,
                    bullet.lifeTime,
                    bullet.maxSpeed,
                    bullet.initialSpeed,
                    bullet.acceleration,
                    bullet.rotSpeed,
                    bullet.InitialmoveDirection.ToVector3(),
                   (int)bullet.movementType,
                   (int)bullet.damageType,
                   bullet.damageRadius
                ));
        }

    }

    public void RemoveProjectileBullet(Projectile bullet)
    {
        int index = aiProjectileList.IndexOf(bullet);
       aiProjectile_JobInfo.RemoveAt(index);
       aiProjectileList.RemoveAt(index);
    }
    public void RemoveBullet(Bullet bullet)
    {
        if(bullet is Projectile)
        {
            RemoveProjectileBullet(bullet as Projectile);
        }
        //todo 如果不是Projectile类型需要用调用其他容器的Remove
    }

    public void AddTargetUnit(Unit unit)
    {
        if (!playerActiveUnitList.Contains(unit))
        {
            playerActiveUnitList.Add(unit);
            playerActiveUnitPos.Add(unit.transform.position);
            
        }
         

    }

    public void RemoveTargetUnit(Unit unit)
    {
        int index  = playerActiveUnitList.IndexOf(unit);
        playerActiveUnitList.RemoveAt(index);
        playerActiveUnitPos.RemoveAt(index);
    }
    public void UpdateAIMovement()
    {

        if (aiShipList == null || aiShipList.Count == 0)
        {
            return;
        }

        JobHandle jobhandle_evadebehavior;
        JobHandle jobhandle_arrivebehavior;
        JobHandle jobhandle_facebehavior;
        JobHandle jobhandle_behaviorTargets;
        JobHandle jobhandle_cohesionbehavior;
        JobHandle jobhandle_separationBehavior;
        JobHandle jobhandle_alignmentBehavior;
        JobHandle jobhandle_collisionAvoidanceBehavior;
        //jobhandleList_AI = new NativeList<JobHandle>(Allocator.TempJob);

        rv_evade_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        rv_arrive_isVelZero = new NativeArray<bool>(ShipCount, Allocator.TempJob);
        rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_collisionavoidance_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);



        EvadeBehavior.EvadeBehaviorJob evadeBehaviorJob = new EvadeBehavior.EvadeBehaviorJob
        {
            job_aiShipPos= steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_evadeTargetPos = playerBoid.GetPosition(),
            job_evadeTargetVel = playerBoid.GetVelocity(),
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_maxPrediction = evade_maxPrediction,

            rv_steering = rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_evadebehavior.Complete();


      

        ArriveBehavior.ArriveBehaviorJobs arriveBehaviorJobs = new ArriveBehavior.ArriveBehaviorJobs
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRadius = steeringBehaviorJob_aiShipRadius,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_arriveRadius = arrive_arriveRadius,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_slowRadius = arrive_slowRadius,
            job_targetPos = playerBoid.GetPosition(),
            job_targetRadius = playerBoid.GetRadius(),

            rv_isVelZero = rv_arrive_isVelZero,
            rv_Steerings = rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs.ScheduleBatch(ShipCount, 2);
        jobhandle_arrivebehavior.Complete();

        ////FaceBehavior Job
        FaceBehavior.FaceBehaviorJob faceBehaviorJob = new FaceBehavior.FaceBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRotationZ = steeringBehaviorJob_aiShipRotationZ,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_facetagetRadius = face_facetargetRadius,
            job_maxAngularAcceleration = aiSteeringBehaviorController_aiShipMaxAngularAcceleration,
            job_targetPos = playerBoid.GetPosition(),
            job_deltatime = Time.fixedDeltaTime,

            rv_Steerings = rv_face_steeringInfo,
        };
        jobhandle_facebehavior = faceBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_facebehavior.Complete();

        //NativeList<JobHandle> behaviorTargetsList = new NativeList<JobHandle>();

        rv_serchingTargetsVelPerShip = new NativeArray<float3>(ShipCount * ShipCount, Allocator.TempJob);
        rv_serchingTargetsPosPerShip = new NativeArray<float3>(ShipCount * ShipCount, Allocator.TempJob);
        rv_serchingTargetsCountPerShip = new NativeArray<int>(ShipCount, Allocator.TempJob);


        AISteeringBehaviorController.CalculateTargetsPosByRadiusJob calculateTargetsPosByRadiusJob = new AISteeringBehaviorController.CalculateTargetsPosByRadiusJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_SerchingRadius = aiSteeringBehaviorController_aiShipTargetSerchingRadius,
            job_threshold = 0.01f,
            

            rv_findedTargetCountPreShip = rv_serchingTargetsCountPerShip,
            rv_findedTargetVelPreShip = rv_serchingTargetsVelPerShip,
            rv_findedTargetsPosPreShip = rv_serchingTargetsPosPerShip,
        };

        jobhandle_behaviorTargets = calculateTargetsPosByRadiusJob.ScheduleBatch(ShipCount, 2);

        jobhandle_behaviorTargets.Complete();


        //Cohesion Job

        CohesionBehavior.CohesionBehaviorJob cohesionBehaviorJob = new CohesionBehavior.CohesionBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_viewAngle = cohesion_viewAngle,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_InRangeLength = rv_serchingTargetsCountPerShip,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_shipcount = ShipCount,

            rv_Steerings = rv_cohesion_steeringInfo,
        };

        jobhandle_cohesionbehavior = cohesionBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_cohesionbehavior.Complete();

        //separation Job
        SeparationBehavior.SeparationBehaviorJob separationBehaviorJob = new SeparationBehavior.SeparationBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_decayCoefficient = separation_decayCoefficient,
            job_threshold = separation_threshold,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_InRangeLength = rv_serchingTargetsCountPerShip,
            job_shipcount = ShipCount,

            rv_Steerings = rv_separation_steeringInfo,
        };
        jobhandle_separationBehavior = separationBehaviorJob.ScheduleBatch(ShipCount, 2);

        jobhandle_separationBehavior.Complete();

        //Alignment Job
        AlignmentBehavior.AlignmentBehaviorJob alignmentBehaviorJob = new AlignmentBehavior.AlignmentBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_alignDistance = alignment_alignDistance,
            job_aiShipPosInRange = rv_serchingTargetsPosPerShip,
            job_aiShipVelInRange = rv_serchingTargetsVelPerShip,
            job_lengthInRange = rv_serchingTargetsCountPerShip,

            job_shipcount = ShipCount,

            rv_Steerings = rv_alignment_steeringInfo,
        };

        jobhandle_alignmentBehavior = alignmentBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_alignmentBehavior.Complete();


        
        /*
        CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRadius = steeringBehaviorJob_aiShipRadius,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,

            job_avoidenceTargetPos = avoidanceCollisionPos,
            job_avoidenceTargetRadius = avoidanceCollisionRadius,
            job_avoidenceTargetVel = avoidanceCollisionVel,

            rv_steering = rv_collisionavoidance_steeringInfo,

        };

        jobhandle_collisionAvoidanceBehavior = collisionAvoidanceBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_collisionAvoidanceBehavior.Complete();
        */



        rv_deltaMovement = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        JobHandle jobhandle_deltamoveposjob;
        AISteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new AISteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_aiShipMaxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            Job_aiShipDrag = aiSteeringBehaviorController_aiShipDrag,
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVelocity = steeringBehaviorJob_aiShipVelocity,

            job_evadeSteering = rv_evade_steeringInfo,
            job_evadeWeight = evade_weight,

            job_arriveSteering = rv_arrive_steeringInfo,
            job_arriveWeight = arrive_weight,
            job_isVelZero = rv_arrive_isVelZero,

            job_faceSteering = rv_face_steeringInfo,
            job_faceWeight = face_weight,

            job_alignmentSteering = rv_alignment_steeringInfo,
            job_alignmentWeight = alignment_weight,

            job_cohesionSteering = rv_cohesion_steeringInfo,
            job_cohesionWeight = cohesion_weight,

            job_separationSteering = rv_separation_steeringInfo,
            job_separationWeight = separation_weight,

            job_collisionAvoidanceSteering = rv_collisionavoidance_steeringInfo,
            job_collisionAvoidanceWeight = collisionavoidance_weight,

            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(ShipCount, 2);
        jobhandle_deltamoveposjob.Complete();
      

        for (int i = 0; i < ShipCount; i++)
        {
            aiSteeringBehaviorControllerList[i].UpdateIBoid();

            aiSteeringBehaviorControllerList[i].Move(rv_deltaMovement[i].linear);
            aiSteeringBehaviorControllerList[i].transform.rotation = Quaternion.Euler(0, 0,rv_deltaMovement[i].angular);
        }

        playerBoid.UpdateIBoid();

        rv_evade_steeringInfo.Dispose();

        rv_arrive_isVelZero.Dispose();
        rv_arrive_steeringInfo.Dispose();

        rv_face_steeringInfo.Dispose();
        rv_cohesion_steeringInfo.Dispose();
        rv_separation_steeringInfo.Dispose();
        rv_alignment_steeringInfo.Dispose();
        rv_collisionavoidance_steeringInfo.Dispose();

        rv_deltaMovement.Dispose();
        rv_serchingTargetsPosPerShip.Dispose();
        rv_serchingTargetsVelPerShip.Dispose();
        rv_serchingTargetsCountPerShip.Dispose();

    }

    public void UpdateAIWeapon()
    {

        if( aiActiveUnitList == null || aiActiveUnitList.Count == 0)
        {
            return;
        }
        AIWeapon weapon;

        int targetstotalcount = 0;
        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            targetstotalcount += aiActiveUnitList[i].maxTargetCount;
        }

        rv_weaponTargetsInfo = new NativeArray<Weapon.RV_WeaponTargetInfo>(targetstotalcount, Allocator.TempJob);

        // 如果Process 则开启索敌Job， 并且蒋索敌结果记录在targetsindex[]中
        //这个Job返回一个JRD_targetsInfo 这个是已经排序的数据， 包含 index， Pos， direction， distance 几部分数据


        Weapon.FindWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindWeaponTargetsJob
        {
            job_attackRange = aiActiveUnitAttackRange,
            job_selfPos = aiActiveUnitPos,
            job_targetsPos =playerActiveUnitPos,
            job_maxTargetCount = aiActiveUnitMaxTargetsCount,

            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(aiActiveUnitList.Count, 2);


        jobHandle.Complete();
        //执行Job 并且等待结果。



        int startindex;
        int targetindex;
        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            //获取Flat Array中的startindex 为后续的拆分做准备
            //吧前面每一个unit的 maxtargetscount全部加起来就是FlatArray中的第一个index
            startindex = 0;
            for (int c = 0; c < i; c++)
            {
                startindex += aiActiveUnitMaxTargetsCount[i];
            }

            if(aiActiveUnitList[i] is AIWeapon)
            {
                weapon = aiActiveUnitList[i] as AIWeapon;
                weapon.targetList.Clear();
                for (int n = 0; n < aiActiveUnitMaxTargetsCount[i]; n++)
                {
                    targetindex = rv_weaponTargetsInfo[startindex + n].targetIndex;
                    if (targetindex == -1)
                    {
                    
                        weapon.WeaponOff();
                        break;
                    }
                    else
                    {
                        if (playerActiveUnitList == null || playerActiveUnitList.Count == 0)
                        {
                         
                            weapon.WeaponOff();
                            break;
                        }
                   
                    
                        weapon.targetList.Add(new WeaponTargetInfo
                            (
                                playerActiveUnitList[targetindex].gameObject,
                                rv_weaponTargetsInfo[startindex + n].targetIndex,
                                rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                                rv_weaponTargetsInfo[startindex + n].targetDirection
                            ));
                    }
                }
                if(weapon.targetList !=null && weapon.targetList.Count != 0)
                {
                    weapon.WeaponOn();
                }
                weapon.ProcessWeapon();
            }
            
        }

        rv_weaponTargetsInfo.Dispose();
        //Dispose all Job tempdata
    }



    public void UpdateProjectile()
    {
        if(aiProjectileList == null || aiProjectileList.Count == 0)
        {
            return;
        }
        rv_aiProjectile_jobUpdateInfo = new NativeArray<ProjectileJobRetrunInfo>(aiProjectileList.Count, Allocator.TempJob);
        // 获取子弹分类
        // 获取子弹的Target，并且更新Target位置， 如果是
        JobHandle aiprojectilejobhandle;
        //根据子弹分类比如是否targetbase来开启对应的JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateProjectileMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = aiProjectile_JobInfo,
            rv_bulletJobUpdateInfos = rv_aiProjectile_jobUpdateInfo,
        };

        aiprojectilejobhandle = staightCalculateProjectileMovementJobJob.ScheduleBatch(aiProjectileList.Count, 2);
        aiprojectilejobhandle.Complete();
        //更新子弹当前的JobData

        //移动子弹
        //处理子弹旋转方向
        _aiProjectileDeathIndex.Clear();

        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            if (!rv_aiProjectile_jobUpdateInfo[i].islifeended)
            {
                aiProjectileList[i].Move(rv_aiProjectile_jobUpdateInfo[i].deltaMovement);
                aiProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_aiProjectile_jobUpdateInfo[i].rotation);
            }
            else
            {
                _aiProjectileDeathIndex.Add(i);
            }
        }





    


        UpdateProjectileJobData();

        rv_aiProjectile_jobUpdateInfo.Dispose();
        //dispose
    }

    public void ProcessProjectileDamage()
    {
        //处理所有子弹的伤害逻辑
        aiProjectileDamageList.Clear();
        aiProjectileDamageList = aiProjectileList.FindAll(x => x.IsApplyDamageAtThisFrame);

        if(aiProjectileDamageList.Count == 0)
        {
            return;
        }


        rv_aiProjectileDamageTargetIndex = new NativeArray<int>(aiProjectileDamageList.Count * playerActiveUnitList.Count, Allocator.TempJob);
        rv_aiProjectileDamageTargetCountPre = new NativeArray<int>(aiProjectileDamageList.Count, Allocator.TempJob);
        JobHandle jobHandle;
        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = aiProjectile_JobInfo,
            job_targesTotalCount = playerActiveUnitList.Count,
            job_targetsPos = playerActiveUnitPos,

            rv_findedTargetsCount = rv_aiProjectileDamageTargetCountPre,
            rv_findedTargetIndex = rv_aiProjectileDamageTargetIndex,

        };
        jobHandle = findBulletDamageTargetJob.ScheduleBatch(aiProjectileDamageList.Count, 2);
        jobHandle.Complete();


        int damagetargetindex;
        IDamageble damageble;

        for (int i = 0; i < aiProjectileDamageList.Count; i++)
        {
            for (int n = 0; n < rv_aiProjectileDamageTargetCountPre[i]; n++)
            {
                damagetargetindex = rv_aiProjectileDamageTargetIndex[i * playerActiveUnitList.Count + n];
                damageble = playerActiveUnitList[damagetargetindex].GetComponent<IDamageble>();
                aiProjectileDamageList[i].ApplyDamage(damageble);
            }
        }


        rv_aiProjectileDamageTargetCountPre.Dispose();
        rv_aiProjectileDamageTargetIndex.Dispose();
    }

    public void UpdateProjectileJobData()
    {
        //这里需要先更新子弹的信息，然后在吧死亡的子弹移除， 否则rv aibullet的静态数据长度无法操作
        //虽然会浪费运算量。但是可以保证index不会错位
        ProjectileJobInitialInfo bulletJobInfo;
        for (int i = 0; i < aiProjectileList.Count; i++)
        {
            if (rv_aiProjectile_jobUpdateInfo[i].islifeended)
                continue;
            //bulletJobInfo 是一个值类型。 只能使用这种方式更新。
            bulletJobInfo = aiProjectile_JobInfo[i];

            bulletJobInfo.update_targetPos = aiProjectileList[i].target? aiProjectileList[i].target.transform.position : aiProjectileList[i].transform.up;
            bulletJobInfo.update_selfPos = aiProjectileList[i].transform.position;
            bulletJobInfo.update_lifeTimeRemain = rv_aiProjectile_jobUpdateInfo[i].lifeTimeRemain;
            bulletJobInfo.update_moveDirection = rv_aiProjectile_jobUpdateInfo[i].moveDirection;
            bulletJobInfo.update_currentSpeed = rv_aiProjectile_jobUpdateInfo[i].currentSpeed;

            aiProjectile_JobInfo[i] = bulletJobInfo;
        }

        //最后一步 执行子弹死亡
        int deathindex = 0;
        for (int i = 0; i < _aiProjectileDeathIndex.Count; i++)
        {
            deathindex = _aiProjectileDeathIndex[i];
            aiProjectileList[deathindex].Death();

        }
    }


}

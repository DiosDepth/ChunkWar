using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class AIManager : Singleton<AIManager>
{
    public const int MaxAICount = 300;
    public int ShipCount { get { return aiShipList.Count; } }
    public int BulletCount { get { return aibulletsList.Count; } }



    //AI list Info
    public List<AIShip> aiShipList = new List<AIShip>();
    public List<Bullet> aibulletsList = new List<Bullet>();

 
    public List<AISteeringBehaviorController> aiSteeringBehaviorControllerList = new List<AISteeringBehaviorController>();
    public List<IBoid> aiShipBoidList = new List<IBoid>();

    //��Ҫ��ÿ��AI��Behavior���ݵ����������� ����ShipCount
    //public List<ArriveBehavior> arriveBehavior = new List<ArriveBehavior>();
    //public List<FaceBehavior> faceBehavior = new List<FaceBehavior>();
    //public List<CohesionBehavior> cohesionBehavior = new List<CohesionBehavior>();
    //public List<SeparationBehavior> separationBehavior = new List<SeparationBehavior>();
    //public List<AlignmentBehavior> aligmentBehavior = new List<AlignmentBehavior>();

    //���Ship��ΪĿ��洢��������Ҫ����Unit�Ƿ񼤻�������targetActiveUnitList��ͬʱ����targetActiveUnitPos
    public IBoid targetBoid;
    public List<Unit> targetActiveUnitList = new List<Unit>();
    public NativeList<float3> targetActiveUnitPos;



    public List<Unit> aiActiveUnitList = new List<Unit>();
    //��Ҫ����aiship Unit���������µ�Jobdata
    public NativeList<float3> aiActiveUnitPos;
    public NativeList<float> aiActiveUnitAttackRange;
    public NativeList<int> aiActiveUnitMaxTargetsCount;


  


    //ȫ�ֵ�Jobdata�� ��������ShipCount ------------------------------------------------
    public NativeList<float3> steeringBehaviorJob_aiShipPos;
    public NativeList<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeList<float> steeringBehaviorJob_aiShipRotationZ;
    public NativeList<float> steeringBehaviorJob_aiShipRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAngularAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipTargetSerchingRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipDrag;

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
        targetBoid = RogueManager.Instance.currentShip.GetComponent<IBoid>();
        targetActiveUnitList.AddRange(RogueManager.Instance.currentShip.UnitList);

        // allocate all job data 
        AllocateAIJobData();

    }

    public virtual void AllocateAIJobData()
    {
        targetActiveUnitPos = new NativeList<float3>(Allocator.Persistent);
        aiActiveUnitPos = new NativeList<float3>(Allocator.Persistent);

        aiActiveUnitAttackRange = new NativeList<float>(Allocator.Persistent);
        aiActiveUnitMaxTargetsCount = new NativeList<int>(Allocator.Persistent);


        steeringBehaviorJob_aiShipPos = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipVelocity = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRotationZ = new NativeList<float>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRadius = new NativeList<float>(Allocator.Persistent);

        aiSteeringBehaviorController_aiShipMaxAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipDrag = new NativeList<float>(Allocator.Persistent);


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


    }

public virtual void UpdateJobData()
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

        //weapon job data
        if(targetActiveUnitList.Count >0)
        {
            for (int i = 0; i < targetActiveUnitList.Count; i++)
            {
                targetActiveUnitPos[i] = targetActiveUnitList[i].transform.position;
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
        if (targetActiveUnitPos.IsCreated) { targetActiveUnitPos.Dispose(); }
        if (aiActiveUnitPos.IsCreated) { aiActiveUnitPos.Dispose(); }
        if(aiActiveUnitAttackRange.IsCreated) { aiActiveUnitAttackRange.Dispose(); }
        if (aiActiveUnitMaxTargetsCount.IsCreated) { aiActiveUnitMaxTargetsCount.Dispose(); }

        //ai data dispose
        if (steeringBehaviorJob_aiShipPos.IsCreated) { steeringBehaviorJob_aiShipPos.Dispose(); }
        if (steeringBehaviorJob_aiShipVelocity.IsCreated) { steeringBehaviorJob_aiShipVelocity.Dispose(); }
        if (steeringBehaviorJob_aiShipRadius.IsCreated) { steeringBehaviorJob_aiShipRadius.Dispose(); }
        if (steeringBehaviorJob_aiShipRotationZ.IsCreated) { steeringBehaviorJob_aiShipRotationZ.Dispose(); }

        if (aiSteeringBehaviorController_aiShipMaxAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipMaxAngularAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipTargetSerchingRadius.IsCreated) { aiSteeringBehaviorController_aiShipTargetSerchingRadius.Dispose(); }
        if (aiSteeringBehaviorController_aiShipDrag.IsCreated) { aiSteeringBehaviorController_aiShipDrag.Dispose(); }



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

        //weapon job data dispose
    }
    public virtual void DisposeAIJobDataFrame()
    {


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
        if(targetActiveUnitList != null)
        {
            if(targetActiveUnitList.Count >0)
            {
                ClearTarget();
            }
        }

        //clear all player

        //todo clear all ai weapon

        //todo clear all bullet

        //Dispose all job data
        DisposeAIJobData();
    }

    public void Stop()
    {
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
        //update weapon
        UpdateAIWeapon();
        UpdateBullet();
    }

    private void LaterUpdate()
    {

    }

    private void FixedUpdate()
    {
        UpdateJobData();

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

        //����Job��Ϣ

        steeringBehaviorJob_aiShipPos.Add(boid.GetPosition());
        steeringBehaviorJob_aiShipRadius.Add(boid.GetRadius());
        steeringBehaviorJob_aiShipVelocity.Add(boid.GetVelocity());
        steeringBehaviorJob_aiShipRotationZ.Add(boid.GetRotationZ());

        aiSteeringBehaviorController_aiShipMaxAcceleration.Add(controller.maxAcceleration);
        aiSteeringBehaviorController_aiShipMaxAngularAcceleration.Add(controller.maxAngularAcceleration);
        aiSteeringBehaviorController_aiShipTargetSerchingRadius.Add(controller.targetSerchingRadius);
        aiSteeringBehaviorController_aiShipDrag.Add(controller.drag);

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

        targetActiveUnitList.Clear();
    }
    public void  AddUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            if( ship.UnitList[i].isActiveAndEnabled &&  !aiActiveUnitList.Contains(ship.UnitList[i]))
            {
                AddSingleUnit(ship.UnitList[i]);
            }
        }
    }
    public void AddSingleUnit(Unit unit)
    {
        if(!aiActiveUnitList.Contains(unit))
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

    public void AddBullet(Bullet bullet)
    {
        if(!aibulletsList.Contains(bullet))
        aibulletsList.Add(bullet);
    }

    public void RemoveBullet(Bullet bullet)
    {
        aibulletsList.Remove(bullet);
    }

    public void AddTargetUnit(Unit unit)
    {
        if (!targetActiveUnitList.Contains(unit))
        {
            targetActiveUnitList.Add(unit);
            targetActiveUnitPos.Add(unit.transform.position);
        }
         

    }

    public void RemoveTargetUnit(Unit unit)
    {
        targetActiveUnitList.Remove(unit);
    }
    public void UpdateAIMovement()
    {

        if (aiShipList == null || aiShipList.Count == 0)
        {
            return;
        }

        JobHandle jobhandle_arrivebehavior;
        JobHandle jobhandle_facebehavior;
        JobHandle jobhandle_behaviorTargets;
        JobHandle jobhandle_cohesionbehavior;
        JobHandle jobhandle_separationBehavior;
        JobHandle jobhandle_AlignmentBehavior;
        //jobhandleList_AI = new NativeList<JobHandle>(Allocator.TempJob);

        rv_arrive_isVelZero = new NativeArray<bool>(ShipCount, Allocator.TempJob);
        rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);
        rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        ArriveBehavior.ArriveBehaviorJobs arriveBehaviorJobs = new ArriveBehavior.ArriveBehaviorJobs
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRadius = steeringBehaviorJob_aiShipRadius,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_arriveRadius = arrive_arriveRadius,
            job_maxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            job_slowRadius = arrive_slowRadius,
            job_targetPos = targetBoid.GetPosition(),
            job_targetRadius = targetBoid.GetRadius(),

            rv_isVelZero = rv_arrive_isVelZero,
            rv_Steerings = rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs.ScheduleBatch(ShipCount, 2);
        jobhandle_arrivebehavior.Complete();

        //FaceBehavior Job
        FaceBehavior.FaceBehaviorJob faceBehaviorJob = new FaceBehavior.FaceBehaviorJob
        {
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipRotationZ = steeringBehaviorJob_aiShipRotationZ,
            job_aiShipVel = steeringBehaviorJob_aiShipVelocity,
            job_facetagetRadius = face_facetargetRadius,
            job_maxAngularAcceleration = aiSteeringBehaviorController_aiShipMaxAngularAcceleration,
            job_targetPos = targetBoid.GetPosition(),
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

        jobhandle_AlignmentBehavior = alignmentBehaviorJob.ScheduleBatch(ShipCount, 2);
        jobhandle_AlignmentBehavior.Complete();





        rv_deltaMovement = new NativeArray<SteeringBehaviorInfo>(ShipCount, Allocator.TempJob);

        JobHandle jobhandle_deltamoveposjob;
        AISteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new AISteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_aiShipMaxAcceleration = aiSteeringBehaviorController_aiShipMaxAcceleration,
            Job_aiShipDrag = aiSteeringBehaviorController_aiShipDrag,
            job_aiShipPos = steeringBehaviorJob_aiShipPos,
            job_aiShipVelocity = steeringBehaviorJob_aiShipVelocity,

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

            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(ShipCount, 2);

        jobhandle_deltamoveposjob.Complete();
      

        for (int i = 0; i < ShipCount; i++)
        {
            aiSteeringBehaviorControllerList[i].UpdateIBoid();
            if(rv_arrive_isVelZero[i])
            {
                aiSteeringBehaviorControllerList[i].SetVelocity(Vector3.zero);
            }
            aiSteeringBehaviorControllerList[i].Move(rv_deltaMovement[i].linear);
            aiSteeringBehaviorControllerList[i].transform.rotation = Quaternion.Euler(0, 0,rv_deltaMovement[i].angular);


        }

        rv_arrive_isVelZero.Dispose();
        rv_arrive_steeringInfo.Dispose();

        rv_face_steeringInfo.Dispose();
        rv_cohesion_steeringInfo.Dispose();
        rv_separation_steeringInfo.Dispose();
        rv_alignment_steeringInfo.Dispose();

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

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindWeaponTargetsJob
        {
            job_attackRange = aiActiveUnitAttackRange,
            job_selfPos = aiActiveUnitPos,
            job_targetsPos =targetActiveUnitPos,
            job_maxTargetCount = aiActiveUnitMaxTargetsCount,

            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(targetActiveUnitList.Count, 1);


        jobHandle.Complete();
        //ִ��Job ���ҵȴ������



        int startindex;
        int targetindex;
        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            //��ȡFlat Array�е�startindex Ϊ�����Ĳ����׼��
            //��ǰ��ÿһ��unit�� maxtargetscountȫ������������FlatArray�еĵ�һ��index
            startindex = 0;
            for (int c = 0; c < i; c++)
            {
                startindex += aiActiveUnitMaxTargetsCount[i];
            }

            if(aiActiveUnitList[i] is AIWeapon)
            {
                weapon = aiActiveUnitList[i] as AIWeapon;





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
                        weapon.targetList.Clear();
                        weapon.targetList.Add(new WeaponTargetInfo
                            (
                                targetActiveUnitList[targetindex].gameObject,
                                rv_weaponTargetsInfo[startindex + n].targetIndex,
                                rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                                rv_weaponTargetsInfo[startindex + n].targetDirection
                            ));
                        weapon.WeaponOn();
                    }
                }
                weapon.ProcessWeapon();
            }
            
        }

        rv_weaponTargetsInfo.Dispose();
        //Dispose all Job tempdata
    }

    public void UpdateBullet()
    {

    }






}

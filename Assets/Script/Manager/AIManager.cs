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

    public List<Unit> aiActiveUnitList = new List<Unit>();
    public List<AISteeringBehaviorController> aiSteeringBehaviorControllerList = new List<AISteeringBehaviorController>();
    public List<IBoid> aiShipBoidList = new List<IBoid>();

    //AI component list
    public List<ArriveBehavior> arriveBehavior = new List<ArriveBehavior>();
    public List<FaceBehavior> faceBehavior = new List<FaceBehavior>();
    public List<CohesionBehavior> cohesionBehavior = new List<CohesionBehavior>();
    public List<SeparationBehavior> separationBehavior = new List<SeparationBehavior>();
    public List<AlignmentBehavior> aligmentBehavior = new List<AlignmentBehavior>();

    //target info
    public IBoid targetBoid;
    public List<Unit> targetActiveUnitList = new List<Unit>();


    public NativeList<float3> aiActiveUnitPos;
    public NativeList<float3> targetActiveUnitPos;


    public NativeList<float3> steeringBehaviorJob_aiShipPos;
    public NativeList<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeList<float> steeringBehavorJob_aiShipRotationZ;
    public NativeList<float> steeringBehaviorJob_aiShipRadius;
    public NativeList<float> aiSteeringBehaviorController_aiShipMaxAcceleration;
    public NativeList<float> aiSteeringBehaviorController_aiShipDrag;



    public NativeList<JobHandle> jobHandleList_AIWeapon;

    //weapon Update job result data
    NativeArray<Weapon.JRD_WeaponTargetInfo>[] weaponTargetsInfo;


    //arrivebehavior return data
    public NativeList<SteeringBehaviorInfo> rv_arrive_steeringInfo;
    public NativeList<bool> rv_arrive_isVelZero;
    //arrivebehavior job input data
    public NativeList<float> arrive_arriveRadius;
    public NativeList<float> arrive_slowRadius;
    public NativeList<float> arrive_weight;



    //facebehavior return data
    public NativeList<SteeringBehaviorInfo> rv_face_steeringInfo;
    //facebehavior job input data
    public NativeList<float> face_weight;



    public NativeList<SteeringBehaviorInfo> rv_cohesion_steeringInfo;
    public NativeList<float> cohesion_weight;

    public NativeList<SteeringBehaviorInfo> rv_separation_steeringInfo;
    public NativeList<float> separation_weight;

    public NativeList<SteeringBehaviorInfo> rv_alignment_steeringInfo;
    public NativeList<float> alignment_weight;

    public NativeList<SteeringBehaviorInfo> rv_deltaMovement;

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

        steeringBehaviorJob_aiShipPos = new NativeList<float3>(Allocator.Persistent);
        steeringBehaviorJob_aiShipVelocity = new NativeList<float3>(Allocator.Persistent);
        steeringBehavorJob_aiShipRotationZ = new NativeList<float>(Allocator.Persistent);
        steeringBehaviorJob_aiShipRadius = new NativeList<float>(Allocator.Persistent);

        aiSteeringBehaviorController_aiShipMaxAcceleration = new NativeList<float>(Allocator.Persistent);
        aiSteeringBehaviorController_aiShipDrag = new NativeList<float>(Allocator.Persistent);


        //arrivebehavior return data
        rv_arrive_steeringInfo = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
        rv_arrive_isVelZero= new NativeList<bool>(Allocator.Persistent);
        //arrivebehavior job input data
        arrive_arriveRadius = new NativeList<float>(Allocator.Persistent);
        arrive_slowRadius = new NativeList<float>(Allocator.Persistent);
        arrive_weight = new NativeList<float>(Allocator.Persistent);


        //facebehavior return data
        rv_face_steeringInfo = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
        //facebehavior job input data
        face_weight = new NativeList<float>(Allocator.Persistent);



        rv_cohesion_steeringInfo = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
        cohesion_weight = new NativeList<float>(Allocator.Persistent);

        rv_separation_steeringInfo = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
        separation_weight = new NativeList<float>(Allocator.Persistent);

        rv_alignment_steeringInfo = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
        alignment_weight = new NativeList<float>(Allocator.Persistent);

        rv_deltaMovement = new NativeList<SteeringBehaviorInfo>(Allocator.Persistent);
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
                steeringBehavorJob_aiShipRotationZ[i] = aiShipBoidList[i].GetRotationZ();
            }
        }

        //weapon job data


        for (int i = 0; i < targetActiveUnitList.Count; i++)
        {
            targetActiveUnitPos[i] = targetActiveUnitList[i].transform.position;
        }

        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            aiActiveUnitPos[i] = aiActiveUnitList[i].transform.position;
        }


    }
    public virtual void DisposeAIJobData()
    {

        //ai data dispose
        if (steeringBehaviorJob_aiShipPos.IsCreated) { steeringBehaviorJob_aiShipPos.Dispose(); }
        if (steeringBehaviorJob_aiShipVelocity.IsCreated) { steeringBehaviorJob_aiShipVelocity.Dispose(); }
        if (steeringBehaviorJob_aiShipRadius.IsCreated) { steeringBehaviorJob_aiShipRadius.Dispose(); }
        if (steeringBehavorJob_aiShipRotationZ.IsCreated) { steeringBehavorJob_aiShipRotationZ.Dispose(); }

        if (aiSteeringBehaviorController_aiShipMaxAcceleration.IsCreated) { aiSteeringBehaviorController_aiShipMaxAcceleration.Dispose(); }
        if (aiSteeringBehaviorController_aiShipDrag.IsCreated) { aiSteeringBehaviorController_aiShipDrag.Dispose(); }



        //dispose arrive job data
        if (rv_arrive_steeringInfo.IsCreated) { rv_arrive_steeringInfo.Dispose(); };
        if (rv_arrive_isVelZero.IsCreated) { rv_arrive_isVelZero.Dispose(); }
        if (arrive_arriveRadius.IsCreated) { arrive_arriveRadius.Dispose(); }
        if (arrive_slowRadius.IsCreated) { arrive_slowRadius.Dispose(); }
        if (arrive_weight.IsCreated) { arrive_weight.Dispose(); }


        //dispose face job data
        if (rv_face_steeringInfo.IsCreated) { rv_face_steeringInfo.Dispose(); }
        if (face_weight.IsCreated) { face_weight.Dispose(); }

        if (rv_separation_steeringInfo.IsCreated) { rv_separation_steeringInfo.Dispose(); }
        if (separation_weight.IsCreated) { separation_weight.Dispose(); }


        if (rv_cohesion_steeringInfo.IsCreated) { rv_cohesion_steeringInfo.Dispose(); }
        if (cohesion_weight.IsCreated) { cohesion_weight.Dispose(); }

        if (rv_alignment_steeringInfo.IsCreated) { rv_alignment_steeringInfo.Dispose(); }
        if (alignment_weight.IsCreated) { alignment_weight.Dispose(); }

        if (rv_deltaMovement.IsCreated) { rv_deltaMovement.Dispose(); }

        //weapon job data dispose
    }
    public virtual void DisposeAIJobDataFrame()
    {
        if (targetActiveUnitPos.IsCreated) { targetActiveUnitPos.Dispose(); }
        if (aiActiveUnitPos.IsCreated) { aiActiveUnitPos.Dispose(); }
        if (jobHandleList_AIWeapon.IsCreated) { jobHandleList_AIWeapon.Dispose(); }
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
        //UpdateAIWeapon();
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
        aiShipBoidList.Add(ship.GetComponent<IBoid>());

        AddUnit(ship);
        aiSteeringBehaviorControllerList.Add(ship.GetComponent<AISteeringBehaviorController>());

        //更新Job信息


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
                aiActiveUnitList.Add(ship.UnitList[i]);
            }
        }
    }
    public void AddSingleUnit(Unit unit)
    {
        if(!aiActiveUnitList.Contains(unit))
        {
            aiActiveUnitList.Add(unit);
        }
    }
    public void RemoveReminedUnit(AIShip ship)
    {
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            if(ship.UnitList[i].isActiveAndEnabled)
            {
                aiActiveUnitList.Remove(ship.UnitList[i]);
            }
        }
    }

    public void RemoveSingleUnit(Unit unit)
    {
        aiActiveUnitList.Remove(unit);
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
            targetActiveUnitList.Add(unit);
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
        //jobhandleList_AI = new NativeList<JobHandle>(Allocator.TempJob);

        rv_arrive_isVelZero.Clear();
        rv_arrive_steeringInfo.Clear();

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
            rv_Steering = rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs.ScheduleBatch(ShipCount, 32);
        //需要添加其他behavior的Job ，统一计算；

        //for (int i = 0; i < ShipCount; i++)
        //{

        //    if (aiSteeringBehaviorControllerList[i].arriveBehavior)
        //    {
        //        ArriveBehavior.ArriveBehaviorJob arriveBehaviorJob = new ArriveBehavior.ArriveBehaviorJob
        //        {
        //            job_maxAcceleration = aiSteeringBehaviorControllerList[i].maxAcceleration,
        //            job_selfPos = steeringBehaviorJob_aiShipPos[i],
        //            job_selfVel = steeringBehaviorJob_aiShipVelocity[i],
        //            job_slowRadius = aiSteeringBehaviorControllerList[i].arrivelBehaviorInfo.slowRadius,
        //            job_targetPos = targetBoid.GetPosition(),
        //            job_targetRadius = targetBoid.GetRadius(),
        //            //return value
        //            JRD_isvelzero = rv_arrive_isVelZero[i],
        //            JRD_linear = rv_arrive_steeringInfo[i],
        //        };

        //        JobHandle jobhandle = arriveBehaviorJob.Schedule();
        //        jobhandleList_AI.Add(jobhandle);
        //    }
        //}

        //Wait to complate Jobs
        //JobHandle.CompleteAll(jobhandleList_AI);

        jobhandle_arrivebehavior.Complete();

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

            job_cohesionSteering = rv_cohesion_steeringInfo,

            job_separationSteering = rv_separation_steeringInfo,

            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(ShipCount, 32);

        for (int i = 0; i < ShipCount; i++)
        {
            aiSteeringBehaviorControllerList[i].UpdateIBoid();
            aiSteeringBehaviorControllerList[i].Move(rv_deltaMovement[i].linear);
           
        }
    }





    public void UpdateAIWeapon()
    {

        if( aiActiveUnitList == null || aiActiveUnitList.Count == 0)
        {
            return;
        }
        AIWeapon weapon;
        weaponTargetsInfo = new NativeArray<Weapon.JRD_WeaponTargetInfo>[aiActiveUnitList.Count];
        jobHandleList_AIWeapon = new NativeList<JobHandle>(Allocator.TempJob);

        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            //检查unit是否为AIweapon
            // 检查所有AIweapon的状态， 是否被击破
            if (aiActiveUnitList[i] is AIWeapon)
            {
                weapon = aiActiveUnitList[i] as AIWeapon;
            }
            else
            {
                continue;
            }

            // 检查所有AiWeapon是否 isprocess 并且 武器状态时Ready
            if (!weapon.IsProcess && weapon.weaponstate.CurrentState != WeaponState.Ready)
            {
                continue;
            }

            // 如果Process 则开启索敌Job， 并且蒋索敌结果记录在targetsindex[]中
            //这个Job返回一个JRD_targetsInfo 这个是已经排序的数据， 包含 index， Pos， direction， distance 几部分数据
           
            weaponTargetsInfo[i] = new NativeArray<Weapon.JRD_WeaponTargetInfo>(weapon.maxTargetCount, Allocator.TempJob);

            Weapon.FindWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindWeaponTargetsJob
            {
                job_attackRange = weapon.weaponAttribute.WeaponRange,
                job_selfPos = weapon.transform.position,
                job_targetsPos = targetActiveUnitPos,
                job_maxTargetCount = weapon.maxTargetCount,

                JRD_targetsInfo = weaponTargetsInfo[i],

            };

            JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(targetActiveUnitList.Count, 20);
            jobHandleList_AIWeapon.Add(jobHandle);
        }

        //执行Job 并且等待结果。
        JobHandle.CompleteAll(jobHandleList_AIWeapon);



        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            if( aiActiveUnitList[i] is AIWeapon)
            {
                weapon = aiActiveUnitList[i] as AIWeapon;
            }
            else
            {
                continue;
            }
            if(weaponTargetsInfo[i].Length <= 0)
            {
                weapon.WeaponOff();

            }
            else
            {
                weapon.targetList.Clear();
                for (int n = 0; n < weapon.maxTargetCount; n++)
                {
                    weapon.targetList.Add(new WeaponTargetInfo
                        (
                            targetActiveUnitList[ weaponTargetsInfo[i][n].JRD_targetIndex].gameObject,
                            weaponTargetsInfo[i][n].JRD_targetIndex,
                            weaponTargetsInfo[i][n].JRD_distanceToTarget,
                            weaponTargetsInfo[i][n].JRD_targetDirection
                        ));
                }
               

                weapon.ProcessWeapon();
                weaponTargetsInfo[i].Dispose();
            }


            //更具结果计算对应的AI旋转角度


            
        }


        //Dispose all Job tempdata
    }

    public void UpdateBullet()
    {

    }






}

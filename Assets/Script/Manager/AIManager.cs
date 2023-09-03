using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;



public class AIManager : Singleton<AIManager>
{
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


    public NativeArray<float3> aiActiveUnitPos;
    public NativeArray<float3> targetActiveUnitPos;

    public NativeArray<float3> steeringBehaviorJob_aiShipPos;
    public NativeArray<float3> steeringBehaviorJob_aiShipVelocity;
    public NativeArray<float> steeringBehavorJob_aiShipRotationZ;
    public NativeArray<float> steeringBehaviorJob_aiShipRadius;



    public NativeList<JobHandle> jobhandleList_AI;
    public NativeList<JobHandle> jobHandleList_AIWeapon;

    //weapon Update job result data
    NativeArray<Weapon.JRD_WeaponTargetInfo>[] weaponTargetsInfo;

    //arrivebehavior native data
    public NativeArray<float3>[] arrivebehavior_steeringlinear;
    NativeArray<bool>[] arrive_isVelZero;


    public NativeArray<float3>[] facebehavior_steeringangle;
    public NativeArray<float3>[] cohesionbehavior_steeringlinear;
    public NativeArray<float3>[] separationbehavior_steeringlinear;
    public NativeArray<float3>[] alignmentbehavior_steeringlinear;




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

    }

    private void LaterUpdate()
    {

    }

    private void FixedUpdate()
    {
        UpdateJobData();

        UpdateAI();
        //update Ai

        //update weapon
        UpdateAIWeapon();

        //update bullet
        UpdateBullet();

        DisposeJobData();
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

        arriveBehavior.Add(ship.GetComponent<ArriveBehavior>());
        faceBehavior.Add(ship.GetComponent<FaceBehavior>());
        cohesionBehavior.Add(ship.GetComponent<CohesionBehavior>());
        separationBehavior.Add(ship.GetComponent<SeparationBehavior>());
        aligmentBehavior.Add(ship.GetComponent<AlignmentBehavior>());
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
        arriveBehavior.RemoveAt(index);
        faceBehavior.RemoveAt(index);
        cohesionBehavior.RemoveAt(index);
        separationBehavior.RemoveAt(index);
        aligmentBehavior.RemoveAt(index);
    }


    public void ClearAI()
    {
        aiShipList.Clear();
        aiShipBoidList.Clear();
        aiActiveUnitList.Clear();
        aiSteeringBehaviorControllerList.Clear();
        arriveBehavior.Clear();
        faceBehavior.Clear();
        cohesionBehavior.Clear();
        separationBehavior.Clear();
        aligmentBehavior.Clear();
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
    public void UpdateAI()
    {

        if (aiShipList == null || aiShipList.Count == 0)
        {
            return;
        }

        arrivebehavior_steeringlinear = new NativeArray<float3>[ShipCount];
        facebehavior_steeringangle = new NativeArray<float3>[ShipCount];
        cohesionbehavior_steeringlinear = new NativeArray<float3>[ShipCount];
        separationbehavior_steeringlinear = new NativeArray<float3>[ShipCount];
        alignmentbehavior_steeringlinear = new NativeArray<float3>[ShipCount];

        arrive_isVelZero = new NativeArray<bool>[ShipCount];
        jobhandleList_AI = new NativeList<JobHandle>(Allocator.TempJob);

        for (int i = 0; i < ShipCount; i++)
        {
            arrive_isVelZero[i] = new NativeArray<bool>(1, Allocator.TempJob);

            arrivebehavior_steeringlinear[i] = new NativeArray<float3>(1, Allocator.TempJob);

            if (aiSteeringBehaviorControllerList[i].arriveBehavior)
            {
                ArriveBehavior.ArriveBehaviorJob arriveBehaviorJob = new ArriveBehavior.ArriveBehaviorJob
                {
                    job_maxAcceleration = aiSteeringBehaviorControllerList[i].maxAcceleration,
                    job_selfPos = steeringBehaviorJob_aiShipPos[i],
                    job_selfVel = steeringBehaviorJob_aiShipVelocity[i],
                    job_slowRadius = arriveBehavior[i].slowRadius,
                    job_targetPos = targetBoid.GetPosition(),
                    job_targetRadius = targetBoid.GetRadius(),
                    //return value
                    JRD_isvelzero = arrive_isVelZero[i],
                    JRD_linear = arrivebehavior_steeringlinear[i],
                };

                JobHandle jobhandle = arriveBehaviorJob.Schedule();
                jobhandleList_AI.Add(jobhandle);
            }

        }

        //Wait to complate Jobs
        JobHandle.CompleteAll(jobhandleList_AI);

        // apply steering data to ai ship
        float3 accelaration;
        float angle;
        Vector3 deltamovement;

        // loop through all airuntimedata 
        for (int i = 0; i < ShipCount; i++)
        {
            if (arrive_isVelZero[i][0])
            {
                aiShipBoidList[i].SetVelocity(Vector3.zero);
            }



            accelaration = float3.zero;
            angle = 0;
            deltamovement = Vector3.zero;

            accelaration += arrivebehavior_steeringlinear[i][0] * arriveBehavior[i].GetWeight();
            //add other behavior steeringdata
            //angle += facebehavior_steeringangle[i][0] * airuntimedata.arriveBehavior[i].GetWeight();

            //make sure the length of accelearation in every frame dosen't great than max acceleration 
            if (math.length(accelaration) > aiSteeringBehaviorControllerList[i].maxAcceleration)
            {
                accelaration = math.normalize(accelaration);
                accelaration *= aiSteeringBehaviorControllerList[i].maxAcceleration;
            }

            //Calculate movement delta
            deltamovement = (aiSteeringBehaviorControllerList[i].velocity + accelaration.ToVector3() * 0.5f * Time.deltaTime * aiSteeringBehaviorControllerList[i].drag);
            aiSteeringBehaviorControllerList[i].Move(aiShipList[i].transform.position + deltamovement * Time.deltaTime);
            //aiSteeringBehaviorControllerList[i].rb.MovePosition();
            //rb.AddForce(accelaration);

            //Update Boiddata;
            aiSteeringBehaviorControllerList[i].UpdateIBoid();

            //Rotate ship if angle != 0;
            if (angle != 0)
            {
                aiShipList[i].controller.rb.rotation = angle; //Quaternion.Euler(0, rotation, 0);
            }
            //Dispose all job data
            arrive_isVelZero[i].Dispose();
            arrivebehavior_steeringlinear[i].Dispose();
        }



        //Dispose all Boid data at the end of frame

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
            }


            //更具结果计算对应的AI旋转角度


            
        }


        //Dispose all Job tempdata
        for (int i = 0; i < weaponTargetsInfo.Length; i++)
        {
            weaponTargetsInfo[i].Dispose();
        }
    }

    public void UpdateBullet()
    {

    }

    public virtual void DisposeJobData()
    {
        if (jobhandleList_AI.IsCreated) { jobhandleList_AI.Dispose(); }

        //ai data dispose
        if (steeringBehaviorJob_aiShipPos.IsCreated) { steeringBehaviorJob_aiShipPos.Dispose(); }
        if (steeringBehaviorJob_aiShipVelocity.IsCreated) { steeringBehaviorJob_aiShipVelocity.Dispose(); }
        if (steeringBehaviorJob_aiShipRadius.IsCreated) { steeringBehaviorJob_aiShipRadius.Dispose(); }
        if (steeringBehavorJob_aiShipRotationZ.IsCreated) { steeringBehavorJob_aiShipRotationZ.Dispose(); }


        if (targetActiveUnitPos.IsCreated) { targetActiveUnitPos.Dispose(); }
        if (aiActiveUnitPos.IsCreated) { aiActiveUnitPos.Dispose(); }
        if (jobHandleList_AIWeapon.IsCreated) { jobHandleList_AIWeapon.Dispose(); }


            
       

        //weapon job data dispose



    }

    public virtual void UpdateJobData()
    {
     
        // ship Job data
        if(aiShipBoidList != null && aiShipBoidList.Count != 0)
        {
            steeringBehaviorJob_aiShipPos = new NativeArray<float3>(aiShipBoidList.Count, Allocator.TempJob);
            steeringBehaviorJob_aiShipVelocity = new NativeArray<float3>(aiShipBoidList.Count, Allocator.TempJob);
            steeringBehavorJob_aiShipRotationZ = new NativeArray<float>(aiShipBoidList.Count, Allocator.TempJob);
            steeringBehaviorJob_aiShipRadius = new NativeArray<float>(aiShipBoidList.Count, Allocator.TempJob);

            for (int i = 0; i < ShipCount; i++)
            {
                steeringBehaviorJob_aiShipPos[i] = aiShipBoidList[i].GetPosition();
                steeringBehaviorJob_aiShipVelocity[i] = aiShipBoidList[i].GetVelocity();
                steeringBehaviorJob_aiShipRadius[i] = aiShipBoidList[i].GetRadius();
                steeringBehavorJob_aiShipRotationZ[i] = aiShipBoidList[i].GetRotationZ();
            }
        }
        //weapon job data
        if (targetActiveUnitList != null && targetActiveUnitList.Count != 0)
        {
            targetActiveUnitPos = new NativeArray<float3>(targetActiveUnitList.Count, Allocator.TempJob);
            aiActiveUnitPos = new NativeArray<float3>(aiActiveUnitList.Count, Allocator.TempJob);
        }




        for (int i = 0; i < targetActiveUnitList.Count; i++)
        {
            targetActiveUnitPos[i] = targetActiveUnitList[i].transform.position;
        }

        for (int i = 0; i < aiActiveUnitList.Count; i++)
        {
            aiActiveUnitPos[i] = aiActiveUnitList[i].transform.position;
        }

        
    }


}

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;





public class JobController : IPauseable
{

    public SearchingTargetData searchingTargetData;
    public JobController()
    {

    }

    ~ JobController()
    {

    }

    public virtual void Initialization()
    {
      

    }

    public virtual void PauseGame()
    {
        
    }

    public virtual void UnPauseGame()
    {
        
    }

    public virtual void UpdateAgentMovement(ref AgentData activeSelfAgentData, ref AgentData activeTargetAgentData, ref IBoid boidTarget)
    {
        if (activeSelfAgentData.shipList == null || activeSelfAgentData.shipList.Count == 0)
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

        activeSelfAgentData.rv_evade_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_arrive_isVelZero = new NativeArray<bool>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);
        activeSelfAgentData.rv_collisionavoidance_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);



        EvadeBehavior.EvadeBehaviorJob evadeBehaviorJob = new EvadeBehavior.EvadeBehaviorJob
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,
            job_evadeTargetPos = boidTarget.GetPosition(),
            job_evadeTargetVel = boidTarget.GetVelocity(),

            rv_steering = activeSelfAgentData.rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_evadebehavior.Complete();




        ArriveBehavior.ArriveBehaviorJobs arriveBehaviorJobs = new ArriveBehavior.ArriveBehaviorJobs
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,

            job_targetPos = boidTarget.GetPosition(),
            job_targetRadius = boidTarget.GetRadius(),

            rv_isVelZero = activeSelfAgentData.rv_arrive_isVelZero,
            rv_Steerings = activeSelfAgentData.rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_arrivebehavior.Complete();

        ////FaceBehavior Job
        FaceBehavior.FaceBehaviorJob faceBehaviorJob = new FaceBehavior.FaceBehaviorJob
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,

            job_targetPos = boidTarget.GetPosition(),
            job_deltatime = Time.fixedDeltaTime,

            rv_Steerings = activeSelfAgentData.rv_face_steeringInfo,
        };
        jobhandle_facebehavior = faceBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_facebehavior.Complete();

        //NativeList<JobHandle> behaviorTargetsList = new NativeList<JobHandle>();

        searchingTargetData.rv_searchingTargetsVelFlatArray = new NativeArray<float3>(activeSelfAgentData.shipList.Count * activeSelfAgentData.shipList.Count, Allocator.TempJob);
        searchingTargetData.rv_searchingTargetsPosFlatArray = new NativeArray<float3>(activeSelfAgentData.shipList.Count * activeSelfAgentData.shipList.Count, Allocator.TempJob);
        searchingTargetData.rv_searchingTargetsCount = new NativeArray<int>(activeSelfAgentData.shipList.Count, Allocator.TempJob);


        SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob calculateSteeringTargetsPosByRadiusJob = new SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,


            job_threshold = 0.01f,


            rv_findedTargetCountPreShip = searchingTargetData.rv_searchingTargetsCount,
            rv_findedTargetVelPreShip = searchingTargetData.rv_searchingTargetsVelFlatArray,
            rv_findedTargetsPosPreShip = searchingTargetData.rv_searchingTargetsPosFlatArray,
        };
        jobhandle_behaviorTargets = calculateSteeringTargetsPosByRadiusJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_behaviorTargets.Complete();


        //Cohesion Job

        CohesionBehavior.CohesionBehaviorJob cohesionBehaviorJob = new CohesionBehavior.CohesionBehaviorJob
        {
            job_shipcount = activeSelfAgentData.shipList.Count,
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,
            rv_Steerings = activeSelfAgentData.rv_cohesion_steeringInfo,
        };
        jobhandle_cohesionbehavior = cohesionBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_cohesionbehavior.Complete();

        //separation Job
        SeparationBehavior.SeparationBehaviorJob separationBehaviorJob = new SeparationBehavior.SeparationBehaviorJob
        {
            job_shipcount = activeSelfAgentData.shipList.Count,
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,

            rv_Steerings = activeSelfAgentData.rv_separation_steeringInfo,
        };
        jobhandle_separationBehavior = separationBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);

        jobhandle_separationBehavior.Complete();

        //Alignment Job
        AlignmentBehavior.AlignmentBehaviorJob alignmentBehaviorJob = new AlignmentBehavior.AlignmentBehaviorJob
        {
            job_shipcount = activeSelfAgentData.shipList.Count,
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetVelFlatArray = searchingTargetData.rv_searchingTargetsVelFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,
            rv_Steerings = activeSelfAgentData.rv_alignment_steeringInfo,
        };

        jobhandle_alignmentBehavior = alignmentBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_alignmentBehavior.Complete();




        CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,
            job_avoidenceData = activeTargetAgentData.boidAgentJobData,
            rv_steering = activeSelfAgentData.rv_collisionavoidance_steeringInfo,
        };

        jobhandle_collisionAvoidanceBehavior = collisionAvoidanceBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_collisionAvoidanceBehavior.Complete();




        activeSelfAgentData.rv_deltaMovement = new NativeArray<SteeringBehaviorInfo>(activeSelfAgentData.shipList.Count, Allocator.TempJob);

        JobHandle jobhandle_deltamoveposjob;
        SteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new SteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,

            job_evadeSteering = activeSelfAgentData.rv_evade_steeringInfo,


            job_arriveSteering = activeSelfAgentData.rv_arrive_steeringInfo,
            job_isVelZero = activeSelfAgentData.rv_arrive_isVelZero,

            job_faceSteering = activeSelfAgentData.rv_face_steeringInfo,


            job_alignmentSteering = activeSelfAgentData.rv_alignment_steeringInfo,


            job_cohesionSteering = activeSelfAgentData.rv_cohesion_steeringInfo,


            job_separationSteering = activeSelfAgentData.rv_separation_steeringInfo,


            job_collisionAvoidanceSteering = activeSelfAgentData.rv_collisionavoidance_steeringInfo,


            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = activeSelfAgentData.rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_deltamoveposjob.Complete();


        for (int i = 0; i < activeSelfAgentData.shipList.Count; i++)
        {
            activeSelfAgentData.steeringControllerList[i].UpdateIBoid();

            activeSelfAgentData.steeringControllerList[i].Move(activeSelfAgentData.rv_deltaMovement[i].linear);
            activeSelfAgentData.steeringControllerList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfAgentData.rv_deltaMovement[i].angular);
        }

        boidTarget.UpdateIBoid();

        activeSelfAgentData.DisposeReturnValue();
        searchingTargetData.DisposeReturnValue();

    }

    public virtual void  UpdateAdditionalWeapon(ref WeaponData activeSelfWeaponData, ref UnitData activeTargetUnitData)
    {
        if (activeSelfWeaponData.activeWeaponList == null || activeSelfWeaponData.activeWeaponList.Count == 0)
        {
            return;
        }
        AdditionalWeapon weapon;

        int targetstotalcount = 0;
        for (int i = 0; i < activeSelfWeaponData.activeWeaponList.Count; i++)
        {
            targetstotalcount += activeSelfWeaponData.activeWeaponList[i].maxTargetCount;
        }

        activeSelfWeaponData.rv_weaponTargetsInfo = new NativeArray<Weapon.WeaponTargetInfo>(targetstotalcount, Allocator.TempJob);

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindMutipleWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleWeaponTargetsJob
        {
            job_weaponJobData = activeSelfWeaponData.activeWeaponJobData,
            job_targetsPos = activeTargetUnitData.activeTargetUnitPos,

            rv_targetsInfo = activeSelfWeaponData.rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(activeSelfWeaponData.activeWeaponList.Count, 2);


        jobHandle.Complete();
        //ִ��Job ���ҵȴ������



        int startindex;
        int targetindex;
        for (int i = 0; i < activeSelfWeaponData.activeWeaponList.Count; i++)
        {
            //��ȡFlat Array�е�startindex Ϊ�����Ĳ����׼��
            //��ǰ��ÿһ��unit�� maxtargetscountȫ������������FlatArray�еĵ�һ��index

            if (activeSelfWeaponData.activeWeaponList[i] is AdditionalWeapon)
            {

                startindex = 0;
                for (int c = 0; c < i; c++)
                {
                    startindex += activeSelfWeaponData.activeWeaponJobData[c].targetCount;
                }


                weapon = activeSelfWeaponData.activeWeaponList[i] as AdditionalWeapon;

                //���û���ڿ�������ڿ����Ъ�У�������ˢдweapon.targetlist
                if (weapon.weaponstate.CurrentState != WeaponState.Firing && weapon.weaponstate.CurrentState != WeaponState.BetweenDelay)
                {
                    weapon.targetList.Clear();
                    for (int n = 0; n < activeSelfWeaponData.activeWeaponJobData[i].targetCount; n++)
                    {
                        targetindex = activeSelfWeaponData.rv_weaponTargetsInfo[startindex + n].targetIndex;
                        if (targetindex == -1 || activeTargetUnitData.activeTargetUnitList == null || activeTargetUnitData.activeTargetUnitList.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            weapon.targetList.Add(new WeaponTargetInfo
                                (
                                    activeTargetUnitData.activeTargetUnitList[targetindex].gameObject,
                                    activeSelfWeaponData.rv_weaponTargetsInfo[startindex + n].targetIndex,
                                    activeSelfWeaponData.rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                                    activeSelfWeaponData.rv_weaponTargetsInfo[startindex + n].targetDirection
                                ));
                        }
                    }
                }
                if (weapon.targetList != null && weapon.targetList.Count != 0)
                {
                    weapon.WeaponOn();
                }
                else
                {
                    weapon.WeaponOff();
                }

                weapon.ProcessWeapon();
            }
        }

        activeSelfWeaponData.UpdateData();
        activeSelfWeaponData.DisposeReturnValue();

    }
    public virtual void UpdateAdditionalBuilding(ref BuildingData activeSelfBuildingData, ref UnitData activeTargetUnitData)
    {
        activeSelfBuildingData.UpdateData();
        activeSelfBuildingData.DisposeReturnValue();
    }

    public virtual void UpdateProjectile(ref ProjectileData activeSelfProjectileData, ref UnitData activeTargetUnitData)
    {
        if (activeSelfProjectileData.activeProjectileList == null || activeSelfProjectileData.activeProjectileList.Count == 0)
        {
            return;
        }
        activeSelfProjectileData.rv_activeProjectileUpdateInfo = new NativeArray<ProjectileJobRetrunInfo>(activeSelfProjectileData.activeProjectileList.Count, Allocator.TempJob);
        // ��ȡ�ӵ�����
        // ��ȡ�ӵ���Target�����Ҹ���Targetλ�ã� �����
        JobHandle aibulletjobhandle;
        //�����ӵ���������Ƿ�targetbase��������Ӧ��JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = activeSelfProjectileData.activeProjectileJobData,
            rv_bulletJobUpdateInfos = activeSelfProjectileData.rv_activeProjectileUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(activeSelfProjectileData.activeProjectileList.Count, 2);
        aibulletjobhandle.Complete();
        //�����ӵ���ǰ��JobData


        //�����ӵ����˺�List �Ͷ�Ӧ������List
        activeSelfProjectileData.deathProjectileIndexList.Clear();
        activeSelfProjectileData.damageProjectileList.Clear();
        activeSelfProjectileData.damageProjectileJobData.Clear();

        //Loop ���е��ӵ����Ҵ������ǵ��˺�������List��ע���˺���������List������һһ��Ӧ�ġ� ��Щ�ӵ��ڲ����˺�֮�󲢲������������紩͸�ӵ�
        for (int i = 0; i < activeSelfProjectileData.activeProjectileList.Count; i++)
        {
            //�ƶ��ӵ�
            //�����ӵ���ת����
            if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended)
            {
                activeSelfProjectileData.activeProjectileList[i].UpdateBullet();
            }
            //�ӵ����ƶ����˺��������б�ά����Ҫ�����ĸ���ͬ��������
            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //����Move �� Rotation
                if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended && !activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    activeSelfProjectileData.activeProjectileList[i].Move(activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    activeSelfProjectileData.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    if (activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        activeSelfProjectileData.damageProjectileList.Add(activeSelfProjectileData.activeProjectileList[i]);
                        activeSelfProjectileData.damageProjectileJobData.Add(activeSelfProjectileData.activeProjectileJobData[i]);
                    }
                    //Collide�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }

            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough ���͵��ӵ��� ֮����������ĩβ
                if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended && activeSelfProjectileData.activeProjectileList[i].TransFixionCount > 0)
                {
                    activeSelfProjectileData.activeProjectileList[i].Move(activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    activeSelfProjectileData.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].rotation);
                }
                if (activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    activeSelfProjectileData.damageProjectileList.Add(activeSelfProjectileData.activeProjectileList[i]);
                    activeSelfProjectileData.damageProjectileJobData.Add(activeSelfProjectileData.activeProjectileJobData[i]);
                }
                //PassTrough�������͵��ӵ��� ֻҪPass CountΪ0 ����LiftTimeΪ0 �ͻ�����
                if (activeSelfProjectileData.activeProjectileList[i].TransFixionCount <= 0 || activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended)
                {
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }

            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //����Move �� Rotation
                if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended && !activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    activeSelfProjectileData.activeProjectileList[i].Move(activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    activeSelfProjectileData.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    //Point�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    if (activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended)
                    {
                        activeSelfProjectileData.damageProjectileList.Add(activeSelfProjectileData.activeProjectileList[i]);
                        activeSelfProjectileData.damageProjectileJobData.Add(activeSelfProjectileData.activeProjectileJobData[i]);
                        activeSelfProjectileData.deathProjectileIndexList.Add(i);
                    }
                }
            }

            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Target)
            {
                //����Move �� Rotation
                if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended && !activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    activeSelfProjectileData.activeProjectileList[i].Move(activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    activeSelfProjectileData.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    if (activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        activeSelfProjectileData.damageProjectileList.Add(activeSelfProjectileData.activeProjectileList[i]);
                        activeSelfProjectileData.damageProjectileJobData.Add(activeSelfProjectileData.activeProjectileJobData[i]);
                    }
                    // Target�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }
        }
        //�����ӵ��˺�������
        HandleProjectileDamage(ref activeSelfProjectileData, ref activeTargetUnitData);
        activeSelfProjectileData.UpdateData();
        HandleProjectileDeath(ref activeSelfProjectileData);
        activeSelfProjectileData.DisposeReturnValue();
    }
    protected virtual void HandleProjectileDamage(ref ProjectileData activeSelfProjectileData, ref UnitData activeTargetUnitData)
    {

        if (activeSelfProjectileData.damageProjectileList.Count == 0)
        {
            return;
        }

        activeSelfProjectileData.rv_damageProjectileTargetIndex = new NativeArray<int>(activeSelfProjectileData.damageProjectileList.Count * activeTargetUnitData.activeTargetUnitList.Count, Allocator.TempJob);
        activeSelfProjectileData.rv_damageProjectileTargetCount = new NativeArray<int>(activeSelfProjectileData.damageProjectileList.Count, Allocator.TempJob);
        JobHandle jobHandle;

        //�ҵ������ӵ����˺�Ŀ�꣬�����ǵ������߶���� 
        //TODO ������Խ����Ż��������ӵ��Ƿ�Ϊ��Ŀ��������Job������Ŀǰ�ķ�ʽ��ͳһ���� ��Ҫ��������߼�֮��������
        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = activeSelfProjectileData.damageProjectileJobData,
            job_targesTotalCount = activeTargetUnitData.activeTargetUnitList.Count,
            job_targetsPos = activeTargetUnitData.activeTargetUnitPos,

            rv_findedTargetsCount = activeSelfProjectileData.rv_damageProjectileTargetCount,
            rv_findedTargetIndex = activeSelfProjectileData.rv_damageProjectileTargetIndex,

        };
        jobHandle = findBulletDamageTargetJob.ScheduleBatch(activeSelfProjectileData.damageProjectileList.Count, 2);
        jobHandle.Complete();


        int damagetargetindex;
        IDamageble damageble;

        //Loop���л�����˺����ӵ�List��
        for (int i = 0; i < activeSelfProjectileData.damageProjectileList.Count; i++)
        {
            //���ö�Ӧ�ӵ���prepareDamageTargetList�������ں���ʵ��Apply Damage��׼��
            for (int n = 0; n < activeSelfProjectileData.rv_damageProjectileTargetCount[i]; n++)
            {
                damagetargetindex = activeSelfProjectileData.rv_damageProjectileTargetIndex[i * activeTargetUnitData.activeTargetUnitList.Count + n];
                if (damagetargetindex < 0 || damagetargetindex >= activeTargetUnitData.activeTargetUnitList.Count)
                {
                    continue;
                }
                damageble = activeTargetUnitData.activeTargetUnitList[damagetargetindex].GetComponent<IDamageble>();
                if (damageble != null && activeSelfProjectileData.damageProjectileList[i] != null)
                {
                    if (!activeSelfProjectileData.damageProjectileList[i].prepareDamageTargetList.Contains(damageble))
                    {
                        activeSelfProjectileData.damageProjectileList[i].prepareDamageTargetList.Add(damageble);
                    }
                }
            }
            //Apply Damage to every damage target
            activeSelfProjectileData.damageProjectileList[i].ApplyDamageAllTarget();
        }

        //damageProjectile_JobInfo.Dispose();
    }

    protected virtual void HandleProjectileDeath(ref ProjectileData activeSelfProjectileData)
    {
        //���һ�� ִ���ӵ�����
        activeSelfProjectileData.deathProjectileList.Clear();
        int deathindex = 0;
        for (int i = 0; i < activeSelfProjectileData.deathProjectileIndexList.Count; i++)
        {
            deathindex = activeSelfProjectileData.deathProjectileIndexList[i];
            activeSelfProjectileData.deathProjectileList.Add(activeSelfProjectileData.deathProjectileList[deathindex]);
            //aiProjectileList[deathindex].Death(null);
        }
        for (int i = 0; i < activeSelfProjectileData.deathProjectileList.Count; i++)
        {
            activeSelfProjectileData.deathProjectileList[i].Death(null);
        }
    }

    public virtual void UpdateDrone()
    {

    }
}

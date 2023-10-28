using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;





public class JobController : IPauseable
{

    public SearchingTargetData searchingTargetData;
    public JobController()
    {
        Initialization();
    }

    ~ JobController()
    {

    }

    public virtual void Initialization()
    {
        searchingTargetData = new SearchingTargetData();

    }

    public virtual void PauseGame()
    {
        
    }

    public virtual void UnPauseGame()
    {
        
    }

    public virtual void UpdateAgentMovement(ref AgentData activeSelfAgentData, IBoidData activeTargetAgentData)
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
            job_evadeTargetPos = activeTargetAgentData.boidAgentJobData[0].position,
            job_evadeTargetVel = activeTargetAgentData.boidAgentJobData[0].velocity,

            rv_steering = activeSelfAgentData.rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob.ScheduleBatch(activeSelfAgentData.shipList.Count, 2);
        jobhandle_evadebehavior.Complete();

        ArriveBehavior.ArriveBehaviorJobs arriveBehaviorJobs = new ArriveBehavior.ArriveBehaviorJobs
        {
            job_boidData = activeSelfAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfAgentData.steeringControllerJobDataNList,

            job_targetPos = activeTargetAgentData.boidAgentJobData[0].position,
            job_targetRadius = activeTargetAgentData.boidAgentJobData[0].boidRadius,

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

            job_targetPos = activeTargetAgentData.boidAgentJobData[0].position,
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
            activeSelfAgentData.steeringControllerList[i].UpdateBoid();

            activeSelfAgentData.steeringControllerList[i].Move(activeSelfAgentData.rv_deltaMovement[i].linear);
            activeSelfAgentData.steeringControllerList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfAgentData.rv_deltaMovement[i].angular);
        }


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

        activeSelfWeaponData.rv_weaponTargetsInfo = new NativeArray<UnitTargetJobData>(targetstotalcount, Allocator.TempJob);

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindMutipleUnitTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleUnitTargetsJob
        {
            job_unitJobData = activeSelfWeaponData.activeWeaponJobData,
            job_targetsPos = activeTargetUnitData.unitPos,

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
                if (weapon.weaponState.CurrentState != WeaponState.Firing && weapon.weaponState.CurrentState != WeaponState.BetweenDelay)
                {
                    weapon.targetList.Clear();
                    for (int n = 0; n < activeSelfWeaponData.activeWeaponJobData[i].targetCount; n++)
                    {
                        targetindex = activeSelfWeaponData.rv_weaponTargetsInfo[startindex + n].targetIndex;
                        if (targetindex == -1 || activeTargetUnitData.unitList == null || activeTargetUnitData.unitList.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            weapon.targetList.Add(new UnitTargetInfo
                                (
                                    activeTargetUnitData.unitList[targetindex].gameObject,
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


    public virtual void UpdateAdditionalWeapon(ref WeaponData activeSelfWeaponData)
    {
        if (activeSelfWeaponData.activeWeaponList == null || activeSelfWeaponData.activeWeaponList.Count == 0)
        {
            return;
        }
        AdditionalWeapon weapon;

        int startindex;
        int targetindex;
        for (int i = 0; i < activeSelfWeaponData.activeWeaponList.Count; i++)
        {
            weapon = activeSelfWeaponData.activeWeaponList[i] as AdditionalWeapon;
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
        activeSelfWeaponData.UpdateData();
        activeSelfWeaponData.DisposeReturnValue();
    }

    public virtual void UpdateAdditionalBuilding(ref BuildingData activeSelfBuildingData, ref UnitData activeTargetUnitData)
    {
        if (activeSelfBuildingData.activeBuildingList == null || activeSelfBuildingData.activeBuildingList.Count == 0)
        {
            return;
        }
        Building building;

        int targetstotalcount = 0;
        for (int i = 0; i < activeSelfBuildingData.activeBuildingList.Count; i++)
        {
            targetstotalcount += activeSelfBuildingData.activeBuildingList[i].maxTargetCount;
        }

        activeSelfBuildingData.rv_buildingTargetsInfo = new NativeArray<UnitTargetJobData>(targetstotalcount, Allocator.TempJob);

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindMutipleUnitTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleUnitTargetsJob
        {
            job_unitJobData = activeSelfBuildingData.activeBuildingJobData,
            job_targetsPos = activeTargetUnitData.unitPos,

            rv_targetsInfo = activeSelfBuildingData.rv_buildingTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(activeSelfBuildingData.activeBuildingList.Count, 2);


        jobHandle.Complete();
        //ִ��Job ���ҵȴ������



        int startindex;
        int targetindex;
        for (int i = 0; i < activeSelfBuildingData.activeBuildingList.Count; i++)
        {
            //��ȡFlat Array�е�startindex Ϊ�����Ĳ����׼��
            //��ǰ��ÿһ��unit�� maxtargetscountȫ������������FlatArray�еĵ�һ��index

            if (activeSelfBuildingData.activeBuildingList[i] is Building)
            {

                startindex = 0;
                for (int c = 0; c < i; c++)
                {
                    startindex += activeSelfBuildingData.activeBuildingJobData[c].targetCount;
                }


                building = activeSelfBuildingData.activeBuildingList[i] as Building;

                if(building.buildingState.CurrentState  == BuildingState.Ready )
                {
                    building.targetList.Clear();
                    for (int n = 0; n < activeSelfBuildingData.activeBuildingJobData[i].targetCount; n++)
                    {
                        targetindex = activeSelfBuildingData.rv_buildingTargetsInfo[startindex + n].targetIndex;
                        if (targetindex == -1 || activeTargetUnitData.unitList == null || activeTargetUnitData.unitList.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            building.targetList.Add(new UnitTargetInfo
                                (
                                    activeTargetUnitData.unitList[targetindex].gameObject,
                                    activeSelfBuildingData.rv_buildingTargetsInfo[startindex + n].targetIndex,
                                    activeSelfBuildingData.rv_buildingTargetsInfo[startindex + n].distanceToTarget,
                                    activeSelfBuildingData.rv_buildingTargetsInfo[startindex + n].targetDirection
                                ));
                        }
                    }
                }
              
                
                if (building.targetList != null && building.targetList.Count != 0)
                {
                    building.BuildingOn();
                }
                else
                {
                    building.BuildingOFF();
                }

                building.ProcessBuilding();
            }
        }

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

        activeSelfProjectileData.rv_damageProjectileTargetIndex = new NativeArray<int>(activeSelfProjectileData.damageProjectileList.Count * activeTargetUnitData.unitList.Count, Allocator.TempJob);
        activeSelfProjectileData.rv_damageProjectileTargetCount = new NativeArray<int>(activeSelfProjectileData.damageProjectileList.Count, Allocator.TempJob);
        JobHandle jobHandle;

        //�ҵ������ӵ����˺�Ŀ�꣬�����ǵ������߶���� 
        //TODO ������Խ����Ż��������ӵ��Ƿ�Ϊ��Ŀ��������Job������Ŀǰ�ķ�ʽ��ͳһ���� ��Ҫ��������߼�֮��������
        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = activeSelfProjectileData.damageProjectileJobData,
            job_targesTotalCount = activeTargetUnitData.unitList.Count,
            job_targetsPos = activeTargetUnitData.unitPos,

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
                damagetargetindex = activeSelfProjectileData.rv_damageProjectileTargetIndex[i * activeTargetUnitData.unitList.Count + n];
                if (damagetargetindex < 0 || damagetargetindex >= activeTargetUnitData.unitList.Count)
                {
                    continue;
                }
                damageble = activeTargetUnitData.unitList[damagetargetindex].GetComponent<IDamageble>();
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

    public virtual void UpdateDroneAgentMovement(ref DroneData activeSelfDroneAgentData, IBoidData activeTargetAgentData)
    {
        if (activeSelfDroneAgentData.shipList == null || activeSelfDroneAgentData.shipList.Count == 0)
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

        activeSelfDroneAgentData.rv_evade_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_arrive_isVelZero = new NativeArray<bool>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_arrive_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_face_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_cohesion_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_separation_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_alignment_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        activeSelfDroneAgentData.rv_collisionavoidance_steeringInfo = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);




        EvadeBehavior.EvadeBehaviorJob_OneOnOne evadeBehaviorJob_oneonone = new EvadeBehavior.EvadeBehaviorJob_OneOnOne
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_evadeTargetBoidData = activeSelfDroneAgentData.targetBoidAgentJobData,

            rv_steering = activeSelfDroneAgentData.rv_evade_steeringInfo,

        };
        jobhandle_evadebehavior = evadeBehaviorJob_oneonone.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_evadebehavior.Complete();


        ArriveBehavior.ArriveBehaviorJobs_OneOnOne arriveBehaviorJobs_oneonone = new ArriveBehavior.ArriveBehaviorJobs_OneOnOne
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_arriveTargetBoidData = activeSelfDroneAgentData.targetBoidAgentJobData,

            rv_isVelZero = activeSelfDroneAgentData.rv_arrive_isVelZero,
            rv_Steerings = activeSelfDroneAgentData.rv_arrive_steeringInfo,

        };
        jobhandle_arrivebehavior = arriveBehaviorJobs_oneonone.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_arrivebehavior.Complete();


        ////FaceBehavior Job
        FaceBehavior.FaceBehaviorJob_OneOnOne faceBehaviorJob_oneonone = new FaceBehavior.FaceBehaviorJob_OneOnOne
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_targetBoidData = activeSelfDroneAgentData.targetBoidAgentJobData,
            job_deltatime = Time.fixedDeltaTime,

            rv_Steerings = activeSelfDroneAgentData.rv_face_steeringInfo,
        };
        jobhandle_facebehavior = faceBehaviorJob_oneonone.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_facebehavior.Complete();

        //NativeList<JobHandle> behaviorTargetsList = new NativeList<JobHandle>();

        searchingTargetData.rv_searchingTargetsVelFlatArray = new NativeArray<float3>(activeSelfDroneAgentData.shipList.Count * activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        searchingTargetData.rv_searchingTargetsPosFlatArray = new NativeArray<float3>(activeSelfDroneAgentData.shipList.Count * activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);
        searchingTargetData.rv_searchingTargetsCount = new NativeArray<int>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);


        SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob calculateSteeringTargetsPosByRadiusJob = new SteeringBehaviorController.CalculateSteeringTargetsPosByRadiusJob
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,

            job_threshold = 0.01f,


            rv_findedTargetCountPreShip = searchingTargetData.rv_searchingTargetsCount,
            rv_findedTargetVelPreShip = searchingTargetData.rv_searchingTargetsVelFlatArray,
            rv_findedTargetsPosPreShip = searchingTargetData.rv_searchingTargetsPosFlatArray,
        };
        jobhandle_behaviorTargets = calculateSteeringTargetsPosByRadiusJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_behaviorTargets.Complete();


        //Cohesion Job

        CohesionBehavior.CohesionBehaviorJob cohesionBehaviorJob = new CohesionBehavior.CohesionBehaviorJob
        {
            job_shipcount = activeSelfDroneAgentData.shipList.Count,
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,
            rv_Steerings = activeSelfDroneAgentData.rv_cohesion_steeringInfo,
        };
        jobhandle_cohesionbehavior = cohesionBehaviorJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_cohesionbehavior.Complete();

        //separation Job
        SeparationBehavior.SeparationBehaviorJob separationBehaviorJob = new SeparationBehavior.SeparationBehaviorJob
        {
            job_shipcount = activeSelfDroneAgentData.shipList.Count,
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,

            rv_Steerings = activeSelfDroneAgentData.rv_separation_steeringInfo,
        };
        jobhandle_separationBehavior = separationBehaviorJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);

        jobhandle_separationBehavior.Complete();

        //Alignment Job
        AlignmentBehavior.AlignmentBehaviorJob alignmentBehaviorJob = new AlignmentBehavior.AlignmentBehaviorJob
        {
            job_shipcount = activeSelfDroneAgentData.shipList.Count,
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_searchingTargetPosFlatArray = searchingTargetData.rv_searchingTargetsPosFlatArray,
            job_searchingTargetVelFlatArray = searchingTargetData.rv_searchingTargetsVelFlatArray,
            job_searchingTargetCount = searchingTargetData.rv_searchingTargetsCount,
            rv_Steerings = activeSelfDroneAgentData.rv_alignment_steeringInfo,
        };

        jobhandle_alignmentBehavior = alignmentBehaviorJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_alignmentBehavior.Complete();




        CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehavior.CollisionAvoidanceBehaviorJob
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,
            job_avoidenceData = activeTargetAgentData.boidAgentJobData,
            rv_steering = activeSelfDroneAgentData.rv_collisionavoidance_steeringInfo,
        };

        jobhandle_collisionAvoidanceBehavior = collisionAvoidanceBehaviorJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_collisionAvoidanceBehavior.Complete();




        activeSelfDroneAgentData.rv_deltaMovement = new NativeArray<SteeringBehaviorInfo>(activeSelfDroneAgentData.shipList.Count, Allocator.TempJob);

        JobHandle jobhandle_deltamoveposjob;
        SteeringBehaviorController.CalculateDeltaMovePosJob calculateDeltaMovePosJob = new SteeringBehaviorController.CalculateDeltaMovePosJob
        {
            job_boidData = activeSelfDroneAgentData.boidAgentJobData,
            job_steeringControllerData = activeSelfDroneAgentData.steeringControllerJobDataNList,

            job_evadeSteering = activeSelfDroneAgentData.rv_evade_steeringInfo,


            job_arriveSteering = activeSelfDroneAgentData.rv_arrive_steeringInfo,
            job_isVelZero = activeSelfDroneAgentData.rv_arrive_isVelZero,

            job_faceSteering = activeSelfDroneAgentData.rv_face_steeringInfo,


            job_alignmentSteering = activeSelfDroneAgentData.rv_alignment_steeringInfo,


            job_cohesionSteering = activeSelfDroneAgentData.rv_cohesion_steeringInfo,


            job_separationSteering = activeSelfDroneAgentData.rv_separation_steeringInfo,


            job_collisionAvoidanceSteering = activeSelfDroneAgentData.rv_collisionavoidance_steeringInfo,


            job_deltatime = Time.fixedDeltaTime,

            rv_deltainfo = activeSelfDroneAgentData.rv_deltaMovement,
        };

        jobhandle_deltamoveposjob = calculateDeltaMovePosJob.ScheduleBatch(activeSelfDroneAgentData.shipList.Count, 2);
        jobhandle_deltamoveposjob.Complete();


        for (int i = 0; i < activeSelfDroneAgentData.shipList.Count; i++)
        {
            activeSelfDroneAgentData.steeringControllerList[i].UpdateBoid();

            activeSelfDroneAgentData.steeringControllerList[i].Move(activeSelfDroneAgentData.rv_deltaMovement[i].linear);
            activeSelfDroneAgentData.steeringControllerList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfDroneAgentData.rv_deltaMovement[i].angular);
        }


        activeSelfDroneAgentData.DisposeReturnValue();
        searchingTargetData.DisposeReturnValue();
    }
}

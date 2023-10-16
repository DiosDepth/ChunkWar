using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;





public class JobController : IPauseable
{
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

        // 如果Process 则开启索敌Job， 并且蒋索敌结果记录在targetsindex[]中
        //这个Job返回一个JRD_targetsInfo 这个是已经排序的数据， 包含 index， Pos， direction， distance 几部分数据


        Weapon.FindMutipleWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleWeaponTargetsJob
        {
            job_weaponJobData = activeSelfWeaponData.activeWeaponJobData,
            job_targetsPos = activeTargetUnitData.activeTargetUnitPos,

            rv_targetsInfo = activeSelfWeaponData.rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(activeSelfWeaponData.activeWeaponList.Count, 2);


        jobHandle.Complete();
        //执行Job 并且等待结果。



        int startindex;
        int targetindex;
        for (int i = 0; i < activeSelfWeaponData.activeWeaponList.Count; i++)
        {
            //获取Flat Array中的startindex 为后续的拆分做准备
            //吧前面每一个unit的 maxtargetscount全部加起来就是FlatArray中的第一个index

            if (activeSelfWeaponData.activeWeaponList[i] is AdditionalWeapon)
            {

                startindex = 0;
                for (int c = 0; c < i; c++)
                {
                    startindex += activeSelfWeaponData.activeWeaponJobData[c].targetCount;
                }


                weapon = activeSelfWeaponData.activeWeaponList[i] as AdditionalWeapon;

                //如果没有在开火或者在开火间歇中，则重新刷写weapon.targetlist
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
        // 获取子弹分类
        // 获取子弹的Target，并且更新Target位置， 如果是
        JobHandle aibulletjobhandle;
        //根据子弹分类比如是否targetbase来开启对应的JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = activeSelfProjectileData.activeProjectileJobData,
            rv_bulletJobUpdateInfos = activeSelfProjectileData.rv_activeProjectileUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(activeSelfProjectileData.activeProjectileList.Count, 2);
        aibulletjobhandle.Complete();
        //更新子弹当前的JobData


        //创建子弹的伤害List 和对应的死亡List
        activeSelfProjectileData.deathProjectileIndexList.Clear();
        activeSelfProjectileData.damageProjectileList.Clear();
        activeSelfProjectileData.damageProjectileJobData.Clear();

        //Loop 所有的子弹并且创建他们的伤害和死亡List，注意伤害和死亡的List并不是一一对应的。 有些子弹在产生伤害之后并不会死亡，比如穿透子弹
        for (int i = 0; i < activeSelfProjectileData.activeProjectileList.Count; i++)
        {
            //移动子弹
            //处理子弹旋转方向
            if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended)
            {
                activeSelfProjectileData.activeProjectileList[i].UpdateBullet();
            }
            //子弹的移动，伤害，销毁列表维护需要区分四个不同触发类型
            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //处理Move 和 Rotation
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
                    //Collide触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }

            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough 类型的子弹， 之后生命周期末尾
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
                //PassTrough触发类型的子弹， 只要Pass Count为0 或者LiftTime为0 就会死亡
                if (activeSelfProjectileData.activeProjectileList[i].TransFixionCount <= 0 || activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended)
                {
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }

            if (activeSelfProjectileData.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //处理Move 和 Rotation
                if (!activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].islifeended && !activeSelfProjectileData.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    activeSelfProjectileData.activeProjectileList[i].Move(activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    activeSelfProjectileData.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, activeSelfProjectileData.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    //Point触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
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
                //处理Move 和 Rotation
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
                    // Target触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
                    activeSelfProjectileData.deathProjectileIndexList.Add(i);
                }
            }
        }
        //处理子弹伤害和死亡
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

        //找到所有子弹的伤害目标，可能是单个或者多个。 
        //TODO 这里可以进行优化，根据子弹是否为多目标来进行Job，不过目前的方式是统一处理， 需要完成所有逻辑之后抽象出来
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

        //Loop所有会产生伤害的子弹List，
        for (int i = 0; i < activeSelfProjectileData.damageProjectileList.Count; i++)
        {
            //设置对应子弹的prepareDamageTargetList，用来在后面实际Apply Damage做准备
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
        //最后一步 执行子弹死亡
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

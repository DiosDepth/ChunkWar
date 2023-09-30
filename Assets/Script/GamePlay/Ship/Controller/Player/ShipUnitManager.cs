using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using static Weapon;

public class ShipUnitManager 
{
    public BaseShip targetShip;
    public List<ShipWeapon> activeWeaponList;
    public List<Building> activeBuildingList;
    public List<Projectile> projectileList = new List<Projectile>();
    private List<int> projiectileDeathIndexList = new List<int>();
    private List<Projectile> projectileDamageList = new List<Projectile>();
    NativeList<ProjectileJobInitialInfo> damageProjectile_JobInfo;
    public NativeArray<int> rv_projectileDamageTargetIndex;
    public NativeArray<int> rv_projectileDamageTargetCountPre;

    public NativeList<ProjectileJobInitialInfo> projectile_JobInfo;
    public NativeArray<ProjectileJobRetrunInfo> rv_projectile_jobUpdateInfo;

    public NativeList<float> activeWeaponAttackRangeList;
    public NativeList<float3> activeWeaponPosList;
    public NativeList<int> activeWeaponTargetCountList;
    public NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;

    public ShipUnitManager()
    {
        activeWeaponList = new List<ShipWeapon>();
        activeBuildingList = new List<Building>();
        projectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);
        damageProjectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);

        activeWeaponPosList = new NativeList<float3>(Allocator.Persistent);
        activeWeaponAttackRangeList = new NativeList<float>(Allocator.Persistent);
        activeWeaponTargetCountList = new NativeList<int>(Allocator.Persistent);
   
    }

    ~ ShipUnitManager()
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (projectile_JobInfo.IsCreated) { projectile_JobInfo.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
        if (damageProjectile_JobInfo.IsCreated) { damageProjectile_JobInfo.Dispose(); }
    }

    public virtual void Initialization(BaseShip target)
    {
        targetShip = target;
    }

    public virtual void Update()
    {
        UpdateActiveUnit();
        UpdateWeapon();
        UpdateBuilding();
        UpdateBullet();
        UpdateProjectileDamage();
    }



    //Only other weapon but main weapon
    protected virtual void UpdateWeapon()
    {
        if(activeWeaponList.Count <= 0)
        {
            return;
        }
        int targetstotalcount = 0;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            targetstotalcount += activeWeaponList[i].maxTargetCount;
        }

        rv_weaponTargetsInfo = new NativeArray<Weapon.RV_WeaponTargetInfo>(targetstotalcount, Allocator.TempJob);

        // 如果Process 则开启索敌Job， 并且蒋索敌结果记录在targetsindex[]中
        //这个Job返回一个JRD_targetsInfo 这个是已经排序的数据， 包含 index， Pos， direction， distance 几部分数据


        Weapon.FindMutipleWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleWeaponTargetsJob
        {
            job_attackRange = activeWeaponAttackRangeList,
            job_selfPos = activeWeaponPosList,
            job_targetsPos = AIManager.Instance.aiActiveUnitPos,
            job_maxTargetCount = activeWeaponTargetCountList,

            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(activeWeaponList.Count, 2);
        jobHandle.Complete();


        int startindex;
        int targetindex;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            //获取Flat Array中的startindex 为后续的拆分做准备
            //吧前面每一个unit的 maxtargetscount全部加起来就是FlatArray中的第一个index
            startindex = 0;
            for (int c = 0; c < i; c++)
            {
                startindex += activeWeaponTargetCountList[i];
            }


            ShipWeapon weapon = activeWeaponList[i];
            weapon.targetList.Clear();
            for (int n = 0; n < activeWeaponTargetCountList[i]; n++)
            {
                targetindex = rv_weaponTargetsInfo[startindex + n].targetIndex;
                if (targetindex == -1)
                {
                    weapon.WeaponOff();
                    break;
                }
                else
                {
                    if (AIManager.Instance.aiActiveUnitList == null || AIManager.Instance.aiActiveUnitList.Count == 0)
                    {
                        weapon.WeaponOff();
                        break;
                    }
                    weapon.targetList.Add(new WeaponTargetInfo
                        (
                            AIManager.Instance.aiActiveUnitList[targetindex].gameObject,
                            rv_weaponTargetsInfo[startindex + n].targetIndex,
                            rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                            rv_weaponTargetsInfo[startindex + n].targetDirection
                        ));
                }
            }
            if (weapon.targetList != null && weapon.targetList.Count != 0)
            {
                weapon.WeaponOn();
            }
            weapon.ProcessWeapon();
        }


        rv_weaponTargetsInfo.Dispose();
    }

    protected virtual void UpdateBuilding()
    {

    }

    protected virtual void UpdateBullet()
    {
        if (projectileList == null || projectileList.Count == 0)
        {
            return;
        }
        rv_projectile_jobUpdateInfo = new NativeArray<ProjectileJobRetrunInfo>(projectileList.Count, Allocator.TempJob);
        // 获取子弹分类
        // 获取子弹的Target，并且更新Target位置， 如果是
        JobHandle aibulletjobhandle;
        //根据子弹分类比如是否targetbase来开启对应的JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = projectile_JobInfo,
            rv_bulletJobUpdateInfos = rv_projectile_jobUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(projectileList.Count, 2);
        aibulletjobhandle.Complete();
        //更新子弹当前的JobData

        //移动子弹
        //处理子弹旋转方向
        projiectileDeathIndexList.Clear();

        for (int i = 0; i < projectileList.Count; i++)
        {
            if (!rv_projectile_jobUpdateInfo[i].islifeended)
            {
                projectileList[i].Move(rv_projectile_jobUpdateInfo[i].deltaMovement);
                projectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_projectile_jobUpdateInfo[i].rotation);
            }
            else
            {
                projiectileDeathIndexList.Add(i);
            }

        }

        UpdateBulletJobData();

        rv_projectile_jobUpdateInfo.Dispose();


    }

    public void UpdateBulletJobData()
    {
        //这里需要先更新子弹的信息，然后在吧死亡的子弹移除， 否则rv aibullet的静态数据长度无法操作
        //虽然会浪费运算量。但是可以保证index不会错位
        ProjectileJobInitialInfo bulletJobInfo;
        for (int i = 0; i < projectileList.Count; i++)
        {
            if (rv_projectile_jobUpdateInfo[i].islifeended)
                continue;

            bulletJobInfo = projectile_JobInfo[i];

            bulletJobInfo.update_targetPos = projectileList[i].target ? projectileList[i].target.transform.position : projectileList[i].transform.up;
            bulletJobInfo.update_selfPos = projectileList[i].transform.position;
            bulletJobInfo.update_lifeTimeRemain = rv_projectile_jobUpdateInfo[i].lifeTimeRemain;
            bulletJobInfo.update_moveDirection = rv_projectile_jobUpdateInfo[i].moveDirection;
            bulletJobInfo.update_currentSpeed = rv_projectile_jobUpdateInfo[i].currentSpeed;

            projectile_JobInfo[i] = bulletJobInfo;
        }

        //最后一步 执行子弹死亡
        int deathindex = 0;
        for (int i = 0; i < projiectileDeathIndexList.Count; i++)
        {
            deathindex = projiectileDeathIndexList[i];
            projectileList[deathindex].Death(null);
        }
    }

    public void UpdateProjectileDamage()
    {
        //处理所有子弹的伤害逻辑
        projectileDamageList.Clear();

        damageProjectile_JobInfo.Clear();
 

        for (int i = 0; i < projectileList.Count; i++)
        {
            if (projectileList[i].IsApplyDamageAtThisFrame == true)
            {
                projectileDamageList.Add(projectileList[i]);
                damageProjectile_JobInfo.Add(projectile_JobInfo[i]);
            }
        }
        //projectileDamageList = projectileList.FindAll(x => x.IsApplyDamageAtThisFrame);

        if (projectileDamageList.Count == 0)
        {
            return;
        }


        rv_projectileDamageTargetIndex = new NativeArray<int>(projectileDamageList.Count * AIManager.Instance.aiActiveUnitList.Count, Allocator.TempJob);
        rv_projectileDamageTargetCountPre = new NativeArray<int>(projectileDamageList.Count, Allocator.TempJob);
        JobHandle jobHandle;

        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = damageProjectile_JobInfo,
            job_targesTotalCount = AIManager.Instance.aiActiveUnitList.Count,
            job_targetsPos = AIManager.Instance.aiActiveUnitPos,

            rv_findedTargetsCount = rv_projectileDamageTargetCountPre,
            rv_findedTargetIndex = rv_projectileDamageTargetIndex,

        };

        jobHandle = findBulletDamageTargetJob.ScheduleBatch(projectileDamageList.Count, 2);
        jobHandle.Complete();


        int damagetargetindex;
        IDamageble damageble;

        for (int i = 0; i < projectileDamageList.Count; i++)
        {
            for (int n = 0; n < rv_projectileDamageTargetCountPre[i]; n++)
            {
                damagetargetindex = rv_projectileDamageTargetIndex[i * AIManager.Instance.aiActiveUnitList.Count + n];
                damageble = AIManager.Instance.aiActiveUnitList[damagetargetindex].GetComponent<IDamageble>();
                projectileDamageList[i].ApplyDamage(damageble);
            }
        }

        //damageProjectile_JobInfo.Dispose();
        rv_projectileDamageTargetCountPre.Dispose();
        rv_projectileDamageTargetIndex.Dispose();
    }


    public virtual void AddActiveUnit(Unit unit)
    {
        if(unit is ShipWeapon)
        {
            ShipWeapon weapon = unit as ShipWeapon;
            if(weapon.WeaponCfg.unitType == UnitType.MainWeapons)
            {
                return;
            }
            if(activeWeaponList.Contains(weapon))
            {
                return;
            }
            activeWeaponList.Add(weapon);
            activeWeaponAttackRangeList.Add(weapon.baseAttribute.WeaponRange);
            activeWeaponPosList.Add(weapon.transform.position);
            activeWeaponTargetCountList.Add(weapon.maxTargetCount);
        }
        
        if(unit is Building)
        {
            Building building = unit as Building;
            if(activeBuildingList.Contains(building))
            {
                return;
            }
            activeBuildingList.Add(building);
        }
    }

    public virtual void RemoveActiveUnit(Unit unit)
    {
        if( unit is ShipWeapon)
        {
            ShipWeapon weapon = unit as ShipWeapon;
            int index = activeWeaponList.IndexOf(weapon);
            activeWeaponList.RemoveAt(index);
            activeWeaponPosList.RemoveAt(index);
            activeWeaponAttackRangeList.RemoveAt(index);
            activeWeaponTargetCountList.RemoveAt(index);
        }

        if(unit is Building)
        {
            Building building = unit as Building;
            int index = activeBuildingList.IndexOf(building);

            activeBuildingList.RemoveAt(index);
        }
      
    }
    public virtual void UpdateActiveUnit()
    {
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            activeWeaponPosList[i] = activeWeaponList[i].transform.position;
        }
    }
    protected virtual List<T> FindAllActiveUnit<T>(List<T> list) where T : Unit
    {
        return list.FindAll(x => x != null && x.isActiveAndEnabled).ConvertAll(x => x as T);
    }



    public void AddBullet(Bullet bullet)
    {
        if (bullet is Projectile)
        {
            AddProjectileBullet(bullet as Projectile);
        }
    }

    public void AddProjectileBullet(Projectile bullet)
    {
        if (!projectileList.Contains(bullet))
        {
            projectileList.Add(bullet);
            projectile_JobInfo.Add(new ProjectileJobInitialInfo
                (
                    bullet.target ? bullet.target.transform.position : bullet.transform.up,
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
        int index = projectileList.IndexOf(bullet);
        projectile_JobInfo.RemoveAt(index);
        projectileList.RemoveAt(index);
    }
    public void RemoveBullet(Bullet bullet)
    {
        if (bullet is Projectile)
        {
            RemoveProjectileBullet(bullet as Projectile);
        }
        //todo 如果不是Projectile类型需要用调用其他容器的Remove
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;




public class TargetUnitData : IJobDisposeable
{

    public List<Unit> activeTargetUnitList;
    public NativeList<float3> activeTargetUnitPos;

    public TargetUnitData()
    {
        activeTargetUnitList = new List<Unit>();
        activeTargetUnitPos = new NativeList<float3>(Allocator.Persistent);
    }

    public void  Dispose()
    {
        if (activeTargetUnitPos.IsCreated) { activeTargetUnitPos.Dispose(); }
    }

    public void DisposeReturnValue()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateData()
    {
        if(activeTargetUnitList.Count  == 0) { return; }
        for (int i = 0; i < activeTargetUnitList.Count; i++)
        {
            activeTargetUnitPos[i] = activeTargetUnitList[i].transform.position;
        }
    }
}

public class AvoidenceCollisionData : IJobDisposeable
{
    public NativeList<AvoidenceCollisionJobData> avoidenceCollisionList;

    public AvoidenceCollisionData()
    {
        
    }
    public void Dispose()
    {
        
    }

    public void DisposeReturnValue()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateData()
    {
       
    }
}

public struct AvoidenceCollisionJobData
{
    public float3 avoidanceCollisionPos;
    public float avoidanceCollisionRadius;
    public float3 avoidanceCollisionVel;
}



public class WeaponData : IJobDisposeable
{

    public List<ShipAdditionalWeapon> activeWeaponList;
    public NativeList<WeaponJobData> activeWeaponJobData;
   
    public WeaponData ()
    {
        activeWeaponList = new List<ShipAdditionalWeapon>();
        activeWeaponJobData = new NativeList<WeaponJobData>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (activeWeaponJobData.IsCreated) { activeWeaponJobData.Dispose(); }
    }

    public void DisposeReturnValue()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateData()
    {
       if(activeWeaponList.Count == 0) { return; }
        WeaponJobData tempweaponjobdata;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            tempweaponjobdata.attackRange = activeWeaponList[i].weaponAttribute.WeaponRange;
            tempweaponjobdata.position = activeWeaponList[i].transform.position;
            tempweaponjobdata.targetCount = activeWeaponList[i].maxTargetCount;
            activeWeaponJobData[i] = tempweaponjobdata;
        }
    }
}

public class BuildingData : IJobDisposeable
{
    public List<Building> activeBuildingList;
    public NativeList<BuildingJobData> activeBuildingJobData;

    public BuildingData ()
    {
        activeBuildingList = new List<Building>();
        activeBuildingJobData = new NativeList<BuildingJobData>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (activeBuildingJobData.IsCreated) { activeBuildingJobData.Dispose(); }
    }

    public void DisposeReturnValue()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateData()
    {
        if (activeBuildingList.Count == 0) { return; }
        BuildingJobData buildingJobData;
        for (int i = 0; i < activeBuildingList.Count; i++)
        {
            buildingJobData.attackRange = activeBuildingList[i].buildingAttribute.WeaponRange;
            buildingJobData.position = activeBuildingList[i].transform.position;
            buildingJobData.targetCount = activeBuildingList[i].maxTargetCount;
            activeBuildingJobData[i] = buildingJobData;
        }
    }
}


public struct WeaponJobData
{
    public float attackRange;
    public float3 position;
    public int targetCount;
}

public struct BuildingJobData
{
    public float attackRange;
    public float3 position;
    public int targetCount;

}


public class ProjectileData : IJobDisposeable
{
    public List<Projectile> activeProjectileList;
    public NativeList<ProjectileJobInitialInfo> activeProjectileJobData;
    public List<Projectile> damageProjectileList;
    public List<int> deathProjectileIndexList;
    public List<Projectile> deathProjectileList;
    public NativeList<ProjectileJobInitialInfo> damageProjectileJobData;

    //这是一个FlatArray 里面记录了每个damageProjectile的TargetIndex 按照顺序全部记录
    public NativeArray<int> rv_damageProjectileTargetIndex;
    //每一个damgeProjectile搜索到的targetCount；
    public NativeArray<int> rv_damageProjectileTargetCount;
    public NativeArray<ProjectileJobRetrunInfo> rv_activeProjectileUpdateInfo;

    public ProjectileData()
    {
        activeProjectileList = new List<Projectile>();
        activeProjectileJobData = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);
        damageProjectileList = new List<Projectile>();
        deathProjectileIndexList = new List<int>();
        deathProjectileList = new List<Projectile>();
        damageProjectileJobData = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);


    }
    public void Dispose()
    {
        if (activeProjectileJobData.IsCreated) { activeProjectileJobData.Dispose(); }
        if (damageProjectileJobData.IsCreated) { damageProjectileJobData.Dispose(); }
    }

    public void DisposeReturnValue()
    {
        if (rv_damageProjectileTargetIndex.IsCreated) { rv_damageProjectileTargetIndex.Dispose(); }
        if (rv_damageProjectileTargetCount.IsCreated) { rv_damageProjectileTargetCount.Dispose(); }
        if (rv_activeProjectileUpdateInfo.IsCreated) { rv_activeProjectileUpdateInfo.Dispose(); }
    }

    public void UpdateData()
    {
        ProjectileJobInitialInfo bulletJobInfo;
        for (int i = 0; i < activeProjectileList.Count; i++)
        {
            if (rv_activeProjectileUpdateInfo[i].islifeended)
                continue;

            bulletJobInfo = activeProjectileJobData[i];

            bulletJobInfo.update_targetPos = activeProjectileList[i].initialTarget ? activeProjectileList[i].initialTarget.transform.position : activeProjectileList[i].transform.up;
            bulletJobInfo.update_selfPos = activeProjectileList[i].transform.position;
            bulletJobInfo.update_lifeTimeRemain = rv_activeProjectileUpdateInfo[i].lifeTimeRemain;
            bulletJobInfo.update_moveDirection = rv_activeProjectileUpdateInfo[i].moveDirection;
            bulletJobInfo.update_currentSpeed = rv_activeProjectileUpdateInfo[i].currentSpeed;

            activeProjectileJobData[i] = bulletJobInfo;
        }
        DisposeReturnValue();
    }
}
public struct DroneData
{

}


public class JobManager: IPauseable
{
    public List<IBoid> targetIBoidList;
    public List<BaseShip> targetShipList;
    public List<BaseShip> activeShipList;

    public TargetUnitData activeTargetUnitData;
    public WeaponData activeWeaponData;
    public NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;

    public BuildingData activeBuildingData;

    public ProjectileData activeProjectileData; 






    public virtual void Initialization()
    {
        targetIBoidList = new List<IBoid>();
        targetShipList = new List<BaseShip>();
        activeShipList = new List<BaseShip>();

    }



    public virtual void DisposeJobData()
    {

    }
    public virtual void UnLoad()
    {

    }

    public virtual void GameOver()
    {

    }

    protected virtual void Update()
    {
        UpdateProjectile(ref activeProjectileData);
    }

    protected virtual void LaterUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    public virtual void PauseGame()
    {
        
    }

    public virtual void UnPauseGame()
    {
        
    }

    public virtual void UpdateJobData()
    {
        //更新自身的Unit相关data,
        activeWeaponData.UpdateData();
        activeBuildingData.UpdateData();
        activeTargetUnitData.UpdateData();
        activeProjectileData.UpdateData();

    }


    public virtual void  UpdateWeapon()
    {

    }
    public virtual void UpdateBuilding()
    {

    }

    public virtual void UpdateProjectile(ref ProjectileData data)
    {
        if (data.activeProjectileList == null || data.activeProjectileList.Count == 0)
        {
            return;
        }
        data.rv_activeProjectileUpdateInfo = new NativeArray<ProjectileJobRetrunInfo>(data.activeProjectileList.Count, Allocator.TempJob);
        // 获取子弹分类
        // 获取子弹的Target，并且更新Target位置， 如果是
        JobHandle aibulletjobhandle;
        //根据子弹分类比如是否targetbase来开启对应的JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = data.activeProjectileJobData,
            rv_bulletJobUpdateInfos = data.rv_activeProjectileUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(data.activeProjectileList.Count, 2);
        aibulletjobhandle.Complete();
        //更新子弹当前的JobData


        //创建子弹的伤害List 和对应的死亡List
        data.deathProjectileIndexList.Clear();
        data.damageProjectileList.Clear();
        data.damageProjectileJobData.Clear();

        //Loop 所有的子弹并且创建他们的伤害和死亡List，注意伤害和死亡的List并不是一一对应的。 有些子弹在产生伤害之后并不会死亡，比如穿透子弹
        for (int i = 0; i < data.activeProjectileList.Count; i++)
        {
            //移动子弹
            //处理子弹旋转方向
            if (!data.rv_activeProjectileUpdateInfo[i].islifeended)
            {
                data.activeProjectileList[i].UpdateBullet();
            }
            //子弹的移动，伤害，销毁列表维护需要区分四个不同触发类型
            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //处理Move 和 Rotation
                if (!data.rv_activeProjectileUpdateInfo[i].islifeended && !data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    data.activeProjectileList[i].Move(data.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    data.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, data.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    if (data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        data.damageProjectileList.Add(data.activeProjectileList[i]);
                        data.damageProjectileJobData.Add(data.activeProjectileJobData[i]);
                    }
                    //Collide触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
                    data.deathProjectileIndexList.Add(i);
                }
            }

            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough 类型的子弹， 之后生命周期末尾
                if (!data.rv_activeProjectileUpdateInfo[i].islifeended && data.activeProjectileList[i].TransFixionCount > 0)
                {
                    data.activeProjectileList[i].Move(data.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    data.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, data.rv_activeProjectileUpdateInfo[i].rotation);
                }
                if (data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    data.damageProjectileList.Add(data.activeProjectileList[i]);
                    data.damageProjectileJobData.Add(data.activeProjectileJobData[i]);
                }
                //PassTrough触发类型的子弹， 只要Pass Count为0 或者LiftTime为0 就会死亡
                if (data.activeProjectileList[i].TransFixionCount <= 0 || data.rv_activeProjectileUpdateInfo[i].islifeended)
                {
                    data.deathProjectileIndexList.Add(i);
                }
            }

            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //处理Move 和 Rotation
                if (!data.rv_activeProjectileUpdateInfo[i].islifeended && !data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    data.activeProjectileList[i].Move(data.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    data.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, data.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    //Point触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
                    if (data.rv_activeProjectileUpdateInfo[i].islifeended)
                    {
                        data.damageProjectileList.Add(data.activeProjectileList[i]);
                        data.damageProjectileJobData.Add(data.activeProjectileJobData[i]);
                        data.deathProjectileIndexList.Add(i);
                    }
                }
            }

            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Target)
            {
                //处理Move 和 Rotation
                if (!data.rv_activeProjectileUpdateInfo[i].islifeended && !data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    data.activeProjectileList[i].Move(data.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    data.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, data.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    if (data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                    {
                        data.damageProjectileList.Add(data.activeProjectileList[i]);
                        data.damageProjectileJobData.Add(data.activeProjectileJobData[i]);
                    }
                    // Target触发类型的子弹， 只要LifeTime或者 Damage有一个触发， 就会执行Death
                    data.deathProjectileIndexList.Add(i);
                }
            }
        }
        //处理子弹伤害
        HandleProjectileDamage();
        //处理子弹死亡
        HandleProjectileDeath();




    }
    public virtual void HandleProjectileDamage()
    {

    }

    public virtual void HandleProjectileDeath()
    {

    }

    public virtual void UpdateDrone()
    {

    }
}

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

    //����һ��FlatArray �����¼��ÿ��damageProjectile��TargetIndex ����˳��ȫ����¼
    public NativeArray<int> rv_damageProjectileTargetIndex;
    //ÿһ��damgeProjectile��������targetCount��
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
        //���������Unit���data,
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
        // ��ȡ�ӵ�����
        // ��ȡ�ӵ���Target�����Ҹ���Targetλ�ã� �����
        JobHandle aibulletjobhandle;
        //�����ӵ���������Ƿ�targetbase��������Ӧ��JOB
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = data.activeProjectileJobData,
            rv_bulletJobUpdateInfos = data.rv_activeProjectileUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(data.activeProjectileList.Count, 2);
        aibulletjobhandle.Complete();
        //�����ӵ���ǰ��JobData


        //�����ӵ����˺�List �Ͷ�Ӧ������List
        data.deathProjectileIndexList.Clear();
        data.damageProjectileList.Clear();
        data.damageProjectileJobData.Clear();

        //Loop ���е��ӵ����Ҵ������ǵ��˺�������List��ע���˺���������List������һһ��Ӧ�ġ� ��Щ�ӵ��ڲ����˺�֮�󲢲������������紩͸�ӵ�
        for (int i = 0; i < data.activeProjectileList.Count; i++)
        {
            //�ƶ��ӵ�
            //�����ӵ���ת����
            if (!data.rv_activeProjectileUpdateInfo[i].islifeended)
            {
                data.activeProjectileList[i].UpdateBullet();
            }
            //�ӵ����ƶ����˺��������б�ά����Ҫ�����ĸ���ͬ��������
            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //����Move �� Rotation
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
                    //Collide�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    data.deathProjectileIndexList.Add(i);
                }
            }

            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough ���͵��ӵ��� ֮����������ĩβ
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
                //PassTrough�������͵��ӵ��� ֻҪPass CountΪ0 ����LiftTimeΪ0 �ͻ�����
                if (data.activeProjectileList[i].TransFixionCount <= 0 || data.rv_activeProjectileUpdateInfo[i].islifeended)
                {
                    data.deathProjectileIndexList.Add(i);
                }
            }

            if (data.activeProjectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //����Move �� Rotation
                if (!data.rv_activeProjectileUpdateInfo[i].islifeended && !data.activeProjectileList[i].IsApplyDamageAtThisFrame)
                {
                    data.activeProjectileList[i].Move(data.rv_activeProjectileUpdateInfo[i].deltaMovement);
                    data.activeProjectileList[i].transform.rotation = Quaternion.Euler(0, 0, data.rv_activeProjectileUpdateInfo[i].rotation);
                }
                else
                {
                    //Point�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
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
                //����Move �� Rotation
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
                    // Target�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    data.deathProjectileIndexList.Add(i);
                }
            }
        }
        //�����ӵ��˺�
        HandleProjectileDamage();
        //�����ӵ�����
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

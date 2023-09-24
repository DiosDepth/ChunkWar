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

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindWeaponTargetsJob
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
            //��ȡFlat Array�е�startindex Ϊ�����Ĳ����׼��
            //��ǰ��ÿһ��unit�� maxtargetscountȫ������������FlatArray�еĵ�һ��index
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
        // ��ȡ�ӵ�����
        // ��ȡ�ӵ���Target�����Ҹ���Targetλ�ã� �����
        JobHandle aibulletjobhandle;
        //�����ӵ���������Ƿ�targetbase��������Ӧ��JOB
        Projectile.CalculateBulletMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateBulletMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = projectile_JobInfo,
            rv_bulletJobUpdateInfos = rv_projectile_jobUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(projectileList.Count, 2);
        aibulletjobhandle.Complete();
        //�����ӵ���ǰ��JobData

        //�ƶ��ӵ�
        //�����ӵ���ת����
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
        //������Ҫ�ȸ����ӵ�����Ϣ��Ȼ���ڰ��������ӵ��Ƴ��� ����rv aibullet�ľ�̬���ݳ����޷�����
        //��Ȼ���˷������������ǿ��Ա�֤index�����λ
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

        //���һ�� ִ���ӵ�����
        int deathindex = 0;
        for (int i = 0; i < projiectileDeathIndexList.Count; i++)
        {
            deathindex = projiectileDeathIndexList[i];
            projectileList[deathindex].Death();
        }
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
                   (int)bullet.movementType
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
        //todo �������Projectile������Ҫ�õ�������������Remove
    }
}
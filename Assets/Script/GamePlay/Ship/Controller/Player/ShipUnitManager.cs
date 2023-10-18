
using System.Collections.Generic;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using static Weapon;

public class ShipUnitManager:IPauseable
{
    /*
    public bool ProcessShip;
    public BaseShip targetShip;
    public List<ShipAdditionalWeapon> activeWeaponList;
    public List<Building> activeBuildingList;
    public List<Projectile> projectileList = new List<Projectile>();
    private List<int> _projectileDeathIndexList = new List<int>();
    private List<Projectile> _ProjectileDeathList = new List<Projectile>();
    private List<Projectile> _projectileDamageList = new List<Projectile>();
    NativeList<ProjectileJobInitialInfo> damageProjectile_JobInfo;
    public NativeArray<int> rv_projectileDamageTargetIndex;
    public NativeArray<int> rv_projectileDamageTargetCountPre;

    public NativeList<ProjectileJobInitialInfo> projectile_JobInfo;
    public NativeArray<ProjectileJobRetrunInfo> rv_projectile_jobUpdateInfo;

    public NativeList<float> activeWeaponAttackRangeList;
    public NativeList<float3> activeWeaponPosList;
    public NativeList<int> activeWeaponTargetCountList;
    public NativeArray<Weapon.WeaponTargetJobData> rv_weaponTargetsInfo;

    public ShipUnitManager()
    {
        GameManager.Instance.RegisterPauseable(this);
        activeWeaponList = new List<ShipAdditionalWeapon>();
        activeBuildingList = new List<Building>();
        projectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);
        damageProjectile_JobInfo = new NativeList<ProjectileJobInitialInfo>(Allocator.Persistent);

        activeWeaponPosList = new NativeList<float3>(Allocator.Persistent);
        activeWeaponAttackRangeList = new NativeList<float>(Allocator.Persistent);
        activeWeaponTargetCountList = new NativeList<int>(Allocator.Persistent);
        
    }

    ~ ShipUnitManager()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        DisposeAllJobData();
    }

    public virtual void DisposeAllJobData()
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (projectile_JobInfo.IsCreated) { projectile_JobInfo.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
        if (damageProjectile_JobInfo.IsCreated) { damageProjectile_JobInfo.Dispose(); }
    }

    public virtual void Initialization(BaseShip target)
    {
        GameManager.Instance.RegisterPauseable(this);
        targetShip = target;
        ProcessShip = true;
    }

    public virtual void Update()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessShip) { return; }
        UpdateActiveUnit();
        UpdateWeapon();
        UpdateBuilding();
        UpdateProjectile();

        //UpdateDrone();

        //UpdatePluge();
    }


    public void GameOver()
    {
        ProcessShip = false;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            activeWeaponList[i].GameOver();
        }
        for (int i = 0; i < activeBuildingList.Count; i++)
        {
            activeBuildingList[i].GameOver();
        }
        for (int i = 0; i < projectileList.Count; i++)
        {
            projectileList[i].GameOver();
        }
        Stop();
        UnLoad();
    }

    public void Stop()
    {
        ProcessShip = false;


    }

    public void UnLoad()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        activeWeaponList?.Clear();
        activeBuildingList?.Clear();
        projectileList?.Clear();
        _projectileDamageList?.Clear();
        _projectileDeathIndexList?.Clear();

        DisposeAllJobData();
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

        rv_weaponTargetsInfo = new NativeArray<Weapon.WeaponTargetJobData>(targetstotalcount, Allocator.TempJob);

        // ���Process ��������Job�� ���ҽ����н����¼��targetsindex[]��
        //���Job����һ��JRD_targetsInfo ������Ѿ���������ݣ� ���� index�� Pos�� direction�� distance ����������


        Weapon.FindMutipleWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindMutipleWeaponTargetsJob
        {
            //job_attackRange = activeWeaponAttackRangeList,
            //job_selfPos = activeWeaponPosList,
            //job_targetsPos = AIManager.Instance.position,
            //job_maxTargetCount = activeWeaponTargetCountList,

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
                startindex += activeWeaponTargetCountList[c];
            }


            ShipAdditionalWeapon weapon = activeWeaponList[i];
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
                    if (AIManager.Instance.activeWeaponList == null || AIManager.Instance.activeWeaponList.Count == 0)
                    {
                        weapon.WeaponOff();
                        break;
                    }
                    weapon.targetList.Add(new WeaponTargetInfo
                        (
                            AIManager.Instance.activeWeaponList[targetindex].gameObject,
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

    protected virtual void UpdateProjectile()
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
        Projectile.CalculateProjectileMovementJobJob staightCalculateBulletMovementJobJob = new Projectile.CalculateProjectileMovementJobJob
        {
            job_deltatime = Time.deltaTime,
            job_jobInfo = projectile_JobInfo,
            rv_bulletJobUpdateInfos = rv_projectile_jobUpdateInfo,
        };

        aibulletjobhandle = staightCalculateBulletMovementJobJob.ScheduleBatch(projectileList.Count, 2);
        aibulletjobhandle.Complete();
        //�����ӵ���ǰ��JobData


        //�����ӵ����˺�List �Ͷ�Ӧ������List
        _projectileDeathIndexList.Clear();
        _projectileDamageList.Clear();
        damageProjectile_JobInfo.Clear();

        //Loop ���е��ӵ����Ҵ������ǵ��˺�������List��ע���˺���������List������һһ��Ӧ�ġ� ��Щ�ӵ��ڲ����˺�֮�󲢲������������紩͸�ӵ�
        for (int i = 0; i < projectileList.Count; i++)
        {
            //�ƶ��ӵ�
            //�����ӵ���ת����
            if (!rv_projectile_jobUpdateInfo[i].islifeended)
            {
                projectileList[i].UpdateBullet();
            }
            //�ӵ����ƶ����˺��������б�ά����Ҫ�����ĸ���ͬ��������
            if (projectileList[i].damageTriggerPattern == DamageTriggerPattern.Collider)
            {
                //����Move �� Rotation
                if (!rv_projectile_jobUpdateInfo[i].islifeended && !projectileList[i].IsApplyDamageAtThisFrame)
                {
                    projectileList[i].Move(rv_projectile_jobUpdateInfo[i].deltaMovement);
                    projectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_projectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    if (projectileList[i].IsApplyDamageAtThisFrame)
                    {
                        _projectileDamageList.Add(projectileList[i]);
                        damageProjectile_JobInfo.Add(projectile_JobInfo[i]);
                    }
                    //Collide�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    _projectileDeathIndexList.Add(i);
                }
            }

            if (projectileList[i].damageTriggerPattern == DamageTriggerPattern.PassTrough)
            {
                //Passthrough ���͵��ӵ��� ֮����������ĩβ
                if (!rv_projectile_jobUpdateInfo[i].islifeended && projectileList[i].TransFixionCount > 0)
                {
                    projectileList[i].Move(rv_projectile_jobUpdateInfo[i].deltaMovement);
                    projectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_projectile_jobUpdateInfo[i].rotation);
                }
                if (projectileList[i].IsApplyDamageAtThisFrame)
                {
                    _projectileDamageList.Add(projectileList[i]);
                    damageProjectile_JobInfo.Add(projectile_JobInfo[i]);
                }
                //PassTrough�������͵��ӵ��� ֻҪPass CountΪ0 ����LiftTimeΪ0 �ͻ�����
                if (projectileList[i].TransFixionCount <= 0 || rv_projectile_jobUpdateInfo[i].islifeended)
                {
                    _projectileDeathIndexList.Add(i);
                }
            }

            if (projectileList[i].damageTriggerPattern == DamageTriggerPattern.Point)
            {
                //����Move �� Rotation
                if (!rv_projectile_jobUpdateInfo[i].islifeended && !projectileList[i].IsApplyDamageAtThisFrame)
                {
                    projectileList[i].Move(rv_projectile_jobUpdateInfo[i].deltaMovement);
                    projectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_projectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    //Point�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    if (rv_projectile_jobUpdateInfo[i].islifeended)
                    {
                        _projectileDamageList.Add(projectileList[i]);
                        damageProjectile_JobInfo.Add(projectile_JobInfo[i]);
                        _projectileDeathIndexList.Add(i);
                    }
                }
            }

            if (projectileList[i].damageTriggerPattern == DamageTriggerPattern.Target)
            {
                //����Move �� Rotation
                if (!rv_projectile_jobUpdateInfo[i].islifeended && !projectileList[i].IsApplyDamageAtThisFrame)
                {
                    projectileList[i].Move(rv_projectile_jobUpdateInfo[i].deltaMovement);
                    projectileList[i].transform.rotation = Quaternion.Euler(0, 0, rv_projectile_jobUpdateInfo[i].rotation);
                }
                else
                {
                    if (projectileList[i].IsApplyDamageAtThisFrame)
                    {
                        _projectileDamageList.Add(projectileList[i]);
                        damageProjectile_JobInfo.Add(projectile_JobInfo[i]);
                    }
                    // Target�������͵��ӵ��� ֻҪLifeTime���� Damage��һ�������� �ͻ�ִ��Death
                    _projectileDeathIndexList.Add(i);
                }
            }
        }

        //���������ӵ����˺��߼�
        HandleProjectileDamage();
        //����ÿ���ӵ�����Ϣ����������LifeEnd���ӵ�
        UpdateBulletJobData();

        rv_projectile_jobUpdateInfo.Dispose();
    }
    public void HandleProjectileDamage()
    {
        //projectileDamageList = projectileList.FindAll(x => x.IsApplyDamageAtThisFrame);
        if (_projectileDamageList.Count == 0)
        {
            return;
        }
        rv_projectileDamageTargetIndex = new NativeArray<int>(_projectileDamageList.Count * AIManager.Instance.activeWeaponList.Count, Allocator.TempJob);
        rv_projectileDamageTargetCountPre = new NativeArray<int>(_projectileDamageList.Count, Allocator.TempJob);
        JobHandle jobHandle;

        //�ҵ������ӵ����˺�Ŀ�꣬�����ǵ������߶���� 
        //TODO ������Խ����Ż��������ӵ��Ƿ�Ϊ��Ŀ��������Job������Ŀǰ�ķ�ʽ��ͳһ���� ��Ҫ��������߼�֮��������
        Bullet.FindBulletDamageTargetJob findBulletDamageTargetJob = new Bullet.FindBulletDamageTargetJob
        {
            job_JobInfo = damageProjectile_JobInfo,
            job_targesTotalCount = AIManager.Instance.activeWeaponList.Count,
            job_targetsPos = AIManager.Instance.position,

            rv_findedTargetsCount = rv_projectileDamageTargetCountPre,
            rv_findedTargetIndex = rv_projectileDamageTargetIndex,

        };

        jobHandle = findBulletDamageTargetJob.ScheduleBatch(_projectileDamageList.Count, 2);
        jobHandle.Complete();


        int damagetargetindex;
        IDamageble damageble;

        //Loop���л�����˺����ӵ�List��
        for (int i = 0; i < _projectileDamageList.Count; i++)
        {
            //���ö�Ӧ�ӵ���prepareDamageTargetList�������ں���ʵ��Apply Damage��׼��
            for (int n = 0; n < rv_projectileDamageTargetCountPre[i]; n++)
            {
                damagetargetindex = rv_projectileDamageTargetIndex[i * AIManager.Instance.activeWeaponList.Count + n];
                if (damagetargetindex < 0 || damagetargetindex >= AIManager.Instance.activeWeaponList.Count)
                {
                    continue;
                }
                damageble = AIManager.Instance.activeWeaponList[damagetargetindex].GetComponent<IDamageble>();
                if (damageble != null && _projectileDamageList[i] != null)
                {
                    if(!_projectileDamageList[i].prepareDamageTargetList.Contains(damageble))
                    {
                        _projectileDamageList[i].prepareDamageTargetList.Add(damageble);
                    }
                }
            }
            //Apply Damage to every damage target
            _projectileDamageList[i].ApplyDamageAllTarget();
        }

        //damageProjectile_JobInfo.Dispose();
        rv_projectileDamageTargetCountPre.Dispose();
        rv_projectileDamageTargetIndex.Dispose();
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

            bulletJobInfo.update_targetPos = projectileList[i].initialTarget ? projectileList[i].initialTarget.transform.position : projectileList[i].transform.up;
            bulletJobInfo.update_selfPos = projectileList[i].transform.position;
            bulletJobInfo.update_lifeTimeRemain = rv_projectile_jobUpdateInfo[i].lifeTimeRemain;
            bulletJobInfo.update_moveDirection = rv_projectile_jobUpdateInfo[i].moveDirection;
            bulletJobInfo.update_currentSpeed = rv_projectile_jobUpdateInfo[i].currentSpeed;

            projectile_JobInfo[i] = bulletJobInfo;
        }

        //���һ�� ִ���ӵ�����
        _ProjectileDeathList.Clear();
        int deathindex = 0;
        for (int i = 0; i < _projectileDeathIndexList.Count; i++)
        {
            deathindex = _projectileDeathIndexList[i];
            _ProjectileDeathList.Add(projectileList[deathindex]);
            //projectileList[deathindex].Death(null);
        }
        for (int i = 0; i < _ProjectileDeathList.Count; i++)
        {
            _ProjectileDeathList[i].Death(null);
        }
    }



    public virtual void AddActiveUnit(Unit unit)
    {
        if(unit is ShipAdditionalWeapon)
        {
            ShipAdditionalWeapon weapon = unit as ShipAdditionalWeapon;
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
        if( unit is ShipAdditionalWeapon)
        {
            ShipAdditionalWeapon weapon = unit as ShipAdditionalWeapon;
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
            float3 initialtargetpos;
            if ((bullet.Owner as Weapon).aimingtype == WeaponAimingType.Directional)
            {
                initialtargetpos = bullet.transform.position+ bullet.transform.up * bullet.Owner.baseAttribute.WeaponRange;
            }
            else
            {
                initialtargetpos = bullet.initialTarget.transform.position;
            }

            float damagerRadius = 0;
            if(bullet.damagePattern == DamagePattern.PointRadius)
            {
                ///CalculateExplodeDamage
                damagerRadius = bullet.DamageRadiusBase;
            }

            projectile_JobInfo.Add(new ProjectileJobInitialInfo
                (
                    bullet.initialTarget ? bullet.initialTarget.transform.position : bullet.transform.up,
                    bullet.transform.position,
                    bullet.lifeTime,
                    bullet.maxSpeed,
                    bullet.initialSpeed,
                    bullet.acceleration,
                    bullet.rotSpeed,
                    bullet.InitialmoveDirection.ToVector3(),
                    initialtargetpos,
                   (int)bullet.movementType,
                   (int)bullet.damagePattern,
                   damagerRadius
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

    public void Unload()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        targetShip = null;
        //clear all ai
        activeWeaponList.Clear();
        activeBuildingList.Clear();

        //todo clear all bullet
        if (projectileList != null && projectileList.Count != 0)
        {
            for (int i = 0; i < projectileList.Count; i++)
            {
                projectileList[i].PoolableDestroy();
            }
        }
        projectileList.Clear();
        _projectileDeathIndexList.Clear();
        _projectileDamageList.Clear();

        //Dispose all job data
        DisposeAllJobData();
    }
    */
    public void PauseGame()
    {
        
    }

    public void UnPauseGame()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;




public class TargetUnitData : IJobData
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
        activeTargetUnitList.Clear();
    }

    public void DisposeReturnValue()
    {
        
    }

    public void UpdateData()
    {
        if(activeTargetUnitList.Count  == 0) { return; }
        for (int i = 0; i < activeTargetUnitList.Count; i++)
        {
            activeTargetUnitPos[i] = activeTargetUnitList[i].transform.position;
        }
    }

    public void Add(BaseShip ship)
    {
        if(ship.UnitList.Count == 0) { return; }
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            Add(ship.UnitList[i]);
        }
    }

    public void Add(Unit unit)
    {
        if (activeTargetUnitList.Contains(unit)) { return; }
        if (!unit.isActiveAndEnabled) { return; }
        activeTargetUnitList.Add(unit);
        activeTargetUnitPos.Add(unit.transform.position);
    }

    public void Remove(Unit unit)
    {
        int index = activeTargetUnitList.IndexOf(unit);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        activeTargetUnitList.RemoveAt(index);
        activeTargetUnitPos.RemoveAt(index);
    }

    public void Clear()
    {
        activeTargetUnitList.Clear();
        activeTargetUnitPos.Clear();
    }


}

public class AvoidenceCollisionData : IJobData
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

public class AgentData : IJobData
{
    public List<BaseShip> shipList;
    public List<IBoid> boidAgentList;
    public List<SteeringBehaviorController> steeringControllerList;
    public NativeList<BoidData> boidAgentJobData;

    //���Agent���µ���Ϊ����Ҫ����SteeringControllerData������
    public NativeList<SteeringControllerData> steeringControllerJobData;


    //��������return value��������ÿһ����Ϊ��Ӧһ����������
    public NativeArray<SteeringBehaviorInfo> rv_evade_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_arrive_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_face_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_cohesion_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_separation_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_alignment_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_collisionavoidance_steeringInfo;

    //ÿ��Agetn�������ݶ���������
    public NativeArray<SteeringBehaviorInfo> rv_deltaMovement;

    public AgentData()
    {
        shipList = new List<BaseShip>();
        boidAgentList = new List<IBoid>();
        steeringControllerList = new List<SteeringBehaviorController>();
        boidAgentJobData = new NativeList<BoidData>(Allocator.Persistent);
        steeringControllerJobData = new NativeList<SteeringControllerData>(Allocator.Persistent);
    }
    public void Dispose()
    {
        if (boidAgentJobData.IsCreated) { boidAgentJobData.Dispose(); }
        if (steeringControllerJobData.IsCreated) { steeringControllerJobData.Dispose(); }
        shipList.Clear();
        boidAgentList.Clear();
        steeringControllerList.Clear();
    }

    public void DisposeReturnValue()
    {
        if (rv_evade_steeringInfo.IsCreated) { rv_evade_steeringInfo.Dispose(); }
        if (rv_arrive_steeringInfo.IsCreated) { rv_arrive_steeringInfo.Dispose(); }
        if (rv_face_steeringInfo.IsCreated) { rv_face_steeringInfo.Dispose(); }
        if (rv_cohesion_steeringInfo.IsCreated) { rv_cohesion_steeringInfo.Dispose(); }
        if (rv_separation_steeringInfo.IsCreated) { rv_separation_steeringInfo.Dispose(); }
        if (rv_alignment_steeringInfo.IsCreated) { rv_alignment_steeringInfo.Dispose(); }
        if (rv_collisionavoidance_steeringInfo.IsCreated) { rv_collisionavoidance_steeringInfo.Dispose(); }
        if (rv_deltaMovement.IsCreated) { rv_deltaMovement.Dispose(); }
    }

    public void UpdateData()
    {
        if(shipList.Count == 0) { return; }
        BoidData data;
        for (int i = 0; i < shipList.Count; i++)
        {
            data.position = boidAgentList[i].GetPosition();
            data.velocity = boidAgentList[i].GetVelocity();
            data.rotationZ = boidAgentList[i].GetRotationZ();
            data.boidRadius = boidAgentList[i].GetRadius();
            boidAgentJobData[i] = data;
        }

    }

    public void Add(BaseShip ship)
    {
        if(shipList.Contains(ship))
        {
            return;
        }
        shipList.Add(ship);

        //add boid agent
        IBoid boid = ship.GetComponent<IBoid>();
        if(boid == null)
        {
            Debug.LogError("ship doesn't implement IBoid");
            return;
        }
        boidAgentList.Add(boid);

        //add steering controller
        SteeringBehaviorController controller = ship.GetComponent<SteeringBehaviorController>();
        if(controller == null)
        {
            Debug.LogError("ship doesn't has SteeringBehaviorController");
            return;
        }
        //ToDo ������ʱֻ����ͨAIShipConfig�Ĵ��� ���������˻����ͺ� ���˻�Ҳ��Ҫ�����Ӧ�����ã� 
        if(ship is AIShip)
        {
            controller.SetAIConfig((ship as AIShip).AIShipCfg);
        }

        if(ship is Drone)
        {
            //��Ҫ����ͬ���͵�shipconfig 
        }
        steeringControllerList.Add(controller);

        // add boid agent job data
        BoidData boidData = new BoidData();
        boidData.position = boid.GetPosition();
        boidData.velocity = boid.GetVelocity();
        boidData.rotationZ = boid.GetRotationZ();
        boidData.boidRadius = boid.GetRadius();
        boidAgentJobData.Add(boidData);

        //add steering controller job data
        SteeringControllerData steeringData = new SteeringControllerData();
        steeringData.maxAcceleration = controller.maxAcceleration;
        steeringData.maxAngularVelocity = controller.maxVelocity;
        steeringData.maxAngularAcceleration = controller.maxAngularAcceleration;
        steeringData.maxAngularVelocity = controller.maxAngularVelocity;
        steeringData.targetSerchingRadius = controller.targetSerchingRadius;
        steeringData.drag = controller.drag;

        steeringData.evade_isActive = controller.isActiveEvade;
        steeringData.evade_weight = controller.evadeBehaviorInfo.GetWeight();
        steeringData.evade_maxPrediction = controller.evadeBehaviorInfo.maxPrediction;

        steeringData.arrive_isActive = controller.isActiveArrive;
        steeringData.arrive_weight = controller.arrivelBehaviorInfo.GetWeight();
        steeringData.arrive_arriveRadius = controller.arrivelBehaviorInfo.arriveRadius;
        steeringData.arrive_slowRadius = controller.arrivelBehaviorInfo.slowRadius;

        steeringData.face_isActive = controller.isActiveFace;
        steeringData.face_weight = controller.faceBehaviorInfo.GetWeight();
        steeringData.face_targetRadius = controller.faceBehaviorInfo.facetargetRadius;

        steeringData.cohesion_isActive = controller.isActiveCohesion;
        steeringData.cohesion_weight = controller.cohesionBehaviorInfo.GetWeight();
        steeringData.cohesion_viewAngle = controller.cohesionBehaviorInfo.viewAngle;

        steeringData.separation_isActive = controller.isActiveSeparation;
        steeringData.separation_weight = controller.separationBehaviorInfo.GetWeight();
        steeringData.separation_threshold = controller.separationBehaviorInfo.threshold;
        steeringData.separation_decayCoefficient = controller.separationBehaviorInfo.decayCoefficient;

        steeringData.alignment_isActive = controller.isActiveAligment;
        steeringData.alignment_weight = controller.alignmentBehaviorInfo.GetWeight();
        steeringData.alignment_alignDistance = controller.alignmentBehaviorInfo.alignDistance;

        steeringData.collisionavoidance_isActive = controller.isActiveCollisionAvoidance;
        steeringData.collisionavoidance_weight = controller.collisionAvoidanceBehaviorInfo.GetWeight();

        steeringControllerJobData.Add(steeringData);

    }

    public void Add(List<BaseShip> shiplist)
    {
        for (int i = 0; i < shipList.Count; i++)
        {
            Add(shipList[i]);
        }
    }

    public void Remove(BaseShip ship)
    {
        if (!shipList.Contains(ship)) { return; }
        int index = shipList.IndexOf(ship);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        shipList.RemoveAt(index);
        boidAgentList.RemoveAt(index);
        steeringControllerList.RemoveAt(index);
        boidAgentJobData.RemoveAt(index);
        steeringControllerJobData.RemoveAt(index);
    }
    public void Clear()
    {
        shipList.Clear();
        boidAgentList.Clear();
        steeringControllerList.Clear();
        boidAgentJobData.Clear();
        steeringControllerJobData.Clear();
    }

}



public class WeaponData : IJobData
{

    public List<ShipAdditionalWeapon> activeWeaponList;
    public NativeList<WeaponJobData> activeWeaponJobData;

    public NativeArray<Weapon.WeaponTargetInfo> rv_weaponTargetsInfo;
    public WeaponData ()
    {
        activeWeaponList = new List<ShipAdditionalWeapon>();
        activeWeaponJobData = new NativeList<WeaponJobData>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (activeWeaponJobData.IsCreated) { activeWeaponJobData.Dispose(); }
        activeWeaponList.Clear();
    }

    public void DisposeReturnValue()
    {
        if (rv_weaponTargetsInfo.IsCreated) { rv_weaponTargetsInfo.Dispose(); }
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

    public void Add(ShipAdditionalWeapon weapon)
    {
        if (activeWeaponList.Contains(weapon)) { return; }
        activeWeaponList.Add(weapon);
        WeaponJobData tempweaponjobdata;
        tempweaponjobdata.attackRange = weapon.weaponAttribute.WeaponRange;
        tempweaponjobdata.position = weapon.transform.position;
        tempweaponjobdata.targetCount = weapon.maxTargetCount;
        activeWeaponJobData.Add(tempweaponjobdata);
    }

    public void Remove(ShipAdditionalWeapon weapon)
    {
        if (!activeWeaponList.Contains(weapon)) { return; }
        int index = activeWeaponList.IndexOf(weapon);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        activeWeaponList.RemoveAt(index);
        activeWeaponJobData.RemoveAt(index);
    }

    public void Clear()
    {
        activeWeaponList.Clear();
        activeWeaponJobData.Clear();

    }

}

public class BuildingData : IJobData
{
    public List<Building> activeBuildingList;
    public NativeList<BuildingJobData> activeBuildingJobData;

    public NativeArray<Building.BuildingTargetInfo> rv_buildingTargetsInfo;

    public BuildingData ()
    {
        activeBuildingList = new List<Building>();
        activeBuildingJobData = new NativeList<BuildingJobData>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (activeBuildingJobData.IsCreated) { activeBuildingJobData.Dispose(); }
        activeBuildingList.Clear();
    }

    public void DisposeReturnValue()
    {
        if (rv_buildingTargetsInfo.IsCreated) { rv_buildingTargetsInfo.Dispose(); }
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

    public void Add(Building building)
    {
        if (activeBuildingList.Contains(building)) { return; }
        activeBuildingList.Add(building);
        BuildingJobData tempbuildingjobdata;
        tempbuildingjobdata.attackRange = building.buildingAttribute.WeaponRange;
        tempbuildingjobdata.position = building.transform.position;
        tempbuildingjobdata.targetCount = building.maxTargetCount;
        activeBuildingJobData.Add(tempbuildingjobdata);
    }

    public void Remove(Building building)
    {
        if (!activeBuildingList.Contains(building)) { return; }
        int index = activeBuildingList.IndexOf(building);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        activeBuildingList.RemoveAt(index);
        activeBuildingJobData.RemoveAt(index);
    }

    public void Clear()
    {
        activeBuildingList.Clear();
        activeBuildingJobData.Clear();

    }
}




public class ProjectileData : IJobData
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
        activeProjectileList.Clear();
        damageProjectileList.Clear();
        deathProjectileIndexList.Clear();
        deathProjectileList.Clear();
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
    }

    public void Add(Projectile projectile)
    {
        if (activeProjectileList.Contains(projectile)) { return; }
        activeProjectileList.Add(projectile);
        float3 initialtargetpos;

        if ((projectile.Owner as Weapon).aimingtype == WeaponAimingType.Directional)
        {
            initialtargetpos = projectile.transform.position + projectile.transform.up * projectile.Owner.baseAttribute.WeaponRange;
        }
        else
        {
            initialtargetpos = projectile.initialTarget.transform.position;
        }

        float damagerRadius = 0;
        if (projectile.damagePattern == DamagePattern.PointRadius)
        {
            ///CalculateExplodeDamage
            damagerRadius = projectile.DamageRadiusBase;
        }

        activeProjectileJobData.Add(new ProjectileJobInitialInfo
            (
                projectile.initialTarget ? projectile.initialTarget.transform.position : projectile.transform.up,
                projectile.transform.position,
                projectile.lifeTime,
                projectile.maxSpeed,
                projectile.initialSpeed,
                projectile.acceleration,
                projectile.rotSpeed,
                projectile.InitialmoveDirection.ToVector3(),
                initialtargetpos,
               (int)projectile.movementType,
               (int)projectile.damagePattern,
               damagerRadius
            ));

    }

    public void Remove(Projectile projectile)
    {
        if (!activeProjectileList.Contains(projectile)) { return; }
        int index = activeProjectileList.IndexOf(projectile);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        activeProjectileList.RemoveAt(index);
        activeProjectileJobData.RemoveAt(index);
    }

    public void Clear()
    {
        activeProjectileList.Clear();
        activeProjectileJobData.Clear();
    }
}
public struct DroneData
{

}

public struct SteeringControllerData
{
    public float maxAcceleration;
    public float maxVelocity;
    public float maxAngularAcceleration;
    public float maxAngularVelocity;
    public float targetSerchingRadius;
    public float drag;


    public bool evade_isActive;
    public float evade_weight;
    public float evade_maxPrediction;

    public bool arrive_isActive;
    public float arrive_weight;
    public float arrive_arriveRadius;
    public float arrive_slowRadius;

    public bool face_isActive;
    public float face_weight;
    public float face_targetRadius;

    public bool cohesion_isActive;
    public float cohesion_weight;
    public float cohesion_viewAngle;

    public bool separation_isActive;
    public float separation_weight;
    public float separation_threshold;
    public float separation_decayCoefficient;


    public bool alignment_isActive;
    public float alignment_weight;
    public float alignment_alignDistance;

    public bool collisionavoidance_isActive;
    public float collisionavoidance_weight;

}



public struct BoidData
{
    public float3 position;
    public float3 velocity;
    public float rotationZ;
    public float boidRadius;
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
public class JobController : IPauseable
{

    public bool ProcessJobs;
    public PlayerShip playerShip;
    public IBoid playerIBoid;


    public AgentData activeTargetAgentData;
    public TargetUnitData activeTargetUnitData;

    public AgentData activeSelfAgentData;

    public WeaponData activeSelfWeaponData;
    public BuildingData activeSelfBuildingData;
    public ProjectileData activeSelfProjectileData;

    public JobController()
    {
        GameManager.Instance.RegisterPauseable(this);
        Initialization();
    }

    ~ JobController()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        DisposeJobData();
    }

    public virtual void Initialization()
    {
      
        playerShip = RogueManager.Instance.currentShip;
        playerIBoid = playerShip.GetComponent<IBoid>();
        activeTargetAgentData = new AgentData();

        activeTargetUnitData = new TargetUnitData();

        activeSelfAgentData = new AgentData();
        activeSelfWeaponData = new WeaponData();
        activeSelfBuildingData = new BuildingData();
        activeSelfProjectileData = new ProjectileData();
    }



    public virtual void ClearJobData()
    {
        activeTargetAgentData.Clear();
        activeTargetUnitData.Clear();
        activeSelfAgentData.Clear();
        activeSelfWeaponData.Clear();
        activeSelfBuildingData.Clear();
        activeSelfProjectileData.Clear();
    }

    public virtual void UnLoad()
    {
        GameManager.Instance.UnRegisterPauseable(this);

        DisposeJobData();
    }

    public virtual void GameOver()
    {
        SetProcessJobs(false);

        for (int i = 0; i < activeSelfWeaponData.activeWeaponList.Count; i++)
        {
            activeSelfWeaponData.activeWeaponList[i]?.GameOver();
        }
        for (int i = 0; i < activeSelfBuildingData.activeBuildingList.Count; i++)
        {
            activeSelfBuildingData.activeBuildingList[i]?.GameOver();
        }
        for (int i = 0; i < activeTargetUnitData.activeTargetUnitList.Count; i++)
        {
            activeTargetUnitData.activeTargetUnitList[i]?.GameOver();
        }
        for (int i = 0; i < activeSelfProjectileData.activeProjectileList.Count; i++)
        {
            activeSelfProjectileData.activeProjectileList[i]?.GameOver();
        }

        UnLoad();
    }



    public virtual void SetProcessJobs(bool isProcessJobs)
    {
        ProcessJobs = isProcessJobs;
    }
    public virtual void UpdateJobs()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessJobs) { return; }
        UpdateAdditionalWeapon();
        UpdateAdditionalBuilding();
        UpdateProjectile();

        //������Ҫ�ȸ����ӵ�����Ϣ��Ȼ���ڰ��������ӵ��Ƴ��� ����rv aibullet�ľ�̬���ݳ����޷�����
        //��Ȼ���˷������������ǿ��Ա�֤index�����λ
        UpdateTargetData();
    }

    public virtual void LaterUpdateJobs()
    {

    }

    public virtual void FixedUpdateJobs()
    {

    }

    public virtual void PauseGame()
    {
        
    }

    public virtual void UnPauseGame()
    {
        
    }




    public virtual void DisposeJobData()
    {
        activeSelfWeaponData.Dispose();
        activeSelfBuildingData.Dispose();
        activeTargetUnitData.Dispose();
        activeSelfProjectileData.Dispose();
    }

    protected virtual void UpdateTargetData()
    {
        activeTargetUnitData.UpdateData();
    }

    public virtual void  UpdateAdditionalWeapon()
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
        activeSelfProjectileData.DisposeReturnValue();

    }
    public virtual void UpdateAdditionalBuilding()
    {
        activeSelfBuildingData.UpdateData();
        activeSelfBuildingData.DisposeReturnValue();
    }

    protected virtual void UpdateProjectile()
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
        HandleProjectileDamage();
        activeSelfProjectileData.UpdateData();
        HandleProjectileDeath();
        activeSelfProjectileData.DisposeReturnValue();
    }
    protected virtual void HandleProjectileDamage()
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

    protected virtual void HandleProjectileDeath()
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

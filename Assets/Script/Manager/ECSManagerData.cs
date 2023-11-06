
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static GameHelper;


public class OtherTargetData : IJobData
{
    public List<IOtherTarget> otherTargetList;
    public NativeList<float3> otherTargetPos;


    public OtherTargetData()
    {
        otherTargetList = new List<IOtherTarget>();
        otherTargetPos = new NativeList<float3>(Allocator.Persistent);
    }


    public void Dispose()
    {
        
    }

    public void DisposeReturnValue()
    {
        
    }

    public void UpdateData()
    {
        if (otherTargetList.Count == 0) { return; }
        for (int i = 0; i < otherTargetList.Count; i++)
        {
            IOtherTarget unit = otherTargetList[i];
            unit.OnUpdateBattle();
            otherTargetPos[i] = unit.GetPosition();
        }
    }

    public void Add(BaseShip ship)
    {
        if (ship.UnitList.Count == 0) { return; }
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            Add(ship.UnitList[i] as IOtherTarget);
        }
    }

    public void Add(IOtherTarget other)
    {
        if (otherTargetList.Contains(other)) { return; }
        if (!other.GetActiveAndEnabled()) { return; }
        otherTargetList.Add(other);
        otherTargetPos.Add(other.GetPosition());
    }

    public void Remove(IOtherTarget other)
    {
        if (!otherTargetList.Contains(other)) { return; }
        int index = otherTargetList.IndexOf(other);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        otherTargetList.RemoveAt(index);
        otherTargetPos.RemoveAt(index);
    }

    public void Clear()
    {
        otherTargetList.Clear();
        if (otherTargetPos.IsCreated) { otherTargetPos.Clear(); }
    }
}

public class UnitData : IJobData
{

    public List<Unit> unitList;
    public NativeList<float3> unitPos;

    public UnitData()
    {
        unitList = new List<Unit>();
        unitPos = new NativeList<float3>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (unitPos.IsCreated) { unitPos.Dispose(); }
        unitList.Clear();
    }

    public void DisposeReturnValue()
    {

    }

    public void UpdateData()
    {
        if (unitList.Count == 0) { return; }
        for (int i = 0; i < unitList.Count; i++)
        {
            var unit = unitList[i];
            unit.OnUpdateBattle();
            unitPos[i] = unit.transform.position;
        }
    }

    public void Add(BaseShip ship)
    {
        if (ship.UnitList.Count == 0) { return; }
        for (int i = 0; i < ship.UnitList.Count; i++)
        {
            Add(ship.UnitList[i]);
        }
    }

    public void Add(Unit unit)
    {
        if (unitList.Contains(unit)) { return; }
        if (!unit.isActiveAndEnabled) { return; }
        unitList.Add(unit);
        unitPos.Add(unit.transform.position);
    }

    public void Remove(Unit unit)
    {
        if (!unitList.Contains(unit)) { return; }
        int index = unitList.IndexOf(unit);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        unitList.RemoveAt(index);
        unitPos.RemoveAt(index);
    }

    public void Clear()
    {
        unitList.Clear();
        if (unitPos.IsCreated) { unitPos.Clear(); }

    }


}

public class IBoidData : IJobData
{
    public List<BaseShip> shipList;
    public List<IBoid> boidAgentList;
    public NativeList<BoidJobData> boidAgentJobData;

    public IBoidData()
    {
        shipList = new List<BaseShip>();
        boidAgentList = new List<IBoid>();
        boidAgentJobData = new NativeList<BoidJobData>(Allocator.Persistent);
    }
    public virtual void Dispose()
    {
        if (boidAgentJobData.IsCreated) { boidAgentJobData.Dispose(); }
        shipList.Clear();
        boidAgentList.Clear();
    }

    public virtual void DisposeReturnValue()
    {

    }

    public virtual void UpdateData()
    {
        if (shipList.Count == 0) { return; }
        BoidJobData data;
        for (int i = 0; i < shipList.Count; i++)
        {
         
            data.position = boidAgentList[i].GetPosition();
            data.velocity = boidAgentList[i].GetVelocity();
            data.rotationZ = boidAgentList[i].GetRotationZ();
            data.boidRadius = boidAgentList[i].GetRadius();
            boidAgentJobData[i] = data;
        }
    }

    public virtual void Add(BaseShip ship)
    {
        if (shipList.Contains(ship))
        {
            return;
        }
        shipList.Add(ship);

        //add boid agent
        IBoid boid = ship.GetComponent<IBoid>();
        if (boid == null)
        {
            Debug.LogError("ship doesn't implement IBoid");
            return;
        }
        boidAgentList.Add(boid);

        // add boid agent job data
        BoidJobData boidData = new BoidJobData();
        boidData.position = boid.GetPosition();
        boidData.velocity = boid.GetVelocity();
        boidData.rotationZ = boid.GetRotationZ();
        boidData.boidRadius = boid.GetRadius();
        boidAgentJobData.Add(boidData);

    }

    public virtual void Add(List<BaseShip> shiplist)
    {
        for (int i = 0; i < shiplist.Count; i++)
        {
            Add(shiplist[i]);
        }
    }

    public virtual void Remove(BaseShip ship)
    {
        if (!shipList.Contains(ship)) { return; }
        int index = shipList.IndexOf(ship);
        RemoveAt(index);
    }

    public virtual void RemoveAt(int index)
    {
        shipList.RemoveAt(index);
        boidAgentList.RemoveAt(index);
        boidAgentJobData.RemoveAt(index);
    }
    public virtual void Clear()
    {
        shipList.Clear();
        boidAgentList.Clear();
        if (boidAgentJobData.IsCreated)
        {
            boidAgentJobData.Clear();
        }
    }
}




public class AgentData : IBoidData
{
    public List<SteeringBehaviorController> steeringControllerList;

    //如果Agent有新的行为，需要增加SteeringControllerData的数据
    public NativeList<SteeringControllerJobData> steeringControllerJobDataNList;


    //并且增加return value的数量。每一个行为对应一个返回数据
    public NativeArray<SteeringBehaviorInfo> rv_evade_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_arrive_steeringInfo;
    public NativeArray<bool> rv_arrive_isVelZero;
    public NativeArray<SteeringBehaviorInfo> rv_face_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_cohesion_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_separation_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_alignment_steeringInfo;
    public NativeArray<SteeringBehaviorInfo> rv_collisionavoidance_steeringInfo;

    //每个Agetn最后的数据都会在这里
    public NativeArray<SteeringBehaviorInfo> rv_deltaMovement;

    public AgentData()
    {
        shipList = new List<BaseShip>();
        boidAgentList = new List<IBoid>();
        steeringControllerList = new List<SteeringBehaviorController>();
        boidAgentJobData = new NativeList<BoidJobData>(Allocator.Persistent);
        steeringControllerJobDataNList = new NativeList<SteeringControllerJobData>(Allocator.Persistent);
    }
    public override void Dispose()
    {
        if (boidAgentJobData.IsCreated) { boidAgentJobData.Dispose(); }
        if (steeringControllerJobDataNList.IsCreated) { steeringControllerJobDataNList.Dispose(); }
        shipList.Clear();
        boidAgentList.Clear();
        steeringControllerList.Clear();
    }

    public override void DisposeReturnValue()
    {
        if (rv_evade_steeringInfo.IsCreated) { rv_evade_steeringInfo.Dispose(); }
        if (rv_arrive_steeringInfo.IsCreated) { rv_arrive_steeringInfo.Dispose(); }
        if (rv_arrive_isVelZero.IsCreated) { rv_arrive_isVelZero.Dispose(); }
        if (rv_face_steeringInfo.IsCreated) { rv_face_steeringInfo.Dispose(); }
        if (rv_cohesion_steeringInfo.IsCreated) { rv_cohesion_steeringInfo.Dispose(); }
        if (rv_separation_steeringInfo.IsCreated) { rv_separation_steeringInfo.Dispose(); }
        if (rv_alignment_steeringInfo.IsCreated) { rv_alignment_steeringInfo.Dispose(); }
        if (rv_collisionavoidance_steeringInfo.IsCreated) { rv_collisionavoidance_steeringInfo.Dispose(); }
        if (rv_deltaMovement.IsCreated) { rv_deltaMovement.Dispose(); }
    }

    public override void UpdateData()
    {
        if (shipList.Count == 0) { return; }
        BoidJobData data;
        SteeringControllerJobData steeringJobData = new SteeringControllerJobData();
        SteeringBehaviorController controller;
        for (int i = 0; i < shipList.Count; i++)
        {
            data.position = boidAgentList[i].GetPosition();
            data.velocity = boidAgentList[i].GetVelocity();
            data.rotationZ = boidAgentList[i].GetRotationZ();
            data.boidRadius = boidAgentList[i].GetRadius();
            boidAgentJobData[i] = data;

            controller = shipList[i].GetComponent<SteeringBehaviorController>();
            steeringJobData.maxAcceleration = controller.maxAcceleration;
            steeringJobData.maxVelocity = controller.maxVelocity;
            steeringJobData.maxAngularAcceleration = controller.maxAngularAcceleration;
            steeringJobData.maxAngularVelocity = controller.maxAngularVelocity;
            steeringJobData.targetSerchingRadius = controller.targetSerchingRadius;
            steeringJobData.drag = controller.drag;

            steeringJobData.evadeData.evade_isActive = controller.isActiveEvade;
            steeringJobData.evadeData.evade_weight = controller.evadeBehaviorInfo.GetWeight();
            steeringJobData.evadeData.evade_maxPrediction = controller.evadeBehaviorInfo.maxPrediction;

            steeringJobData.arriveData.arrive_isActive = controller.isActiveArrive;
            steeringJobData.arriveData.arrive_weight = controller.arrivelBehaviorInfo.GetWeight();
            steeringJobData.arriveData.arrive_arriveRadius = controller.arrivelBehaviorInfo.arriveRadius;
            steeringJobData.arriveData.arrive_slowRadius = controller.arrivelBehaviorInfo.slowRadius;

            steeringJobData.faceData.face_isActive = controller.isActiveFace;
            steeringJobData.faceData.face_weight = controller.faceBehaviorInfo.GetWeight();
            steeringJobData.faceData.face_targetRadius = controller.faceBehaviorInfo.facetargetRadius;

            steeringJobData.cohesionData.cohesion_isActive = controller.isActiveCohesion;
            steeringJobData.cohesionData.cohesion_weight = controller.cohesionBehaviorInfo.GetWeight();
            steeringJobData.cohesionData.cohesion_viewAngle = controller.cohesionBehaviorInfo.viewAngle;

            steeringJobData.separationData.separation_isActive = controller.isActiveSeparation;
            steeringJobData.separationData.separation_weight = controller.separationBehaviorInfo.GetWeight();
            steeringJobData.separationData.separation_threshold = controller.separationBehaviorInfo.threshold;
            steeringJobData.separationData.separation_decayCoefficient = controller.separationBehaviorInfo.decayCoefficient;

            steeringJobData.alignmentData.alignment_isActive = controller.isActiveAligment;
            steeringJobData.alignmentData.alignment_weight = controller.alignmentBehaviorInfo.GetWeight();
            steeringJobData.alignmentData.alignment_alignDistance = controller.alignmentBehaviorInfo.alignDistance;

            steeringJobData.collisionAvoidenceData.collisionavoidance_isActive = controller.isActiveCollisionAvoidance;
            steeringJobData.collisionAvoidenceData.collisionavoidance_weight = controller.collisionAvoidanceBehaviorInfo.GetWeight();

            steeringControllerJobDataNList[i] = steeringJobData;
        }
    }

    public override void Add(BaseShip ship)
    {
        if (shipList.Contains(ship))
        {
            return;
        }
        shipList.Add(ship);

        //add boid agent
        IBoid boid = ship.GetComponent<IBoid>();
        if (boid == null)
        {
            Debug.LogError("ship doesn't implement IBoid");
            return;
        }
        boidAgentList.Add(boid);

        //add steering controller
        SteeringBehaviorController controller = ship.GetComponent<SteeringBehaviorController>();
        if (controller == null)
        {
            Debug.LogError("ship doesn't has SteeringBehaviorController");
            return;
        }
        //ToDo 这里暂时只有普通AIShipConfig的处理， 增加了无人机类型后， 无人机也需要处理对应的配置， 
        if (ship is AIShip)
        {
            controller.SetAIConfig((ship as AIShip).AIShipCfg);
        }

        if (ship is BaseDrone)
        {
            controller.SetDroneConfig((ship as BaseDrone).droneCfg);
            //需要处理不同类型的shipconfig 
        }
        steeringControllerList.Add(controller);

        // add boid agent job data
        BoidJobData boidData = new BoidJobData();
        boidData.position = boid.GetPosition();
        boidData.velocity = boid.GetVelocity();
        boidData.rotationZ = boid.GetRotationZ();
        boidData.boidRadius = boid.GetRadius();
        boidAgentJobData.Add(boidData);

        //add steering controller job data
        SteeringControllerJobData steeringJobData = new SteeringControllerJobData();
        steeringJobData.maxAcceleration = controller.maxAcceleration;
        steeringJobData.maxVelocity = controller.maxVelocity;
        steeringJobData.maxAngularAcceleration = controller.maxAngularAcceleration;
        steeringJobData.maxAngularVelocity = controller.maxAngularVelocity;
        steeringJobData.targetSerchingRadius = controller.targetSerchingRadius;
        steeringJobData.drag = controller.drag;


        steeringJobData.evadeData.evade_isActive = controller.isActiveEvade;
        steeringJobData.evadeData.evade_weight = controller.evadeBehaviorInfo.GetWeight();
        steeringJobData.evadeData.evade_maxPrediction = controller.evadeBehaviorInfo.maxPrediction;

        steeringJobData.arriveData.arrive_isActive = controller.isActiveArrive;
        steeringJobData.arriveData.arrive_weight = controller.arrivelBehaviorInfo.GetWeight();
        steeringJobData.arriveData.arrive_arriveRadius = controller.arrivelBehaviorInfo.arriveRadius;
        steeringJobData.arriveData.arrive_slowRadius = controller.arrivelBehaviorInfo.slowRadius;

        steeringJobData.faceData.face_isActive = controller.isActiveFace;
        steeringJobData.faceData.face_weight = controller.faceBehaviorInfo.GetWeight();
        steeringJobData.faceData.face_targetRadius = controller.faceBehaviorInfo.facetargetRadius;

        steeringJobData.cohesionData.cohesion_isActive = controller.isActiveCohesion;
        steeringJobData.cohesionData.cohesion_weight = controller.cohesionBehaviorInfo.GetWeight();
        steeringJobData.cohesionData.cohesion_viewAngle = controller.cohesionBehaviorInfo.viewAngle;

        steeringJobData.separationData.separation_isActive = controller.isActiveSeparation;
        steeringJobData.separationData.separation_weight = controller.separationBehaviorInfo.GetWeight();
        steeringJobData.separationData.separation_threshold = controller.separationBehaviorInfo.threshold;
        steeringJobData.separationData.separation_decayCoefficient = controller.separationBehaviorInfo.decayCoefficient;

        steeringJobData.alignmentData.alignment_isActive = controller.isActiveAligment;
        steeringJobData.alignmentData.alignment_weight = controller.alignmentBehaviorInfo.GetWeight();
        steeringJobData.alignmentData.alignment_alignDistance = controller.alignmentBehaviorInfo.alignDistance;

        steeringJobData.collisionAvoidenceData.collisionavoidance_isActive = controller.isActiveCollisionAvoidance;
        steeringJobData.collisionAvoidenceData.collisionavoidance_weight = controller.collisionAvoidanceBehaviorInfo.GetWeight();

        steeringControllerJobDataNList.Add(steeringJobData);

    }

    public override void Add(List<BaseShip> shiplist)
    {
        for (int i = 0; i < shiplist.Count; i++)
        {
            Add(shiplist[i]);
        }
    }

    public override void Remove(BaseShip ship)
    {
        if (!shipList.Contains(ship)) { return; }
        int index = shipList.IndexOf(ship);
        RemoveAt(index);
    }

    public override void RemoveAt(int index)
    {
        shipList.RemoveAt(index);
        boidAgentList.RemoveAt(index);
        steeringControllerList.RemoveAt(index);
        boidAgentJobData.RemoveAt(index);
        steeringControllerJobDataNList.RemoveAt(index);
    }
    public override void Clear()
    {
        shipList.Clear();
        boidAgentList.Clear();
        steeringControllerList.Clear();
        if (boidAgentJobData.IsCreated)
        {
            boidAgentJobData.Clear();
        }
        if (steeringControllerJobDataNList.IsCreated) { steeringControllerJobDataNList.Clear(); }
    }
}



public class WeaponData : IJobData
{

    public List<Weapon> activeWeaponList;
    public NativeList<UnitJobData> activeWeaponJobData;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<UnitTargetJobData> rv_weaponTargetsInfo;
    public WeaponData()
    {
        activeWeaponList = new List<Weapon>();
        activeWeaponJobData = new NativeList<UnitJobData>(Allocator.Persistent);
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
        if (activeWeaponList.Count == 0) { return; }
        UnitJobData tempweaponjobdata;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            tempweaponjobdata.range = activeWeaponList[i].weaponAttribute.Range;
            tempweaponjobdata.position = activeWeaponList[i].transform.position;
            tempweaponjobdata.targetCount = activeWeaponList[i].maxTargetCount;
            activeWeaponJobData[i] = tempweaponjobdata;
        }
    }

    public void Add(Weapon weapon)
    {
        if (activeWeaponList.Contains(weapon)) { return; }
        activeWeaponList.Add(weapon);
        UnitJobData tempweaponjobdata;
        tempweaponjobdata.range = weapon.weaponAttribute.Range;
        tempweaponjobdata.position = weapon.transform.position;
        tempweaponjobdata.targetCount = weapon.maxTargetCount;
        activeWeaponJobData.Add(tempweaponjobdata);
    }

    public void Remove(Weapon weapon)
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
        if (activeWeaponJobData.IsCreated) { activeWeaponJobData.Clear(); }
      

    }

}

public class BuildingData : IJobData
{
    public List<Building> activeBuildingList;
    public NativeList<UnitJobData> activeBuildingJobData;

    [NativeDisableContainerSafetyRestriction]
    public NativeArray<UnitTargetJobData> rv_buildingTargetsInfo;

    public BuildingData()
    {
        activeBuildingList = new List<Building>();
        activeBuildingJobData = new NativeList<UnitJobData>(Allocator.Persistent);
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
        UnitJobData buildingJobData;
        for (int i = 0; i < activeBuildingList.Count; i++)
        {
            buildingJobData.range = activeBuildingList[i].buildingAttribute.Range;
            buildingJobData.position = activeBuildingList[i].transform.position;
            buildingJobData.targetCount = activeBuildingList[i].maxTargetCount;
            activeBuildingJobData[i] = buildingJobData;
        }

    }

    public void Add(Building building)
    {
        if (activeBuildingList.Contains(building)) { return; }
        activeBuildingList.Add(building);
        UnitJobData tempbuildingjobdata;
        tempbuildingjobdata.range = building.buildingAttribute.Range;
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
        if (activeBuildingJobData.IsCreated) { activeBuildingJobData.Clear(); }
       

    }
}

public class DroneData : AgentData
{
    public List<BaseShip> targetShipList;
    public List<IBoid> targetBoidAgentList;
    public NativeList<BoidJobData> targetBoidAgentJobData;


    public DroneData()
    {
        targetShipList = new List<BaseShip>();
        targetBoidAgentList = new List<IBoid>();
        targetBoidAgentJobData = new NativeList<BoidJobData>(Allocator.Persistent);
    }
    public override void Dispose()
    {
        if (targetBoidAgentJobData.IsCreated) { targetBoidAgentJobData.Dispose(); }
        targetBoidAgentList.Clear();
        base.Dispose();
    }

    public override void DisposeReturnValue()
    {
        base.DisposeReturnValue();
    }

    public override void UpdateData()
    {
        base.UpdateData();
        //update target ship list and target boid agent list
        BoidJobData data;
        if(shipList == null || shipList.Count == 0) { return; }
        for (int i = 0; i < shipList.Count; i++)
        {
            var ship = shipList[i].GetFirstTarget();
            if(ship != null)
            {
                targetShipList[i] = shipList[i].GetFirstTarget();
                targetBoidAgentList[i] = targetShipList[i].GetComponent<IBoid>();
            }
            data.position = targetBoidAgentList[i].GetPosition();
            data.velocity = targetBoidAgentList[i].GetVelocity();
            data.rotationZ = targetBoidAgentList[i].GetRotationZ();
            data.boidRadius = targetBoidAgentList[i].GetRadius();
            targetBoidAgentJobData[i] = data;
        }
    }


    public override void Add(BaseShip drone)
    {
        base.Add(drone);
        targetShipList.Add(drone.GetFirstTarget());
        IBoid boid = drone.GetFirstTarget().GetComponent<IBoid>();
        if (boid == null)
        {
            Debug.LogError("ship doesn't implement IBoid");
            return;
        }
        targetBoidAgentList.Add(boid);
        // add boid agent job data
        BoidJobData boidData = new BoidJobData();
        boidData.position = boid.GetPosition();
        boidData.velocity = boid.GetVelocity();
        boidData.rotationZ = boid.GetRotationZ();
        boidData.boidRadius = boid.GetRadius();
        targetBoidAgentJobData.Add(boidData);
    }

    public override void Remove(BaseShip drone)
    {
        base.Remove(drone);
        //if (!shipList.Contains(drone)) { return; }
        //int index = shipList.IndexOf(drone);
        //RemoveAt(index);
       
    }

    public override void RemoveAt(int index)
    {
        targetShipList.RemoveAt(index);
        targetBoidAgentList.RemoveAt(index);
        targetBoidAgentJobData.RemoveAt(index);
        base.RemoveAt(index);
    }

    public override void Clear()
    {
        base.Clear();
        if (targetBoidAgentJobData.IsCreated) { targetBoidAgentJobData.Clear(); }
        targetBoidAgentList.Clear();
        targetBoidAgentList.Clear();

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
            initialtargetpos = projectile.transform.position + projectile.transform.up * projectile.Owner.baseAttribute.Range;
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
        if (activeProjectileJobData.IsCreated) { activeProjectileJobData.Clear(); }
        if(damageProjectileJobData.IsCreated) { damageProjectileJobData.Clear(); }
        
    }
}

public class SearchingTargetData : IJobData
{
    public NativeArray<float3> rv_searchingTargetsPosFlatArray;
    public NativeArray<float3> rv_searchingTargetsVelFlatArray;
    public NativeArray<int> rv_searchingTargetsCount;

    public SearchingTargetData()
    {

    }
    public void Dispose()
    {
        
    }

    public void DisposeReturnValue()
    {
        if (rv_searchingTargetsPosFlatArray.IsCreated) { rv_searchingTargetsPosFlatArray.Dispose(); }
        if (rv_searchingTargetsVelFlatArray.IsCreated) { rv_searchingTargetsVelFlatArray.Dispose(); }
        if (rv_searchingTargetsCount.IsCreated) { rv_searchingTargetsCount.Dispose(); }
    }

    public void UpdateData()
    {
       
    }
}

public struct DroneJobData
{

}

public struct SteeringControllerJobData
{
    public float maxAcceleration;
    public float maxVelocity;
    public float maxAngularAcceleration;
    public float maxAngularVelocity;
    public float targetSerchingRadius;
    public float drag;

    public SteeringJobDataEvade evadeData;
    public SteeringJobDataArrive arriveData;
    public SteeringJobDataFace faceData;
    public SteeringJobDataCohesion cohesionData;
    public SteeringJobDataSeparation separationData;
    public SteeringJobDataAlignment alignmentData;
    public SteeringJobDataCollisionAvoidence collisionAvoidenceData;
}

public struct SteeringJobDataEvade
{
    public bool evade_isActive;
    public float evade_weight;
    public float evade_maxPrediction;
}

public struct SteeringJobDataArrive
{
    public bool arrive_isActive;
    public float arrive_weight;
    public float arrive_arriveRadius;
    public float arrive_slowRadius;
}

public struct SteeringJobDataFace
{
    public bool face_isActive;
    public float face_weight;
    public float face_targetRadius;
}

public struct SteeringJobDataCohesion
{
    public bool cohesion_isActive;
    public float cohesion_weight;
    public float cohesion_viewAngle;
}
public struct SteeringJobDataSeparation
{
    public bool separation_isActive;
    public float separation_weight;
    public float separation_threshold;
    public float separation_decayCoefficient;
}

public struct SteeringJobDataAlignment
{
    public bool alignment_isActive;
    public float alignment_weight;
    public float alignment_alignDistance;
}

public struct SteeringJobDataCollisionAvoidence
{
    public bool collisionavoidance_isActive;
    public float collisionavoidance_weight;
}





public struct BoidJobData
{
    public float3 position;
    public float3 velocity;
    public float rotationZ;
    public float boidRadius;
}

public struct UnitJobData
{
    public float range;
    public float3 position;
    public int targetCount;
}

public struct AvoidenceCollisionJobData
{
    public float3 avoidanceCollisionPos;
    public float avoidanceCollisionRadius;
    public float3 avoidanceCollisionVel;
}


public enum FindCondition
{
    MaximumHP,
    MinimumHP,
    ClosestDistance,
    LongestDistance,
}

public struct UnitReferenceInfo
{
    public TargetInfo info;
    public Unit reference;
}



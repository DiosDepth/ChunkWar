using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static GameHelper;

public class ECSManager : Singleton<ECSManager>, IPauseable
{

    public bool ProcessECS;

    public int CurrentAIAgentCount
    {
        get
        {
            if(activeAIAgentData == null || activeAIAgentData.shipList == null)
            {
                return 0;
            }
            else
            {
                    return activeAIAgentData.shipList.Count;
                
            }
            
        } 
    }
    // job data owner type == AI
    public AgentData activeAIAgentData;
    public UnitData activeAIUnitData;
    public OtherTargetData activeAIOtherTargetData;
    public WeaponData activeAIWeaponData_Unit;
    public WeaponData activeAIWeaponData_Other;

    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneAgentData;
    public UnitData activeAIDroneUnitData;
    public WeaponData activeAIDroneWeaponData_Unit;
    public WeaponData activeAIDroneWeaponData_Other;
    public ProjectileData activeAIProjectileData;



    public int CurrentPlayerAgentCount 
    { 
        get
        {
            if (activePlayerBoidData == null || activePlayerBoidData.shipList == null)
            {
                return 0;
            }
            else
            {
                return activePlayerBoidData.shipList.Count;

            }
        } 
    }
    // job data owner type == Player
    public IBoidData activePlayerBoidData;
    public UnitData activePlayerUnitData;
    public OtherTargetData activePlayerOtherTargetData;
    public WeaponData activePlayerWeaponData_Unit;
    public WeaponData activePlayerWeaponData_Other;
    public BuildingData activePlayerBuildingData;
    public DroneData activePlayerDroneAgentData;
    public UnitData activePlayerDroneUnitData;
    public WeaponData activePlayerDroneWeaponData_Unit;
    public WeaponData activePlayerDroneWeaponData_Other;
    public ProjectileData activePlayerProjectileData;


    public PlayerJobController playerJobController;
    public AIJobController AIJobController;
    public override void Initialization()
    {
        base.Initialization();
        GameManager.Instance.RegisterPauseable(this);

        AIJobController = new AIJobController();
        activeAIAgentData = new AgentData();
        activeAIUnitData = new UnitData();
        activeAIOtherTargetData = new OtherTargetData();
        activeAIWeaponData_Unit = new WeaponData();
        activeAIWeaponData_Other = new WeaponData();
        activeAIBuildingData = new BuildingData();
        activeAIDroneAgentData = new DroneData();
        activeAIDroneUnitData = new UnitData();
        activeAIDroneWeaponData_Unit = new WeaponData();
        activeAIDroneWeaponData_Other = new WeaponData();
        activeAIProjectileData = new ProjectileData();

        playerJobController = new PlayerJobController();
        activePlayerBoidData = new IBoidData();
        activePlayerUnitData = new UnitData();
        activePlayerOtherTargetData = new OtherTargetData();
        activePlayerWeaponData_Unit = new WeaponData();
        activePlayerWeaponData_Other = new WeaponData();
        activePlayerBuildingData = new BuildingData();
        activePlayerDroneAgentData = new DroneData();
        activePlayerDroneUnitData = new UnitData();
        activePlayerDroneWeaponData_Unit = new WeaponData();
        activePlayerDroneWeaponData_Other = new WeaponData();
        activePlayerProjectileData = new ProjectileData();


        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddFixedUpdateListener(FixedUpdate);

    }


    public virtual void RegisterJobData(OwnerType type, BaseShip ship)
    {

        switch (type)
        {
            case OwnerType.Player:
                if(ship is PlayerShip)
                {
                    activePlayerBoidData.Add(ship);
                }

                if(ship is PlayerDrone)
                {
                    activePlayerDroneAgentData.Add(ship);
                }

                break;
            case OwnerType.AI:
                if(ship is AIShip)
                {
                    activeAIAgentData.Add(ship);
              
                }
                if(ship is AIDrone)
                {
                    activeAIDroneAgentData.Add(ship);
                }
                break;
        }

        OnShipCountChange(type);
    }
    public virtual void RegisterJobData(OwnerType type, Unit unit)
    {
        switch (type)
        {
            case OwnerType.Player:
     
                if(unit is Weapon )
                {
                    if(unit._owner is PlayerShip)
                    {
                        activePlayerUnitData.Add(unit);
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Unit)
                        {
                            activePlayerWeaponData_Unit.Add(unit as Weapon);
                        }
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Other)
                        {
                            activePlayerWeaponData_Other.Add(unit as Weapon);
                        }
                    }
                    if(unit._owner is PlayerDrone)
                    {
                        activePlayerDroneUnitData.Add(unit);
                        activePlayerOtherTargetData.Add(unit as IOtherTarget);
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Unit)
                        {
                            activePlayerDroneWeaponData_Unit.Add(unit as Weapon);
                        }
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Other)
                        {
                            activePlayerDroneWeaponData_Other.Add(unit as Weapon);
                        }
                    }
                }
                if(unit is Building)
                {
                    activePlayerUnitData.Add(unit);
                    activePlayerBuildingData.Add(unit as Building);
                }
                break;
            case OwnerType.AI:
          
                if (unit is Weapon)
                {
                    if(unit._owner is AIShip)
                    {
                        activeAIUnitData.Add(unit);
                        if((unit as Weapon).intercepttype == WeaponInterceptType.Unit)
                        {
                            activeAIWeaponData_Unit.Add(unit as Weapon);
                        }
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Other)
                        {
                            activeAIWeaponData_Other.Add(unit as Weapon);
                        }

                    }
                    if (unit._owner is AIDrone)
                    {
                        activeAIDroneUnitData.Add(unit);
                        activeAIOtherTargetData.Add(unit as IOtherTarget);
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Unit)
                        {
                            activeAIDroneWeaponData_Unit.Add(unit as Weapon);
                        }
                        if ((unit as Weapon).intercepttype == WeaponInterceptType.Other)
                        {
                            activeAIDroneWeaponData_Other.Add(unit as Weapon);
                        }
       
                    }

                }
                if (unit is Building)
                {
                    activeAIUnitData.Add(unit);
                    activeAIBuildingData.Add(unit as Building);
                }

                break;
        }
    }
    public virtual void RegisterJobData(OwnerType type, Bullet bullet)
    {
        switch (type)
        {
            case OwnerType.Player:
                if (bullet is Projectile)
                {
                    activePlayerProjectileData.Add(bullet as Projectile);
                    activePlayerOtherTargetData.Add(bullet as IOtherTarget);
                }
                
                break;
            case OwnerType.AI:
                if (bullet is Projectile)
                {
                    activeAIProjectileData.Add(bullet as Projectile);
                    activeAIOtherTargetData.Add(bullet as IOtherTarget);
                }
                break;
        }
    }
    public virtual void UnRegisterJobData(OwnerType type, BaseShip ship)
    {
        switch (type)
        {
            case OwnerType.Player:

                if(ship is BaseDrone)
                {
                    for (int i = 0; i < ship.UnitList.Count; i++)
                    {
                        activePlayerDroneUnitData.Remove(ship.UnitList[i]);
                        activePlayerOtherTargetData.Remove(ship.UnitList[i] as IOtherTarget);
                        if (ship.UnitList[i] is Weapon)
                        {
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Unit)
                            {
                                activePlayerDroneWeaponData_Unit.Remove(ship.UnitList[i] as Weapon);
                            }
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Other)
                            {
                                activePlayerDroneWeaponData_Other.Remove(ship.UnitList[i] as Weapon);
                            }
                        }
                    }
                    activePlayerDroneAgentData.Remove(ship);
                }
                else
                {
                    for (int i = 0; i < ship.UnitList.Count; i++)
                    {
                        activePlayerUnitData.Remove(ship.UnitList[i]);
                        if (ship.UnitList[i] is Weapon)
                        {
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Unit)
                            {
                                activePlayerWeaponData_Unit.Remove(ship.UnitList[i] as Weapon);
                            }
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Other)
                            {
                                activePlayerWeaponData_Other.Remove(ship.UnitList[i] as Weapon);
                            }

                        }
                        if (ship.UnitList[i] is Building)
                        {
                            activePlayerBuildingData.Remove(ship.UnitList[i] as Building);
                        }
                    }
                    activePlayerBoidData.Remove(ship);
                }
                break;
            case OwnerType.AI:
                if(ship is BaseDrone)
                {
                    for (int i = 0; i < ship.UnitList.Count; i++)
                    {
                        activeAIDroneUnitData.Remove(ship.UnitList[i]);
                        activeAIOtherTargetData.Remove(ship.UnitList[i] as IOtherTarget);
                        if (ship.UnitList[i] is Weapon)
                        {

                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Unit)
                            {
                                activeAIDroneWeaponData_Unit.Remove(ship.UnitList[i] as Weapon);
                            }
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Other)
                            {
                                activeAIDroneWeaponData_Other.Remove(ship.UnitList[i] as Weapon);
                            }
                         
                        }
                    }
                    activeAIDroneAgentData.Remove(ship);
                }
                else
                {
                    for (int i = 0; i < ship.UnitList.Count; i++)
                    {
                        activeAIUnitData.Remove(ship.UnitList[i]);
                        if (ship.UnitList[i] is Weapon)
                        {
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Unit)
                            {
                                activeAIWeaponData_Unit.Remove(ship.UnitList[i] as Weapon);
                            }
                            if ((ship.UnitList[i] as Weapon).intercepttype == WeaponInterceptType.Other)
                            {
                                activeAIWeaponData_Other.Remove(ship.UnitList[i] as Weapon);
                            }
                        }
                        if (ship.UnitList[i] is Building)
                        {
                            activeAIBuildingData.Remove(ship.UnitList[i] as Building);

                        }
                    }
                    activeAIAgentData.Remove(ship);
                }

                break;
        }
        OnShipCountChange(type);
    }
    public virtual void UnRegisterJobData(OwnerType type, Unit unit)
    {
        switch (type)
        {
            case OwnerType.Player:
                if(unit is Weapon)
                {
                    if(unit._owner is PlayerShip)
                    {
                        activePlayerUnitData.Remove(unit);
                        if((unit as Weapon).intercepttype == WeaponInterceptType.Unit)
                        {
                            activePlayerWeaponData_Unit.Remove(unit as Weapon);
                        }
                        if((unit as Weapon).intercepttype == WeaponInterceptType.Other)
                        {
                            activePlayerWeaponData_Other.Remove(unit as Weapon);
                        }
                    }
                    if(unit._owner is PlayerDrone)
                    {
                        activePlayerDroneUnitData.Remove(unit);
                        activePlayerOtherTargetData.Remove(unit as IOtherTarget);
                        activePlayerDroneWeaponData_Unit.Remove(unit as Weapon);
                    }
      
                }
                if(unit is Building)
                {
                    activePlayerUnitData.Remove(unit);
                    activePlayerBuildingData.Remove(unit as Building);
                }
           
                break;
            case OwnerType.AI:
                if (unit is Weapon)
                {
                    if(unit._owner is AIShip)
                    {
                        activeAIUnitData.Remove(unit);
                        activeAIWeaponData_Unit.Remove(unit as Weapon);
                    }
                    if(unit._owner is AIDrone)
                    {
                        activeAIDroneUnitData.Remove(unit);
                        activeAIOtherTargetData.Remove(unit as IOtherTarget);
                        activeAIDroneWeaponData_Unit.Remove(unit as Weapon);
                    }
                }
                if (unit is Building)
                {
                    activeAIUnitData.Remove(unit);
                    activeAIBuildingData.Remove(unit as Building);
                }
           
                break;
        }
    }
    public virtual void UnRegisterJobData(OwnerType type, Bullet bullet)
    {
        switch (type)
        {
            case OwnerType.Player:
                if (bullet is Projectile)
                {
                    activePlayerProjectileData.Remove(bullet as Projectile);
                    activePlayerOtherTargetData.Remove(bullet as IOtherTarget);
                }
                break;
            case OwnerType.AI:
                if(bullet is Projectile)
                {
                    activeAIProjectileData.Remove(bullet as Projectile);
                    activeAIOtherTargetData.Remove(bullet as IOtherTarget);
                }
                break;
        }
    }


    public virtual void Update()
    {
         if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessECS) { return; }


        //update Player job 
        playerJobController.UpdateAdditionalWeapon(ref activePlayerWeaponData_Unit, ref activeAIUnitData);
        playerJobController.UpdateAdditionalWeapon(ref activePlayerWeaponData_Other, ref activeAIOtherTargetData);
        playerJobController.UpdateAdditionalBuilding(ref activePlayerBuildingData, ref activeAIUnitData);
        playerJobController.UpdateAdditionalWeapon(ref activePlayerDroneWeaponData_Unit);


        playerJobController.UpdateProjectile(ref activePlayerProjectileData, ref activeAIUnitData);


        //update AI job 
        AIJobController.UpdateAdditionalWeapon(ref activeAIWeaponData_Unit, ref activePlayerUnitData);
        AIJobController.UpdateAdditionalWeapon(ref activeAIWeaponData_Other, ref activePlayerOtherTargetData);
        AIJobController.UpdateAdditionalBuilding(ref activeAIBuildingData, ref activePlayerUnitData);
        AIJobController.UpdateAdditionalWeapon(ref activeAIDroneWeaponData_Unit);

        AIJobController.UpdateProjectile(ref activeAIProjectileData, ref activePlayerUnitData);


    }

    public virtual void FixedUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessECS) { return; }
        UpdateJobData();

        //Allcate Drone Target 


        playerJobController.UpdateDroneAgentMovement(ref activePlayerDroneAgentData, activeAIAgentData);
        AIJobController.UpdateDroneAgentMovement(ref activeAIDroneAgentData, activePlayerBoidData);
        AIJobController.UpdateAgentMovement(ref activeAIAgentData,  activePlayerBoidData);

    }

    public virtual void UpdateJobData()
    {
        activeAIAgentData.UpdateData();
        activeAIUnitData.UpdateData();
        activeAIOtherTargetData.UpdateData();
        activeAIDroneAgentData.UpdateData();
        activeAIDroneUnitData.UpdateData();


        activePlayerBoidData.UpdateData();
        activePlayerUnitData.UpdateData();
        activePlayerOtherTargetData.UpdateData();
        activePlayerDroneAgentData.UpdateData();
        activePlayerDroneUnitData.UpdateData();
    }


    public virtual void SetProcessECS(bool isProcess)
    {
        ProcessECS = isProcess;
    }


    public virtual void DisposeJobData()
    {
        activeAIAgentData.Dispose();
        activeAIUnitData.Dispose();
        activeAIOtherTargetData.Dispose();
        activeAIWeaponData_Unit.Dispose();
        activeAIWeaponData_Other.Dispose();
        activeAIBuildingData.Dispose();
        activeAIDroneAgentData.Dispose();
        activeAIDroneUnitData.Dispose();
        activeAIDroneWeaponData_Unit.Dispose();
        activeAIDroneWeaponData_Other.Dispose();
        activeAIProjectileData.Dispose();

        activePlayerBoidData.Dispose();
        activePlayerUnitData.Dispose();
        activePlayerOtherTargetData.Dispose();
        activePlayerWeaponData_Unit.Dispose();
        activePlayerWeaponData_Other.Dispose();
        activePlayerBuildingData.Dispose();
        activePlayerDroneAgentData.Dispose();
        activePlayerDroneUnitData.Dispose();
        activePlayerDroneWeaponData_Unit.Dispose();
        activePlayerDroneWeaponData_Other.Dispose();
        activePlayerProjectileData.Dispose();
    }

    public virtual void ClearJobData()
    {
        activeAIAgentData.Clear();
        activeAIUnitData.Clear();
        activeAIOtherTargetData.Clear();
        activeAIWeaponData_Unit.Clear();
        activeAIWeaponData_Other.Clear();
        activeAIBuildingData.Clear();
        activeAIDroneAgentData.Clear();
        activeAIDroneWeaponData_Unit.Clear();
        activeAIDroneWeaponData_Other.Clear();
        activeAIProjectileData.Clear();

        activePlayerBoidData.Clear();
        activePlayerUnitData.Clear();
        activePlayerOtherTargetData.Clear();
        activePlayerWeaponData_Unit.Clear();
        activePlayerWeaponData_Other.Clear();
        activePlayerBuildingData.Clear();
        activePlayerDroneAgentData.Clear();
        activePlayerDroneWeaponData_Unit.Clear();
        activePlayerDroneWeaponData_Other.Clear();
        activePlayerProjectileData.Clear();
}

    public virtual void UnLoad()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        MonoManager.Instance.RemoveUpdateListener(Update);
        MonoManager.Instance.RemoveFixedUpdateListener(FixedUpdate);
        ClearJobData();
        DisposeJobData();
    }



    public virtual void GameOverProjectile()
    {
        for (int i = 0; i < activeAIProjectileData.activeProjectileList.Count; i++)
        {
            activeAIProjectileData.activeProjectileList[i]?.GameOver();
        }
        for (int i = 0; i < activePlayerProjectileData.activeProjectileList.Count; i++)
        {
            activePlayerProjectileData.activeProjectileList[i]?.GameOver();
        }
    }

    public virtual void GameOverAgent()
    {
        for (int i = 0; i < activeAIAgentData.shipList.Count; i++)
        {
            var ship = activeAIAgentData.shipList[i];
            for (int n = 0; n < ship.UnitList.Count; n++)
            {
                ship.UnitList[n]?.GameOver();
            }
            ship.GameOver();
        }


        for (int i = 0; i < activePlayerBoidData.shipList.Count; i++)
        {
            for (int n = 0; n < activePlayerBoidData.shipList[i].UnitList.Count; n++)
            {
                activePlayerBoidData.shipList[i].UnitList[n]?.GameOver();
            }
        }
    }



    public virtual void GameOver()
    {
        SetProcessECS(false);
        GameOverProjectile();
        GameOverAgent();
        UnLoad();
    }
    public void PauseGame()
    {

    }

    public void UnPauseGame()
    {

    }

    public List<Unit> GetRandomUnitList(OwnerType type, int count)
    {
        List<Unit> randomlist = new List<Unit>();
        System.Random rnd = new System.Random();
        int randIndex;

        if(type == OwnerType.Player)
        {
            for (int i = 0; i < count; i++)
            {
                randIndex = rnd.Next(activePlayerUnitData.unitList.Count);
                if (!randomlist.Contains(activePlayerUnitData.unitList[randIndex]))
                {
                    randomlist.Add(activePlayerUnitData.unitList[randIndex]);
                }
                else
                {
                    i--;
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                randIndex = rnd.Next(activeAIUnitData.unitList.Count);
                if (!randomlist.Contains(activeAIUnitData.unitList[randIndex]))
                {
                    randomlist.Add(activeAIUnitData.unitList[randIndex]);
                }
                else
                {
                    i--;
                }
            }
        }

        return randomlist;
    }

    public Unit GetUnitByUID(OwnerType type,  uint uid)
    {
        if(type == OwnerType.Player)
        {
            return activePlayerUnitData.unitList.Find(x => x.UID == uid);
        }
        else
        {
            return activeAIUnitData.unitList.Find(x => x.UID == uid);
        }
    }

    public int GetBuildingIndex(OwnerType type, Building building)
    {
        if(type == OwnerType.Player)
        {
            return activePlayerBuildingData.activeBuildingList.IndexOf(building);
        }
        else
        {
            return activeAIBuildingData.activeBuildingList.IndexOf(building);
        }
    }

    public List<Unit> GetUnitsWithCondition(OwnerType type, FindCondition condition, int count)
    {
        if(type == OwnerType.Player)
        {
            activePlayerUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP);
            var targetCount = Mathf.Min(count, activePlayerUnitData.unitList.Count);
            if (condition == FindCondition.MaximumHP)
            {
                return activePlayerUnitData.unitList.Take(targetCount).ToList();
            }
            else if (condition == FindCondition.MinimumHP)
            {
                return activePlayerUnitData.unitList.Reverse<Unit>().Take(targetCount).ToList();
            }
        }
        else
        {
            activeAIUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP);
            var targetCount = Mathf.Min(count, activeAIUnitData.unitList.Count);
            if (condition == FindCondition.MaximumHP)
            {
                return activeAIUnitData.unitList.Take(targetCount).ToList();
            }
            else if (condition == FindCondition.MinimumHP)
            {
                return activeAIUnitData.unitList.Reverse<Unit>().Take(targetCount).ToList();
            }
        }

        return new List<Unit>();
    }

    public Unit GetUnitWithCondition(OwnerType type, FindCondition condition)
    {
        Unit unit;
        if(type == OwnerType.Player)
        {
            if (condition == FindCondition.MaximumHP)
            {
                unit = activePlayerUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP).Last();
                return unit;
            }
            if (condition == FindCondition.MinimumHP)
            {
                unit = activePlayerUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP).First();
                return unit;
            }
        }
        else
        {
            if (condition == FindCondition.MaximumHP)
            {
                unit = activeAIUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP).Last();
                return unit;
            }
            if (condition == FindCondition.MinimumHP)
            {
                unit = activeAIUnitData.unitList.OrderBy(u => u.HpComponent.GetCurrentHP).First();
                return unit;
            }
        }


        return null;
    }

    public Unit GetAIUnitWithCondition(OwnerType type, FindCondition condition, Vector3 referencePos)
    {
        Unit unit;
        if(type == OwnerType.Player)
        {
            if (condition == FindCondition.ClosestDistance)
            {
                unit = activePlayerUnitData.unitList.OrderBy(u => math.distance(u.transform.position, referencePos)).First();
                return unit;
            }
            if (condition == FindCondition.LongestDistance)
            {
                unit = activePlayerUnitData.unitList.OrderBy(u => math.distance(u.transform.position, referencePos)).Last();
                return unit;
            }
        }
        else
        {
            if (condition == FindCondition.ClosestDistance)
            {
                unit = activeAIUnitData.unitList.OrderBy(u => math.distance(u.transform.position, referencePos)).First();
                return unit;
            }
            if (condition == FindCondition.LongestDistance)
            {
                unit = activeAIUnitData.unitList.OrderBy(u => math.distance(u.transform.position, referencePos)).Last();
                return unit;
            }
        }


        return null;
    }

    /// <summary>
    /// 获取AI飞船所有Units,只要有一个unit在飞船内，该飞船所有的unit都会计入
    /// </summary>
    /// <param name="type"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public virtual List<Unit> GetAIShipAllActiveUnitByTargetsInfo(TargetInfo[] info, out List<AIShip> outShips)
    {
        List<Unit> result = new List<Unit>();
        outShips = new List<AIShip>();
        for (int i = 0; i < info.Length; i++)
        {
            var target = activeAIUnitData.unitList[info[i].index];

            var ownerShip = target._owner;
            var shipUnits = ownerShip.UnitList;

            if (!outShips.Contains(ownerShip))
            {
                outShips.Add(ownerShip as AIShip);
            }

            for (int j = 0; j < shipUnits.Count; j++) 
            {
                var targetUnit = shipUnits[j];
                if(targetUnit.isActiveAndEnabled && !result.Contains(targetUnit))
                {
                    result.Add(targetUnit);
                }
            }
        }
        return result;
    }

    public virtual List<UnitReferenceInfo> GetActiveUnitReferenceByTargetInfo(OwnerType type,  TargetInfo[] info)
    {
        List<UnitReferenceInfo> result = new List<UnitReferenceInfo>();
        UnitReferenceInfo tempinfo;
        if (type == OwnerType.Player)
        {
            for (int i = 0; i < info.Length; i++)
            {
                tempinfo.info = info[i];
                tempinfo.reference = activePlayerUnitData.unitList[info[i].index];
                result.Add(tempinfo);
            }
        }
        else
        {
            for (int i = 0; i < info.Length; i++)
            {
                tempinfo.info = info[i];
                tempinfo.reference = activeAIUnitData.unitList[info[i].index];
                result.Add(tempinfo);
            }
        }
        return result;
    }


    private void OnShipCountChange(OwnerType type)
    {
        if(type == OwnerType.Player)
        {

        }
        else
        {
            LevelManager.Instance.EnemyShipCountChange(activeAIAgentData.shipList.Count);
        }
        
    }
}

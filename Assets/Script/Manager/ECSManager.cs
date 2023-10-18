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

    public int CurrentAIAgentCount { get { return activeAIAgentData.shipList.Count; } }
    // job data owner type == AI
    public AgentData activeAIAgentData;
    public UnitData activeAIUnitData;
    public WeaponData activeAIWeaponData;
    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneData;
    public ProjectileData activeAIProjectileData;


    public int CurrentPlayerAgentCount { get { return activePlayerAgentData.shipList.Count; } }
    // job data owner type == Player
    public PlayerShip playerShip;
    public IBoid playerIBoid;
    public AgentData activePlayerAgentData;
    public UnitData activePlayerUnitData;
    public WeaponData activePlayerWeaponData;
    public BuildingData activePlayerBuildingData;
    public DroneData activePlayerDroneData;
    public ProjectileData activePlayerProjectileData;


    public PlayerJobController playerJobController;
    public AIJobController AIJobController;
    public override void Initialization()
    {
        base.Initialization();
        GameManager.Instance.RegisterPauseable(this);
        playerJobController = new PlayerJobController();
        AIJobController = new AIJobController();
        activeAIAgentData = new AgentData();
        activeAIUnitData = new UnitData();
        activeAIWeaponData = new WeaponData();
        activeAIBuildingData = new BuildingData();
        activeAIDroneData = new DroneData();
        activeAIProjectileData = new ProjectileData();


        activePlayerAgentData = new AgentData();
        activePlayerUnitData = new UnitData();
        activePlayerWeaponData = new WeaponData();
        activePlayerBuildingData = new BuildingData();
        activePlayerDroneData = new DroneData();
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
                    playerShip = ship as PlayerShip;
                    playerIBoid = (ship as PlayerShip).GetComponent<IBoid>();
                }

                break;
            case OwnerType.AI:
                if(ship is AIShip)
                {
                    activeAIAgentData.Add(ship);
              
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
                activePlayerUnitData.Add(unit);
                if(unit is AdditionalWeapon)
                {
                    activePlayerWeaponData.Add(unit as AdditionalWeapon);
                }
                if(unit is Building)
                {
                    activePlayerBuildingData.Add(unit as Building);
                }
                break;
            case OwnerType.AI:
                activeAIUnitData.Add(unit);
                if (unit is AdditionalWeapon)
                {
                    activeAIWeaponData.Add(unit as AdditionalWeapon);
                }
                if (unit is Building)
                {
                    activePlayerBuildingData.Add(unit as Building);
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
                }
                
                break;
            case OwnerType.AI:
                if (bullet is Projectile)
                {
                    activeAIProjectileData.Add(bullet as Projectile);
                }
                break;
        }
    }
    public virtual void UnRegisterJobData(OwnerType type, BaseShip ship)
    {
        switch (type)
        {
            case OwnerType.Player:
                for (int i = 0; i < ship.UnitList.Count; i++)
                {
                    activePlayerUnitData.Remove(ship.UnitList[i]);
                    if (ship.UnitList[i] is AdditionalWeapon)
                    {
                        activePlayerWeaponData.Remove(ship.UnitList[i] as AdditionalWeapon);
                    }
                    if (ship.UnitList[i] is Building)
                    {
                        activePlayerBuildingData.Remove(ship.UnitList[i] as Building);
                    }
                }
                activePlayerAgentData.Remove(ship);
                break;
            case OwnerType.AI:
                for (int i = 0; i < ship.UnitList.Count; i++)
                {
                    activeAIUnitData.Remove(ship.UnitList[i]);
                    if (ship.UnitList[i] is AdditionalWeapon)
                    {
                        activeAIWeaponData.Remove(ship.UnitList[i] as AdditionalWeapon);
                    }
                    if (ship.UnitList[i] is Building)
                    {
                        activeAIBuildingData.Remove(ship.UnitList[i] as Building);
                    }
                }
                activeAIAgentData.Remove(ship);
                break;
        }
        OnShipCountChange(type);
    }
    public virtual void UnRegisterJobData(OwnerType type, Unit unit)
    {
        switch (type)
        {
            case OwnerType.Player:
                if(unit is AdditionalWeapon)
                {
                    activePlayerWeaponData.Remove(unit as AdditionalWeapon);
                }
                if(unit is Building)
                {
                    activePlayerBuildingData.Remove(unit as Building);
                }
                activePlayerUnitData.Remove(unit);
                break;
            case OwnerType.AI:
                if (unit is AdditionalWeapon)
                {
                    activeAIWeaponData.Remove(unit as AdditionalWeapon);
                }
                if (unit is Building)
                {
                    activeAIBuildingData.Remove(unit as Building);
                }
                activeAIUnitData.Remove(unit);
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
                }
                break;
            case OwnerType.AI:
                if(bullet is Projectile)
                {
                    activeAIProjectileData.Remove(bullet as Projectile);
                }
                break;
        }
    }


    public virtual void Update()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessECS) { return; }


        //update Player job 
        playerJobController.UpdateAdditionalWeapon(ref activePlayerWeaponData, ref activeAIUnitData);
        playerJobController.UpdateAdditionalBuilding(ref activePlayerBuildingData, ref activeAIUnitData);
        playerJobController.UpdateProjectile(ref activePlayerProjectileData, ref activeAIUnitData);


        //update AI job 
        AIJobController.UpdateAdditionalWeapon(ref activeAIWeaponData, ref activePlayerUnitData);
        AIJobController.UpdateAdditionalBuilding(ref activeAIBuildingData, ref activePlayerUnitData);
        AIJobController.UpdateProjectile(ref activeAIProjectileData, ref activePlayerUnitData);
      
    }

    public virtual void FixedUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!ProcessECS) { return; }
        UpdateJobData();
        AIJobController.UpdateAgentMovement(ref activeAIAgentData, ref activePlayerAgentData , ref playerIBoid);
    }

    public virtual void UpdateJobData()
    {
        activeAIAgentData.UpdateData();
        activeAIUnitData.UpdateData();



        activePlayerAgentData.UpdateData();
        activePlayerUnitData.UpdateData();
    }


    public virtual void SetProcessECS(bool isProcess)
    {
        ProcessECS = isProcess;
    }


    public virtual void DisposeJobData()
    {
        activeAIAgentData.Dispose();
        activeAIUnitData.Dispose();
        activeAIWeaponData.Dispose();
        activeAIBuildingData.Dispose();
        activeAIDroneData.Dispose();
        activeAIProjectileData.Dispose();

        activePlayerAgentData.Dispose();
        activePlayerUnitData.Dispose();
        activePlayerWeaponData.Dispose();
        activePlayerBuildingData.Dispose();
        activePlayerDroneData.Dispose();
        activePlayerProjectileData.Dispose();
    }

    public virtual void ClearJobData()
    {
        activeAIAgentData.Clear();
        activeAIUnitData.Clear();
        activeAIWeaponData.Clear();
        activeAIBuildingData.Clear();
        activeAIDroneData.Clear();
        activeAIProjectileData.Clear();

        activePlayerAgentData.Clear();
        activePlayerUnitData.Clear();
        activePlayerWeaponData.Clear();
        activePlayerBuildingData.Clear();
        activePlayerDroneData.Clear();
        activePlayerProjectileData.Clear();
}

    public virtual void UnLoad()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        MonoManager.Instance.RemoveUpdateListener(Update);
        MonoManager.Instance.RemoveFixedUpdateListener(FixedUpdate);
        DisposeJobData();
    }

    public virtual void GameOver()
    {
        SetProcessECS(false);

        for (int i = 0; i < activeAIAgentData.shipList.Count; i++)
        {
            for (int n = 0; n < activeAIAgentData.shipList[i].UnitList.Count; n++)
            {
                activeAIAgentData.shipList[i].UnitList[n]?.GameOver();
            }
        }
        for (int i = 0; i < activeAIProjectileData.activeProjectileList.Count; i++)
        {
            activeAIProjectileData.activeProjectileList[i]?.GameOver();
        }

        for (int i = 0; i < activePlayerAgentData.shipList.Count; i++)
        {
            for (int n = 0; n < activePlayerAgentData.shipList[i].UnitList.Count; n++)
            {
                activePlayerAgentData.shipList[i].UnitList[n]?.GameOver();
            }
        }
        for (int i = 0; i < activePlayerProjectileData.activeProjectileList.Count; i++)
        {
            activePlayerProjectileData.activeProjectileList[i]?.GameOver();
        }
        UnLoad();
    }
    public void PauseGame()
    {
        throw new System.NotImplementedException();
    }

    public void UnPauseGame()
    {
        throw new System.NotImplementedException();
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

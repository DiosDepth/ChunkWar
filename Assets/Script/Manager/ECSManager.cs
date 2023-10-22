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
    public WeaponData activeAIWeaponData;
    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneAgentData;
    public ProjectileData activeAIProjectileData;


    public int CurrentPlayerAgentCount 
    { 
        get
        {
            if (activePlayerAgentData == null || activePlayerAgentData.shipList == null)
            {
                return 0;
            }
            else
            {
                return activePlayerAgentData.shipList.Count;

            }
        } 
    }
    // job data owner type == Player
    public PlayerShip playerShip;
    public IBoid playerIBoid;
    public AgentData activePlayerAgentData;
    public UnitData activePlayerUnitData;
    public WeaponData activePlayerWeaponData;
    public BuildingData activePlayerBuildingData;
    public DroneData activePlayerDroneAgentData;
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
        activeAIWeaponData = new WeaponData();
        activeAIBuildingData = new BuildingData();
        activeAIDroneAgentData = new DroneData();
        activeAIProjectileData = new ProjectileData();

        playerJobController = new PlayerJobController();
        activePlayerAgentData = new AgentData();
        activePlayerUnitData = new UnitData();
        activePlayerWeaponData = new WeaponData();
        activePlayerBuildingData = new BuildingData();
        activePlayerDroneAgentData = new DroneData();
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

                if(ship is BaseDrone)
                {
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
                    activePlayerDroneAgentData.Remove(ship);
                }
                else
                {
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
                }
                break;
            case OwnerType.AI:
                if(ship is BaseDrone)
                {
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
                    activeAIDroneAgentData.Remove(ship);
                }
                else
                {
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
        playerJobController.UpdateDroneAgentMovement(ref activePlayerDroneAgentData, ref activeAIAgentData);
        AIJobController.UpdateAgentMovement(ref activeAIAgentData, ref activePlayerAgentData, ref playerIBoid);

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
        activeAIDroneAgentData.Dispose();
        activeAIProjectileData.Dispose();

        activePlayerAgentData.Dispose();
        activePlayerUnitData.Dispose();
        activePlayerWeaponData.Dispose();
        activePlayerBuildingData.Dispose();
        activePlayerDroneAgentData.Dispose();
        activePlayerProjectileData.Dispose();
    }

    public virtual void ClearJobData()
    {
        activeAIAgentData.Clear();
        activeAIUnitData.Clear();
        activeAIWeaponData.Clear();
        activeAIBuildingData.Clear();
        activeAIDroneAgentData.Clear();
        activeAIProjectileData.Clear();

        activePlayerAgentData.Clear();
        activePlayerUnitData.Clear();
        activePlayerWeaponData.Clear();
        activePlayerBuildingData.Clear();
        activePlayerDroneAgentData.Clear();
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


        for (int i = 0; i < activePlayerAgentData.shipList.Count; i++)
        {
            for (int n = 0; n < activePlayerAgentData.shipList[i].UnitList.Count; n++)
            {
                activePlayerAgentData.shipList[i].UnitList[n]?.GameOver();
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
    /// ��ȡAI�ɴ�����Units,ֻҪ��һ��unit�ڷɴ��ڣ��÷ɴ����е�unit�������
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

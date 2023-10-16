using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ECSManager : Singleton<ECSManager>, IPauseable
{

    public bool ProcessECS;


    // job data owner type == AI
    public AgentData activeAIAgentData;
    public UnitData activeAIUnitData;
    public WeaponData activeAIWeaponData;
    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneData;
    public ProjectileData activeAIProjectileData;


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
                activePlayerAgentData.Remove(ship);
                break;
            case OwnerType.AI:
                activeAIAgentData.Remove(ship);
                break;
        }
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
        UpdateJobData();

        //update Player job 
        playerJobController.UpdateAdditionalWeapon(ref activePlayerWeaponData, ref activeAIUnitData);
        playerJobController.UpdateAdditionalBuilding(ref activePlayerBuildingData, ref activeAIUnitData);
        playerJobController.UpdateProjectile(ref activePlayerProjectileData, ref activeAIUnitData);


        //update AI job 
        AIJobController.UpdateJobs();
    }

    public virtual void FixedUpdate()
    {
      
        AIJobController.FixedUpdateJobs();
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
}

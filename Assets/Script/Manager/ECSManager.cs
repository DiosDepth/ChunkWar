using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ECSManager : Singleton<ECSManager>, IPauseable
{

    public bool ProcessECS;
    public PlayerShip playerShip;
    public IBoid playerIBoid;

    // job data owner type == AI
    public AgentData activeAIAgentData;
    public UnitData activeAIUnitData;
    public WeaponData activeAIWeaponData;
    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneData;
    public ProjectileData activeAIProjectileData;


    // job data owner type == Player
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

    }
    public virtual void RegisterJobData(OwnerType type, Unit unit)
    {

    }
    public virtual void RegisterJobData(OwnerType type, Bullet bullet)
    {

    }
    public virtual void UnRegisterJobData(OwnerType type, BaseShip ship)
    {

    }
    public virtual void UnRegisterJobData(OwnerType type, Unit unit)
    {

    }
    public virtual void UnRegisterJobData(OwnerType type, Bullet bullet)
    {

    }


    public virtual void Update()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }

        UpdateJobData();



        playerJobController.UpdateAdditionalWeapon(ref activePlayerWeaponData, ref activeAIUnitData);
        //UpdateAdditionalBuilding();
        //UpdateProjectile();
        AIJobController.UpdateJobs();
    }

    public virtual void FixedUpdate()
    {
        playerJobController.FixedUpdateJobs();
        AIJobController.FixedUpdateJobs();
    }

    public virtual void UpdateJobData()
    {

    }


    public virtual void SetProcessECS(bool isProcess)
    {
        ProcessECS = isProcess;
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

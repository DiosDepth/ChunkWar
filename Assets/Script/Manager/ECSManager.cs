using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ECSManager : Singleton<ECSManager>
{
    // job data owner type == target
    public AgentData activeAIAgentData;
    public UnitData activeAIUnitData;
    public WeaponData activeAIWeaponData;
    public BuildingData activeAIBuildingData;
    public DroneData activeAIDroneData;
    public ProjectileData activeAIProjectileData;


    // job data owner type == self
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
        playerJobController = new PlayerJobController();
        AIJobController = new AIJobController();
        MonoManager.Instance.AddUpdateListener(Update);
        MonoManager.Instance.AddFixedUpdateListener(FixedUpdate);
    }


    public virtual void Update()
    {
        playerJobController.UpdateJobs();
        AIJobController.UpdateJobs();
    }

    public virtual void FixedUpdate()
    {
        playerJobController.FixedUpdateJobs();
        AIJobController.FixedUpdateJobs();
    }

}

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;



public struct TargetShipData
{
    
}
public struct TargetActiveUnitData 
{ 

}

public struct TargetAvoidenceCollisionData
{

}

public struct ShipData
{

}

public struct BuildingData
{
    
}
public struct WeaponData
{
    public List<ShipAdditionalWeapon> activeWeaponList;
    public NativeList<WeaponJobData> activeWeaponJobData;
    public NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;
    
    public void New()
    {
        activeWeaponList = new List<ShipAdditionalWeapon>();
        activeWeaponJobData = new NativeList<WeaponJobData>(Allocator.Persistent);

    }

    public void Dispose()
    {
        if (activeWeaponJobData.IsCreated) { activeWeaponJobData.Dispose(); }
        if (rv_weaponTargetsInfo.IsCreated) {rv_weaponTargetsInfo.Dispose(); }
    }

    public void DisposeJobReturn()
    {
        if (rv_weaponTargetsInfo.IsCreated) { rv_weaponTargetsInfo.Dispose(); }
    }
}

public struct WeaponJobData
{
    public float attackRange;
    public float3 position;
    public int targetCount;

}



public struct ProjectileData
{ 

}
public struct DroneData
{

}


public interface JobData
{
    public virtual void New() { }
    public virtual void Dispose() { }
    public virtual void DisposeJobReturn() { }
}
public class UnitManager : Singleton<UnitManager>, IPauseable
{


    public override void Initialization()
    {
        base.Initialization();
    }


    public virtual void DisposeJobData()
    {

    }
    public virtual void UnLoad()
    {

    }

    public virtual void GameOver()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void LaterUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    public virtual void PauseGame()
    {
        
    }

    public virtual void UnPauseGame()
    {
        
    }

    public virtual void UpdateShip()
    {

    }
    public virtual void  UpdateWeapon()
    {

    }
    public virtual void UpdateBuilding()
    {

    }

    public virtual void UpdateProjectile()
    {

    }

    public virtual void UpdateDrone()
    {

    }
}

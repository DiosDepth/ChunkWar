using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneFactory : Building
{
    public int maxDroneCount = 4;
    public float droneSpawnTime = 5;

    public float searchingRadius = 25;


    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
    }

    public override void InitBuildingAttribute(bool isPlayerShip)
    {
        base.InitBuildingAttribute(isPlayerShip);
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
    }

    public override void Restore()
    {
        base.Restore();
    }

    public override void SetUnitProcess(bool isprocess)
    {
        base.SetUnitProcess(isprocess);
    }

    public override void GameOver()
    {
        base.GameOver();
    }


    public override void BuildingUpdate()
    {
        base.BuildingUpdate();
    }

    public virtual void SpawnDrone()
    {

    }




}

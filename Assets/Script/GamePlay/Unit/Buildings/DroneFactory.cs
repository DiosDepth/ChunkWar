using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DroneFactoryAttribute : BuildingAttribute
{
    public float SpawnTime
    {
        get;
        protected set;
    }

    public float SearchingRadius
    {
        get;
        protected set;
    }

    public float MaxCount
    {
        get;
        protected set;
    }


    private float DroneSpawnTimeBase;
    private float DroneSearchingRadiusBase;
    private float DroneMaxCountBase;

    private List<BaseDrone> drones;
    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, OwnerShipType ownerType)
    {
        base.InitProeprty(parentUnit, cfg, ownerType);
        var _droneCfg = cfg as DroneFactoryConfig;
        DroneSpawnTimeBase = _droneCfg.DroneSpawnTime;
        DroneSearchingRadiusBase = _droneCfg.SearchingRadius;
        DroneMaxCountBase = _droneCfg.MaxDroneCount;

        SearchingRadius = DroneSearchingRadiusBase;
        MaxCount = DroneMaxCountBase;
        if (_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateSpawnTime);
            CalculateSpawnTime();
        }
        else if(_ownerShipType == OwnerShipType.AIShip)
        {
            SpawnTime = DroneSpawnTimeBase;
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        if (_ownerShipType == OwnerShipType.PlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.AttackSpeed, CalculateSpawnTime);
        }
    }

    private void CalculateSpawnTime()
    {
        var cd = GameHelper.CalculatePlayerWeaponCD(DroneSpawnTimeBase);
        SpawnTime = cd;
    }
}


public class DroneFactory : Building
{
    protected DroneFactoryConfig _factoryCfg;

    public Transform launchPoint;
    public DroneFactoryAttribute FactoryAttribute;

    public List<BaseDrone> DroneList
    {
        get { return _droneList; }
    }
    protected List<BaseDrone> _droneList;

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _factoryCfg = m_unitconfig as DroneFactoryConfig;
        base.Initialization(m_owner, m_unitconfig);
    }

    public override void InitBuildingAttribute(OwnerShipType type)
    {
        FactoryAttribute.InitProeprty(this, _factoryCfg, type);
        buildingAttribute = FactoryAttribute;
        baseAttribute = FactoryAttribute;
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

    public override bool OnUpdateBattle()
    {
        if (!base.OnUpdateBattle())
            return false;

        return true;
    }

    public virtual void SpawnDrone()
    {
        Vector2 spawnpoint = launchPoint.position.ToVector2();
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.AIFactoryPath, true, (obj) =>
        {
            ShipSpawnAgent spawnAgent = obj.GetComponent<ShipSpawnAgent>();
            spawnAgent.PoolableSetActive(true);
            spawnAgent.Initialization();

            ShipSpawnInfo spawninfo = new ShipSpawnInfo(_factoryCfg.ID, spawnpoint, null, (baseship) =>
            {
                ECSManager.Instance.RegisterJobData(OwnerType.Player, baseship);
            });
            spawnAgent.StartSpawn(spawninfo);
        });
    } 
}

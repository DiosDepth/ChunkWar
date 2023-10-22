using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DroneFactoryAttribute : BuildingAttribute
{
    public float RepairTime
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
            RepairTime = DroneSpawnTimeBase;
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
        RepairTime = cd;
    }
}


public class DroneFactory : Building
{
    protected DroneFactoryConfig _factoryCfg;
    public DroneFactoryAttribute factoryAttribute;

    public Transform launchPoint;
    public Transform launchedGroup;
    public Transform repairingGroup;
    public Transform apronGroup;
    public float launchIntervalTime = 1;




    public List<BaseDrone> DroneList
    {
        get { return _launchedList; }
    }
    protected List<BaseDrone> _launchedList = new List<BaseDrone>();
    protected Queue<BaseDrone> _apronQueue = new Queue<BaseDrone>();
    protected Queue<BaseDrone> _repairQueue = new Queue<BaseDrone>();
    

    private float _repairTimeCounter;
    private bool _isRepairing = false;
    private BaseDrone _repairingDrone;
    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _factoryCfg = m_unitconfig as DroneFactoryConfig;
        InitialFactorDrones();
        base.Initialization(m_owner, m_unitconfig);
    }

    public override void InitBuildingAttribute(OwnerShipType type)
    {
        factoryAttribute.InitProeprty(this, _factoryCfg, type);
        buildingAttribute = factoryAttribute;
        baseAttribute = factoryAttribute;
        _repairTimeCounter = factoryAttribute.RepairTime;
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

        for (int i = 0; i < _apronQueue.Count; i++)
        {
            _apronQueue.Dequeue().GameOver();
        }
        for (int i = 0; i < _repairQueue.Count; i++)
        {
            _repairQueue.Dequeue().GameOver();
        }

        for (int i = 0; i < _launchedList.Count; i++)
        {
            _launchedList[i].GameOver();
        }
        _launchedList.Clear();
        base.GameOver();
    }

    public override bool OnUpdateBattle()
    {
        if (!base.OnUpdateBattle())
            return false;

        return true;
    }

    public override void BuildingReady()
    {

    }

    public override void BuildingStart()
    {

        _repairTimeCounter = factoryAttribute.RepairTime;
    

    }

    public override void BuildingActive()
    {
        //launch if find target
        if (targetList != null && targetList.Count != 0)
        {
            LaunchDrone();
        }
        //Restore if any drone is crashed
        RepairDrone();
        
    }

    public override void BuildingEnd()
    {

    }

    public override void BuildingRecover()
    {

    }

    public async virtual  void LaunchDrone()
    {
        if(_apronQueue == null || _apronQueue.Count == 0) { return; }

        BaseDrone drone;
        _apronQueue.TryDequeue(out drone);
        if(drone == null)
        {
            return;
        }
        _launchedList.Add(drone);
        drone.transform.position = launchPoint.transform.position;
        drone.transform.SetParent(launchedGroup);
        drone.Launch();
        ECSManager.Instance.RegisterJobData(OwnerType.Player, drone);
        await UniTask.Delay((int)launchIntervalTime * 1000);
    }

    public virtual void Landing(BaseDrone drone)
    {
        if (_apronQueue.Contains(drone)) { return; }
        _apronQueue.Enqueue(drone);
        if(_launchedList.Contains(drone))
            _launchedList.Remove(drone);
        drone.transform.position = launchPoint.transform.position;
        drone.transform.SetParent(apronGroup);
        ECSManager.Instance.UnRegisterJobData(OwnerType.Player, drone);
    }

    public virtual void Crash(BaseDrone drone)
    {
        if (_repairQueue.Contains(drone)) { return; }
        _repairQueue.Enqueue(drone);
        if (_launchedList.Contains(drone))
            _launchedList.Remove(drone);
        drone.transform.SetParent(repairingGroup);
        ECSManager.Instance.UnRegisterJobData(OwnerType.Player, drone);
    }
    public virtual void RepairDrone()
    {
        if(_repairQueue == null || _repairQueue.Count == 0) { return; }
   
        if(!_isRepairing)
        {
            _repairQueue.TryDequeue(out _repairingDrone);
            _isRepairing = true;
        }
        if(_repairingDrone == null) { return; }
        _repairTimeCounter -= Time.deltaTime;
        if(_repairTimeCounter <= 0)
        {
            _repairingDrone.Repair();
            _repairingDrone.transform.SetParent(apronGroup);
        }


    }
    public virtual void InitialFactorDrones()
    {
        Vector2 spawnpoint = launchPoint.position.ToVector2();


        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.PlayerDroneSpawnAgentPath, true, (obj) =>
        {
            ShipSpawnAgent spawnAgent = obj.GetComponent<ShipSpawnAgent>();
            spawnAgent.PoolableSetActive(true);
            spawnAgent.Initialization();

            DroneSpawnInfo spawninfo = new DroneSpawnInfo(_factoryCfg.DroneID, spawnpoint, null, (baseship) =>
            {
                (baseship as BaseDrone).SetOwnerFactory(this);
                (baseship as BaseDrone).Landing();
            });

            for (int i = 0; i < factoryAttribute.MaxCount; i++)
            {
                spawnAgent.StartSpawn(spawninfo);
            }
         
        });
    } 
}

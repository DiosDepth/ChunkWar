using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

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
        DroneSearchingRadiusBase = _droneCfg.BaseRange;
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
    public float callbackThreshold = 2f;



    public List<BaseDrone> DroneList
    {
        get { return _droneList; }
    }
    protected List<BaseDrone> _droneList = new List<BaseDrone>();
    protected List<BaseDrone> _callbackList = new List<BaseDrone>();
    protected List<BaseDrone> _launchedList = new List<BaseDrone>();
    protected Queue<BaseDrone> _apronQueue = new Queue<BaseDrone>();
    protected Queue<BaseDrone> _repairQueue = new Queue<BaseDrone>();
    

    private float _repairTimeCounter;
    private const float _launchInterval = 0.25f;
    private float _launchIntervalCounter;
    private bool _isRepairing = false;
    private BaseDrone _repairingDrone;
    private int _allocateTargetCount = 0 ;

    private Dictionary<int, SteeringJobDataArrive> _droneArriveDataDic = new Dictionary<int, SteeringJobDataArrive>();

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        launchedGroup = LevelManager.DronePool;
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
        _allocateTargetCount = 0;
        _launchIntervalCounter = 0;

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);

        for (int i = 0; i < _droneList.Count; i++)
        {
            if (_owner is PlayerShip)
            {
                ECSManager.Instance.UnRegisterJobData(OwnerType.Player, _droneList[i]);
            }
            if (_owner is AIShip)
            {
                ECSManager.Instance.UnRegisterJobData(OwnerType.AI, _droneList[i]);
            }
            _droneList[i].Death(info);

        }
        _apronQueue.Clear();
        _repairQueue.Clear();
        _launchedList.Clear();

    }

    public override void Restore()
    {
        _repairTimeCounter = factoryAttribute.RepairTime;
        _allocateTargetCount = 0;
        _launchIntervalCounter = 0;
        base.Restore();
    }

    public override void SetUnitProcess(bool isprocess)
    {
        base.SetUnitProcess(isprocess);
    }

    public override void GameOver()
    {

        for (int i = 0; i < _droneList.Count; i++)
        {
            _droneList[i].GameOver();
        }
        _apronQueue.Clear();
        _repairQueue.Clear();
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
        if (!_isBuildingOn) { return; }
        buildingState.ChangeState(BuildingState.Start);
    }

    public override void BuildingStart()
    {
        _repairTimeCounter = factoryAttribute.RepairTime;
        buildingState.ChangeState(BuildingState.Active);
 
    }

    public override void BuildingActive()
    {
        if (damageState == DamagableState.Destroyed)
        {
            buildingState.ChangeState(BuildingState.End);
            return;
        }

        if (!_isBuildingOn) 
        {
            buildingState.ChangeState(BuildingState.Ready);
            return;
        }

 
        targetList.RemoveAll(t => t == null);
        targetList.RemoveAll(t => t.target.activeInHierarchy == false);
        if (targetList.Count == 0)
        {
            _callbackList.Clear();

            List<BaseDrone> backlist = new List<BaseDrone>();
            UnitTargetInfo backtargetinfo = new UnitTargetInfo(this.gameObject, ECSManager.Instance.GetBuildingIndex(GetOwnerType, this), 0, Vector3.zero);
            SteeringBehaviorController controller;
            SteeringJobDataArrive arrivedata;

            for (int i = 0; i < _launchedList.Count; i++)
            {
                _launchedList[i].SetFirstTargetInfo(backtargetinfo);
                for (int n = 0; n < _launchedList[i].UnitList.Count; n++)
                {
                    _launchedList[i].UnitList[n].SetUnitProcess(false);
                }
                controller = _launchedList[i].GetComponent<SteeringBehaviorController>();
                _droneArriveDataDic.TryGetValue(_launchedList[i].gameObject.GetInstanceID(), out arrivedata);
                arrivedata.arrive_arriveRadius = 0.5f;
                arrivedata.arrive_slowRadius = 0.5f;
                controller.SetArriveData(arrivedata);
                _callbackList.Add(_launchedList[i]);
            }

            _launchedList.Clear();
            buildingState.ChangeState(BuildingState.End);
            return;
        }

        if (_launchedList.Count != 0)
        {
            for (int i = 0; i < _launchedList.Count; i++)
            {
                _allocateTargetCount = _allocateTargetCount % targetList.Count;
                var ship = _launchedList[i].GetFirstTarget();
                if (ship == null || !ship.isActiveAndEnabled || ship == this._owner)
                {
                    _launchedList[i].SetFirstTargetInfo(targetList[_allocateTargetCount]);
                    _allocateTargetCount++; 
                }

            }
        }
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

        BuildingOFF();
        //callback �б��е����³����� 
        if (targetList.Count != 0)
        {
            for (int i = 0; i < _callbackList.Count; i++)
            {
                _callbackList[i].transform.SetParent(launchedGroup);
                //_callbackList[i].SetFirstTargetInfo(null);
                SteeringJobDataArrive dronedata;
                _droneArriveDataDic.TryGetValue(_callbackList[i].gameObject.GetInstanceID(), out dronedata);
                var controller = _callbackList[i].GetComponent<SteeringBehaviorController>();
                controller.SetArriveData(dronedata);
                _launchedList.Add(_callbackList[i]);
            }
            _callbackList.Clear();
            //�Ѿ�����apron�����·���
            //if (_apronQueue.Count != 0)
            //{
            //    int ct = _apronQueue.Count;
            //    for (int i = 0; i < ct; i++)
            //    {
            //        LaunchDrone();
            //    }
            //}

            //��������Drone��Ŀ�꣬���ҿ�ʼunit��process
            if (_launchedList.Count != 0)
            {
                for (int i = 0; i < _launchedList.Count; i++)
                {
                    for (int n = 0; n < _launchedList[i].UnitList.Count; n++)
                    {
                        _launchedList[i].UnitList[n].SetUnitProcess(true);
                    }
                    _allocateTargetCount = _allocateTargetCount % targetList.Count;
                    _launchedList[i].SetFirstTargetInfo(targetList[_allocateTargetCount]);
                    _allocateTargetCount++;
                }
            }

            buildingState.ChangeState(BuildingState.Active);
            return;
        }

        List<BaseDrone> back = _callbackList.FindAll(d => d.transform.position.DistanceXY(this.transform.position) <= callbackThreshold);
        if(back.Count == 0)
        {
            return;
        }

        for (int i = 0; i < back.Count; i++)
        {
            Landing(back[i]);
        }

        if (_callbackList.Count == 0)
        {
            buildingState.ChangeState(BuildingState.Recover);
        }
        //recover if it is recoverable unit
    }

    public override void BuildingRecover()
    {
        Restore();
        buildingState.ChangeState(BuildingState.Ready);
    }

    public virtual  void LaunchDrone()
    {
        if(_apronQueue == null || _apronQueue.Count == 0) { return; }

        if(_launchIntervalCounter >= 0)
        {
            _launchIntervalCounter -= Time.deltaTime;
            return;
        }
        _launchIntervalCounter = _launchInterval;
        BaseDrone drone;
        _apronQueue.TryDequeue(out drone);
        if(drone == null)
        {
            return;
        }
        _launchedList.Add(drone);

        //��Drone���г�ʼ����
        drone.transform.position = launchPoint.transform.position;
        drone.transform.SetParent(launchedGroup);
        drone.SetFirstTargetInfo(targetList[0]);
        SteeringJobDataArrive dronedata;
        _droneArriveDataDic.TryGetValue(drone.gameObject.GetInstanceID(), out dronedata);
        var controller = drone.GetComponent<SteeringBehaviorController>();
        controller.SetArriveData(dronedata);
        controller.lastpos = launchPoint.transform.position;

        drone.Launch();

        if(_owner is PlayerShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.Player, drone);
            for (int i = 0; i < drone.UnitList.Count; i++)
            {
                ECSManager.Instance.RegisterJobData(OwnerType.Player, drone.UnitList[i]);
                drone.UnitList[i].SetUnitProcess(true);
            }
        }
        if(_owner is AIShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.AI, drone);
            for (int i = 0; i < drone.UnitList.Count; i++)
            {
                ECSManager.Instance.RegisterJobData(OwnerType.AI, drone.UnitList[i]);
                drone.UnitList[i].SetUnitProcess(true);
            }
        }

    }





    public virtual void Landing(BaseDrone drone)
    {
        if (_apronQueue.Contains(drone)) { return; }
        _apronQueue.Enqueue(drone);
        if(_callbackList.Contains(drone))
            _callbackList.Remove(drone);
        drone.transform.SetParent(apronGroup);
        if(_owner is PlayerShip)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.Player, drone);
        }
        if(_owner is AIShip)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.AI, drone);
        }
   
    }

    public virtual void Crash(BaseDrone drone)
    {
        if (_repairQueue.Contains(drone)) { return; }
        _repairQueue.Enqueue(drone);
        if (_launchedList.Contains(drone))
            _launchedList.Remove(drone);
        if (_callbackList.Contains(drone))
            _callbackList.Remove(drone);
         drone.transform.SetParent(repairingGroup);

        if (_owner is PlayerShip)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.Player, drone);
        }
        if (_owner is AIShip)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.AI, drone);
        }

    }
    public virtual void RepairDrone()
    {
        if(_repairingDrone == null &&(_repairQueue == null || _repairQueue.Count == 0)) { return; }
   
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
            _apronQueue.Enqueue(_repairingDrone);
            _repairingDrone = null;
            _repairTimeCounter = factoryAttribute.RepairTime;
            _isRepairing = false;
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
                _droneList.Add((baseship as BaseDrone));
                var controller = baseship.GetComponent<SteeringBehaviorController>();
                SteeringJobDataArrive data = controller.GetArriveData();
                _droneArriveDataDic.Add(baseship.gameObject.GetInstanceID(), data);
            });

            for (int i = 0; i < factoryAttribute.MaxCount; i++)
            {
                spawnAgent.StartSpawn(spawninfo);
            }
         
        });
    } 
}

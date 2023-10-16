using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDrone : BaseShip, IPoolable
{
    protected DroneConfig _droneCfg;

    public DroneAttribute Attribute;

    public int DroneID;

    public OwnerType ownerType
    {
        get;
        private set;
    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void CreateShip()
    {
        base.CreateShip();
        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        DroneConfig droneCfg = DataManager.Instance.GetDroneConfig(DroneID);
        _droneCfg = droneCfg;
        baseShipCfg = droneCfg;
        ownerType = droneCfg.Owner;
        Vector2Int pos;
        InitAttribute();

        for (int row = 0; row < droneCfg.Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < droneCfg.Map.GetLength(1); colume++)
            {
                if (droneCfg.Map[row, colume] == 0)
                {
                    continue;
                }
                pos = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                _chunkMap[row, colume] = new Chunk();
                _chunkMap[row, colume].shipCoord = pos;
                _chunkMap[row, colume].state = DamagableState.Normal;
                _chunkMap[row, colume].isBuildingPiovt = false;
                _chunkMap[row, colume].isOccupied = false;
            }
        }
        //处理unit
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            unit.gameObject.SetActive(true);

            unit.Initialization(this, unitconfig);
            unit.SetUnitProcess(true);
            if (unit.IsCoreUnit)
            {
                CoreUnits.Add(unit);
            }
        }
    }

    public void PoolableReset()
    {

    }

    public void PoolableDestroy()
    {

        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    private void InitAttribute()
    {
        Attribute.InitProeprty(this, _droneCfg);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo
{
    public uint UID;
    public int UnitID;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;
    public List<Vector2Int> effectSlotCoords;

    public UnitInfo(Unit m_unit)
    {
        UID = m_unit.UID;
        UnitID = m_unit.UnitID;
        direction = m_unit.direction;
        pivot = m_unit.pivot;
        occupiedCoords = m_unit.occupiedCoords;
        effectSlotCoords = m_unit.effectSlotCoords;
    }

    public UnitInfo(ShipInitUnitConfig cfg)
    {
        UnitID = cfg.UnitID;
        pivot = new Vector2Int(cfg.PosX, cfg.PosY);
    }

    public UnitInfo(ShipUnitRuntimeSaveData sav)
    {
        UID = sav.UID;
        UnitID = sav.UnitID;
        pivot = new Vector2Int(sav.PosX, sav.PosY);
    }

}
public class ChunkPartMapInfo
{

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;
    public bool CorePoint = false;

    public ChunkPartMapInfo() { }
}

/// <summary>
/// 舰船临时存档
/// </summary>
public class ShipMapData
{
    public ChunkPartMapInfo[,] ShipMap = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    public List<UnitInfo> UnitList = new List<UnitInfo>();

    public ShipMapData(PlayerShip shipdata)
    {
        for (int row = 0; row < shipdata.ChunkMap.GetLength(0); row++)
        {
            for (int colume = 0; colume < shipdata.ChunkMap.GetLength(1); colume++)
            {
                ChunkPartMapInfo tempinfo = new ChunkPartMapInfo();
                tempinfo.shipCoord = shipdata.ChunkMap[row, colume].shipCoord;
                tempinfo.isOccupied = shipdata.ChunkMap[row, colume].isOccupied;
                tempinfo.isBuildingPiovt = shipdata.ChunkMap[row, colume].isBuildingPiovt;

                ShipMap[row, colume] = tempinfo;
            }
        }

        UnitList.Clear();
        for (int i = 0; i < shipdata.UnitList.Count; i++)
        {
            UnitList.Add(new UnitInfo(shipdata.UnitList[i] as Building));
        }

    }

    public ShipMapData(int[,] shipmap)
    {

        for (int row = 0; row < shipmap.GetLength(0); row++)
        {
            for (int colume = 0; colume < shipmap.GetLength(1); colume++)
            {

                if (shipmap[row, colume] == 0)
                {
                    continue;
                }
                ChunkPartMapInfo tempinfo = new ChunkPartMapInfo();
                tempinfo.shipCoord = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                tempinfo.isOccupied = false;
                tempinfo.isBuildingPiovt = false;

                if (shipmap[row, colume] == 2)
                {
                    tempinfo.CorePoint = true;
                }

                ShipMap[row, colume] = tempinfo;
            }
        }

    }

    /// <summary>
    /// 获取实际占据的格子
    /// </summary>
    /// <param name="unitID"></param>
    /// <param name="pivot"></param>
    /// <returns></returns>
    public List<Vector2Int> GetOccupiedCoords(int unitID, Vector2Int pivot)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        var unitCfg = DataManager.Instance.GetUnitConfig(unitID);

        var unitMap = unitCfg.GetReletiveCoord();
        unitMap.AddToAll(pivot);

        if (unitMap.Length > 0)
        {
            for (int i = 0; i < unitMap.Length; i++)
            {
                var buildarray = GameHelper.CoordinateMapToArray(unitMap[i], GameGlobalConfig.ShipMapSize);
                if (ShipMap[buildarray.x, buildarray.y].isOccupied)
                {
                    Debug.LogError("初始化Unit失败，格已经被占据 " + string.Format("X :{0}  Y :{1}", buildarray.x, buildarray.y));
                    return result;
                }

                result.Add(unitMap[i]);
            }
        }
        return result;
    }

    public List<Vector2Int> GetEffectSlotCoords(int unitID, Vector2Int pivot)
    {
        var unitCfg = DataManager.Instance.GetUnitConfig(unitID);
        if(unitCfg != null)
        {
            return unitCfg.GetEffectSlotCoord().AddToAll(pivot).ToList();
        }
        return new List<Vector2Int>();
    }

    /// <summary>
    /// 生成实际存档
    /// </summary>
    /// <returns></returns>
    public List<ShipUnitRuntimeSaveData> GenerateRuntimeSaveData()
    {
        List<ShipUnitRuntimeSaveData> result = new List<ShipUnitRuntimeSaveData>();
        for (int i = 0; i < UnitList.Count; i++) 
        {
            var unit = UnitList[i];
            ShipUnitRuntimeSaveData sav = new ShipUnitRuntimeSaveData();
            sav.UID = unit.UID;
            sav.UnitID = unit.UnitID;
            sav.PosX = unit.pivot.x;
            sav.PosY = unit.pivot.y;
            result.Add(sav);
        }
        return result;
    }

#if GMDEBUG
    public List<ShipInitUnitConfig> GenerateShipUnitData()
    {
        List<ShipInitUnitConfig> result = new List<ShipInitUnitConfig>();
        for (int i = 0; i < UnitList.Count; i++)
        {
            var unit = UnitList[i];
            ShipInitUnitConfig cfg = new ShipInitUnitConfig();
            cfg.UnitID = unit.UnitID;
            cfg.PosX = (byte)unit.pivot.x;
            cfg.PosY = (byte)unit.pivot.y;
            result.Add(cfg);
        }
        return result;
    }
#endif
}
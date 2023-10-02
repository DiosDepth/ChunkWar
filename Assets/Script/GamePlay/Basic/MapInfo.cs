using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class MapInfo
{
   
    public DamagableState state = DamagableState.None;

}
public class UnitInfo : MapInfo
{
    public uint UID;
    public int UnitID;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;

    public UnitInfo(Unit m_unit)
    {
        UID = m_unit.UID;
        UnitID = m_unit.UnitID;
        state = m_unit.state;
        direction = m_unit.direction;
        pivot = m_unit.pivot;
        occupiedCoords = m_unit.occupiedCoords;
    }

    public UnitInfo(ShipInitUnitConfig cfg)
    {
        UnitID = cfg.UnitID;
        pivot = new Vector2Int(cfg.PosX, cfg.PosY);
    }

}
public class ChunkPartMapInfo : MapInfo
{

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;
    public bool CorePoint = false;

    public ChunkPartMapInfo() { }
}


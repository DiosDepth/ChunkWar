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

    /// <summary>
    /// 目标进化UnitID
    /// </summary>
    public int TargetEvolveUnitID;

    /// <summary>
    /// 当前进化点数
    /// </summary>
    public byte EvolvePoints;

    public UnitInfo(Unit m_unit)
    {
        UID = m_unit.UID;
        UnitID = m_unit.UnitID;
        state = m_unit.state;
        direction = m_unit.direction;
        pivot = m_unit.pivot;
        occupiedCoords = m_unit.occupiedCoords;
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


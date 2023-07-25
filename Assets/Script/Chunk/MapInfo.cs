using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class MapInfo
{
   
    public DamagableState state = DamagableState.None;

}
public class UnitInfo :MapInfo
{
    public string unitName;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;


    public UnitInfo(Unit m_unit)
    {
        state = m_unit.state;
        unitName = m_unit.unitName;
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
    public ChunkType type = ChunkType.None;

    public ChunkPartMapInfo() { }
}


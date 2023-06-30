using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class MapInfo
{
   
    public UnitState state = UnitState.None;

}
public class BuildingMapInfo :MapInfo
{
    public string unitName;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;


    public BuildingMapInfo(Building building)
    {
        state = building.state;
        unitName = building.unitName;
        direction = building.direction;
        pivot = building.pivot;
        occupiedCoords = building.occupiedCoords;
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


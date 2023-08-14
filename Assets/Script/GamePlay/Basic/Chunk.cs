using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum ChunkType
{
    None = 0,
    Core = 1,
    Base = 2,

}

public enum DamagableState
{
    None,
    Normal,
    Destroyed,
}
public class Chunk : IDamageble
{

    public DamagableState state = DamagableState.None;

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;


    public Unit unit;

    public void Death()
    {
        throw new System.NotImplementedException();
    }

    public bool TakeDamage(ref DamageResultInfo info)
    {
        return unit.TakeDamage(ref info);
    }

}

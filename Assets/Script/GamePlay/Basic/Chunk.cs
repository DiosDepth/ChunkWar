using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DamagableState
{
    None,
    Normal,
    /// <summary>
    /// Ì±»¾
    /// </summary>
    Paralysis,
    /// <summary>
    /// ÍêÈ«Ïú»Ù
    /// </summary>
    Destroyed,
    Immortal,
}
public class Chunk : IDamageble
{

    public DamagableState state = DamagableState.None;

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;


    public Unit unit;

    public void Death(UnitDeathInfo info)
    {

    }

    public bool TakeDamage(DamageResultInfo info)
    {
        return unit.TakeDamage(info);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DamagableState
{
    None,
    Normal,
    /// <summary>
    /// 瘫痪
    /// </summary>
    Paralysis,
    /// <summary>
    /// 完全销毁
    /// </summary>
    Destroyed,
    /// <summary>
    /// 会受到伤害，并且会跳字、统计，但是不会实际扣血
    /// </summary>
    Immortal,
}
public class Chunk : IDamageble
{

    public DamagableState state = DamagableState.None;

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;

    public Vector2 Position
    {
        get { return unit.transform.position; }
    }


    public Unit unit;

    public void Death(UnitDeathInfo info)
    {

    }

    public bool TakeDamage(DamageResultInfo info)
    {
        return unit.TakeDamage(info);
    }

}

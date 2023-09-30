using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DamagableState
{
    None,
    Normal,
    /// <summary>
    /// ̱��
    /// </summary>
    Paralysis,
    /// <summary>
    /// ��ȫ����
    /// </summary>
    Destroyed,
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

    public bool TakeDamage(ref DamageResultInfo info)
    {
        return unit.TakeDamage(ref info);
    }

}

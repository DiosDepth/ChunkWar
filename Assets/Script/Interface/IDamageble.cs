using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble 
{
    Vector2 Position { get; }

    bool TakeDamage(DamageResultInfo info);
    void Death(UnitDeathInfo info);
}

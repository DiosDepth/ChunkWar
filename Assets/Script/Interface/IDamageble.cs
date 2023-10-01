using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble 
{

    bool TakeDamage(DamageResultInfo info);
    void Death(UnitDeathInfo info);
}

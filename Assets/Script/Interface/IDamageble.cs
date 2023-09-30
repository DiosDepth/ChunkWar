using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble 
{

    bool TakeDamage(ref DamageResultInfo info);
    void Death(UnitDeathInfo info);
}

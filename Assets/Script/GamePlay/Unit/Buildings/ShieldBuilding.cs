using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBuilding : Building
{
    public GeneralShieldHPComponet ShieldComponet;

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        if (ShieldComponet.IsShieldBroken)
        {
            return base.TakeDamage(ref info);
        }
        else
        {
            info.Damage = Mathf.CeilToInt(info.Damage * (1 + info.ShieldDamagePercent / 100f));
            ShieldComponet.TakeDamage(info.Damage);
            return false;
        }
    }
}

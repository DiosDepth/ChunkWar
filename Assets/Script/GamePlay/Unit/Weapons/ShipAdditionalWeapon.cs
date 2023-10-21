using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAdditionalWeapon : AdditionalWeapon
{

    public override void InitWeaponAttribute(OwnerShipType type)
    {
        base.InitWeaponAttribute(type);
        if (weaponAttribute.MagazineBased)
        {
            //magazine = 0;
            magazine = weaponAttribute.MaxMagazineSize;
        }
        else
        {
            ///Fix Value
            magazine = 1;
        }
     
    }
}

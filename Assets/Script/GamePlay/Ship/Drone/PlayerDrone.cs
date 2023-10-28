using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrone : BaseDrone
{



    public override void Death(UnitDeathInfo info)
    {
        ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
        ResetAllAnimation();
        base.Death(info);
    }
}

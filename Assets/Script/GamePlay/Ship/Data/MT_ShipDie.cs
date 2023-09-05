using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_ShipDie : ModifyTriggerData
{
    public MT_ShipDie(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {

    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.Action_OnShipDie += OnEnemyShipDie;
    }

    private void OnEnemyShipDie(BaseShip ship)
    {

    }
}

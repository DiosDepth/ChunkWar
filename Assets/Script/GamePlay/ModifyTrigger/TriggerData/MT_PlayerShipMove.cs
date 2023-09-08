using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_PlayerShipMove : ModifyTriggerData
{

    private MTC_OnPlayerShipMove _shipMoveCfg;

    public MT_PlayerShipMove(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _shipMoveCfg = cfg as MTC_OnPlayerShipMove;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerShipMove += OnShipMove;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerShipMove -= OnShipMove;
    }

    private void OnShipMove(bool isMove)
    {

    }
}

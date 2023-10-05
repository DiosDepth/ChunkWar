using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_PlayerShipMove : ModifyTriggerData
{

    private MTC_OnPlayerShipMove _shipMoveCfg;

    /// <summary>
    /// ÕýÔÚ´¥·¢
    /// </summary>
    private bool _isTriggering = false;

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
        if(isMove == _shipMoveCfg.IsMoving)
        {
            if (!_isTriggering)
            {
                EffectTrigger();
            }
            _isTriggering = true;
        }
        else
        {
            if (_isTriggering)
            {
                UnEffectTrigger();
            }
            _isTriggering = false;
        }
    }
}

public class MT_PlayerShipParry : ModifyTriggerData
{
    private MTC_OnPlayerShipParry _parryCfg;

    public MT_PlayerShipParry(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _parryCfg = cfg as MTC_OnPlayerShipParry;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerShipParry += OnPlayerShipParry;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerShipParry -= OnPlayerShipParry;
    }

    private void OnPlayerShipParry(Unit unit)
    {
        if (unit == null)
            return;

        Trigger(unit.UID);
    }
}
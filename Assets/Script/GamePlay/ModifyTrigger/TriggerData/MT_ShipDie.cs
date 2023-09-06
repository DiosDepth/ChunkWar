using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_ShipDie : ModifyTriggerData
{

    private int _currentKillCount;

    private MTC_OnKillEnemy _killEnemyCfg;

    public MT_ShipDie(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _killEnemyCfg = cfg as MTC_OnKillEnemy;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.Action_OnShipDie += OnEnemyShipDie;
    }

    private void OnEnemyShipDie(BaseShip ship)
    {
        _currentKillCount++;
        if (_currentKillCount >= _killEnemyCfg.TriggerRequireCount && CheckCanTrigger())
        {
            Trigger();
            if (_killEnemyCfg.IsLoop)
            {
                _currentKillCount = 0;
            }
        }
    }

    private bool CheckCanTrigger()
    {
        if (!_killEnemyCfg.IsLoop && currentTriggerCount >= 1)
            return false;

        return true;
    }

    public override void Reset()
    {
        base.Reset();
        _currentKillCount = 0;
    }
}

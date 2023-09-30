using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDeathInfo
{
    /// <summary>
    /// ÊÇ·ñ±©»÷»÷É±
    /// </summary>
    public bool isCriticalKill;
}

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

    private void OnEnemyShipDie(BaseShip ship, UnitDeathInfo info)
    {
        _currentKillCount++;
        if (_currentKillCount >= _killEnemyCfg.TriggerRequireCount && CheckCanTrigger(info))
        {
            Trigger();
            if (_killEnemyCfg.IsLoop)
            {
                _currentKillCount = 0;
            }
        }
    }

    private bool CheckCanTrigger(UnitDeathInfo info)
    {
        if (!_killEnemyCfg.IsLoop && currentTriggerCount >= 1)
            return false;

        if ((_killEnemyCfg.CheckCriticalKill == BoolType.True && !info.isCriticalKill) || (_killEnemyCfg.CheckCriticalKill == BoolType.False && info.isCriticalKill))
            return false;

        return true;
    }

    public override void Reset()
    {
        base.Reset();
        _currentKillCount = 0;
    }
}

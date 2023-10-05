using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_PlayerCoreUnitTakeDamage : ModifyTriggerData
{
    private MTC_OnPlayerCoreUnitTakeDamage _hitCfg;

    public DamageResultInfo DamageInfo;

    public MT_PlayerCoreUnitTakeDamage(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _hitCfg = cfg as MTC_OnPlayerCoreUnitTakeDamage;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerCoreUnitTakeDamage += OnTakeDamage;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerCoreUnitTakeDamage -= OnTakeDamage;
    }

    private void OnTakeDamage(DamageResultInfo damageInfo)
    {
        var damage = damageInfo.Damage;
        bool vaild = MathExtensionTools.CalculateCompareType(_hitCfg.Value, damage, _hitCfg.Compare);
        if (!vaild)
            return;

        if (Trigger())
        {
            if (_hitCfg.ModifyFinalDamage)
            {
                var percent = 1 + _hitCfg.DamageAddPercent / 100f;
                damageInfo.Damage = Mathf.RoundToInt(damageInfo.Damage * percent);
            }
        }
    }
}

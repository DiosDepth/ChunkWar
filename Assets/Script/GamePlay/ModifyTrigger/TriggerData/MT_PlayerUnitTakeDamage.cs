using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_PlayerUnitTakeDamage : ModifyTriggerData
{
    private MTC_OnPlayerUnitTakeDamage _hitCfg;

    public DamageResultInfo DamageInfo;

    public MT_PlayerUnitTakeDamage(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _hitCfg = cfg as MTC_OnPlayerUnitTakeDamage;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerUnitTakeDamage += OnTakeDamage;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerUnitTakeDamage -= OnTakeDamage;
    }

    private void OnTakeDamage(DamageResultInfo damageInfo)
    {
        if (_hitCfg.IsCoreUnit)
        {
            var unit = damageInfo.Target as Unit;
            if (unit == null || !unit.IsCoreUnit)
                return;
        }

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

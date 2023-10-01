using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInfo
{
    public bool isPlayerAttack;
    public WeaponDamageType DamageType;
    public bool isCritical;

}

public class MT_OnWeaponHitTarget : ModifyTriggerData
{
    private MTC_OnWeaponHitTarget _hitCfg;

    public MT_OnWeaponHitTarget(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _hitCfg = cfg as MTC_OnWeaponHitTarget;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnUnitHit += OnWeaponHit;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnUnitHit -= OnWeaponHit;
    }

    private void OnWeaponHit(HitInfo info, DamageResultInfo damageInfo)
    {
        if (!info.isPlayerAttack)
            return;

        if ((_hitCfg.DamageTypeBool == BoolType.True && info.DamageType != _hitCfg.DamageType) ||
            (_hitCfg.DamageTypeBool == BoolType.False && info.DamageType == _hitCfg.DamageType))
            return;

        if ((_hitCfg.CriticalBool == BoolType.True && !info.isCritical) ||
            (_hitCfg.CriticalBool == BoolType.False && info.isCritical))
            return;

        Trigger();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInfo
{
    public bool isPlayerAttack;
    public WeaponDamageType DamageType;

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

    private void OnWeaponHit(HitInfo info)
    {
        if (!info.isPlayerAttack)
            return;

        if (info.DamageType != _hitCfg.DamageType)
            return;

        Trigger();
    }
}

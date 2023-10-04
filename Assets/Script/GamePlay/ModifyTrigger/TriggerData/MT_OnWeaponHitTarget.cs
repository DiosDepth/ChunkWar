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

        if(Trigger())
        {
            if (_hitCfg.ModifyFinalDamage)
            {
                var percent = 1 + _hitCfg.DamageAddPercent / 100f;
                damageInfo.Damage = Mathf.RoundToInt(damageInfo.Damage * percent);
            }
        }
    }
}


public class MT_OnWeaponReload : ModifyTriggerData
{
    private MTC_OnWeaponReload _reloadCfg;

    private List<uint> _uidStorage = new List<uint>();

    public MT_OnWeaponReload(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _reloadCfg = cfg as MTC_OnWeaponReload;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerWeaponReload += OnPlayerWeaponReload;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
    }

    private void OnPlayerWeaponReload(uint weaponUID, bool reloadStart)
    {
        if (reloadStart)
        {
            if (!_uidStorage.Contains(weaponUID))
            {
                _uidStorage.Add(weaponUID);
            }
            EffectTrigger(weaponUID);
        }
        else
        {
            if (_uidStorage.Contains(weaponUID))
            {
                _uidStorage.Remove(weaponUID);
                UnEffectTrigger(weaponUID);
            }
        }
    }
}
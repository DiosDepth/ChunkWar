using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_OnWeaponHitTarget : ModifyTriggerData
{
    private MTC_OnWeaponHitTarget _hitCfg;

    public DamageResultInfo DamageInfo;

    public MT_OnWeaponHitTarget(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _hitCfg = cfg as MTC_OnWeaponHitTarget;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnUnitBeforeHit += OnWeaponHit;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnUnitBeforeHit -= OnWeaponHit;
    }

    private void OnWeaponHit(DamageResultInfo damageInfo)
    {
        DamageInfo = damageInfo;
        if (!damageInfo.IsPlayerAttack)
            return;

        if ((_hitCfg.DamageTypeBool == BoolType.True && damageInfo.DamageType != _hitCfg.DamageType) ||
            (_hitCfg.DamageTypeBool == BoolType.False && damageInfo.DamageType == _hitCfg.DamageType))
            return;

        if ((_hitCfg.CriticalBool == BoolType.True && !damageInfo.IsCritical) ||
            (_hitCfg.CriticalBool == BoolType.False && damageInfo.IsCritical))
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

public class MT_OnWeaponFire : ModifyTriggerData
{
    private MTC_OnWeaponFire _fireCfg;

    private Dictionary<uint, int> _fireCountDic = new Dictionary<uint, int>();

    public MT_OnWeaponFire(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _fireCfg = cfg as MTC_OnWeaponFire;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerWeaponFire += OnWeaponFire;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerWeaponFire -= OnWeaponFire;
    }

    private void OnWeaponFire(uint uid, int count)
    {
        ///数量保护
        count = Mathf.Clamp(1, count, 100);
        var fireCount = Mathf.Max(1, _fireCfg.FireCount);

        if (!_fireCfg.CheckFireCount)
        {
            Trigger(uid);
            return;
        }

        if (_fireCountDic.ContainsKey(uid))
        {
            var newCount = _fireCountDic[uid] + count;
            while(newCount >= fireCount)
            {
                newCount -= fireCount;
                Trigger();
            }
            _fireCountDic[uid] = newCount;
        }
        else
        {
            while(count >= fireCount)
            {
                count -= _fireCfg.FireCount;
                Trigger();
            }

            _fireCountDic.Add(uid, count);
        }
    }

}

public class MT_OnPlayerCreateExplode : ModifyTriggerData
{
    private MTC_OnPlayerCreateExplode _explodeCfg;

    public MT_OnPlayerCreateExplode(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _explodeCfg = cfg as MTC_OnPlayerCreateExplode;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerCreateExplode += OnPlayerCreateExplode;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerCreateExplode -= OnPlayerCreateExplode;
    }

    private void OnPlayerCreateExplode()
    {
        Trigger();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_OnShieldRecover : ModifyTriggerData
{
    private List<uint> _storageUnitIDs;

    public MT_OnShieldRecover(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _storageUnitIDs = new List<uint>();
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnShieldRecoverStart += OnShieldRecoverStart;
        LevelManager.Instance.OnShieldRecoverEnd += OnShieldRecoverEnd;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnShieldRecoverStart -= OnShieldRecoverStart;
        LevelManager.Instance.OnShieldRecoverEnd -= OnShieldRecoverEnd;
    }

    private void OnShieldRecoverStart(uint targetUID)
    {
        _storageUnitIDs.Add(targetUID);
        EffectTrigger(targetUID);
    }

    private void OnShieldRecoverEnd(uint targetUID)
    {
        if (_storageUnitIDs.Remove(targetUID)) 
        {
            UnEffectTrigger(targetUID);
        }
    }
}

public class MT_OnShieldBroken : ModifyTriggerData
{
    private MTC_OnShieldBroken _shieldCfg;

    public MT_OnShieldBroken(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _shieldCfg = cfg as MTC_OnShieldBroken;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnShieldBroken += OnShieldBroken;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnShieldBroken -= OnShieldBroken;
    }

    private void OnShieldBroken(uint targetUID)
    {
        var unit = AIManager.Instance.GetUnitByUID(targetUID);
        if (unit == null)
            return;

        if (_shieldCfg.IsPlayer && unit._owner is PlayerShip)
        {
            Trigger(targetUID);
        }
    }
}
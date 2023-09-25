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

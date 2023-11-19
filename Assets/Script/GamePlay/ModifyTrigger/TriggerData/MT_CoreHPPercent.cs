using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_CoreHPPercent : ModifyTriggerData
{
    private MTC_CoreHPPercent _coreConfig;
    private bool _istriggering = false;

    public MT_CoreHPPercent(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _coreConfig = cfg as MTC_CoreHPPercent;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnPlayerCoreHPPercentChange += OnCoreHPChange;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnPlayerCoreHPPercentChange -= OnCoreHPChange;
    }

    private void OnCoreHPChange(float percent, int oldValue, int newValue)
    {
        bool vaild = MathExtensionTools.CalculateCompareType(_coreConfig.HPPercent, percent * 100, _coreConfig.Compare);
        if(vaild && !_istriggering)
        {
            EffectTrigger();
            _istriggering = true;
        }
        else if(!vaild && _istriggering)
        {
            UnEffectTrigger();
            _istriggering = false;
        }
    }
}


public class MT_EnemyCoreHPPercent : ModifyTriggerData
{
    private MTC_EnemyCoreHPPercent _coreConfig;
    private bool _istriggering = false;

    public MT_EnemyCoreHPPercent(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _coreConfig = cfg as MTC_EnemyCoreHPPercent;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnEnemyCoreHPPercentChange += OnCoreHPChange;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnEnemyCoreHPPercentChange -= OnCoreHPChange;
    }

    private void OnCoreHPChange(float percent, int oldValue, int newValue)
    {
        bool vaild = MathExtensionTools.CalculateCompareType(_coreConfig.HPPercent, percent * 100, _coreConfig.Compare);
        if (vaild && !_istriggering)
        {
            EffectTrigger();
            _istriggering = true;
        }
        else if (!vaild && _istriggering)
        {
            UnEffectTrigger();
            _istriggering = false;
        }
    }
}

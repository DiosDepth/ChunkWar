using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * ¼ÆÊ±Modifer
 */
public abstract class TimerModiferData 
{

    private float TotalTime;
    private float _timer;

    private bool isNeedToRemove = false;
    public bool IsNeedToRemove
    {
        get { return isNeedToRemove; }
    }

    public uint UID;

    private bool useDuration;

    protected ModifyTriggerData _parentTrigger;

    public abstract TimerModifierStackConfig StackCfg { get; }

    public TimerModiferData(ModifyTriggerData parent, float totalTime, bool useDuration, uint uid)
    {
        this._parentTrigger = parent;
        this.TotalTime = totalTime;
        this.UID = uid;
        this.useDuration = useDuration;
        _timer = 0;
    }

    public virtual void OnAdded()
    {
        if (_parentTrigger.Config.SelfUnique)
        {
            _parentTrigger._uniqueEffecting = true;
        }
    }

    public virtual void OnRemove()
    {
        if (_parentTrigger.Config.SelfUnique)
        {
            _parentTrigger._uniqueEffecting = false;
        }
    }

    public void RefreshDuration()
    {
        _timer = 0;
    }

    public void OnUpdate()
    {
        if (!useDuration)
            return;

        if (isNeedToRemove)
            return;

        _timer += Time.deltaTime;
        if(_timer >= TotalTime)
        {
            isNeedToRemove = true;
        }
    }
}

public class TimerModiferData_Global : TimerModiferData
{
    private MTEC_AddGlobalTimerModifier _cfg;

    public override TimerModifierStackConfig StackCfg
    {
        get
        {
            return _cfg.StackConfig;
        }
    }

    public TimerModiferData_Global(ModifyTriggerData parent, MTEC_AddGlobalTimerModifier cfg, uint uid) : base(parent, cfg.DurationTime, cfg.UseDuration, uid)
    {
        this._cfg = cfg;
    }

    public override void OnAdded()
    {
        base.OnAdded();
        if(_cfg.ModifyMap != null)
        {
            foreach(var item in _cfg.ModifyMap)
            {
                RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(item.Key, PropertyModifyType.TempModify, UID, item.Value);
            }
        }
    }

    public override void OnRemove()
    {
        base.OnRemove();

        if (_cfg.ModifyMap != null)
        {
            foreach (var item in _cfg.ModifyMap)
            {
                RogueManager.Instance.MainPropertyData.RemovePropertyModifyValue(item.Key, PropertyModifyType.TempModify, UID, item.Value);
            }
        }
    }
}

public class TimerModiferData_Unit : TimerModiferData
{
    private MTEC_AddUnitTimerModifier _cfg;

    public Unit TargetUnit;

    public Action<Unit> OnRemoveAction;

    public override TimerModifierStackConfig StackCfg
    {
        get
        {
            return _cfg.StackConfig;
        }
    }

    public TimerModiferData_Unit(ModifyTriggerData parent, MTEC_AddUnitTimerModifier cfg, uint uid, Unit targetUnit) : base(parent, cfg.DurationTime, cfg.UseDuration, uid)
    {
        this._cfg = cfg;
        this.TargetUnit = targetUnit;
    }

    public override void OnAdded()
    {
        base.OnAdded();
        if (TargetUnit == null)
            return;

        if (_cfg.ModifyMap != null)
        {
            foreach (var item in _cfg.ModifyMap)
            {
                TargetUnit.LocalPropetyData.AddPropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID, item.Value);
            }
        }
    }

    public override void OnRemove()
    {
        base.OnRemove();
        if (TargetUnit == null)
            return;

        if (_cfg.ModifyMap != null)
        {
            foreach (var item in _cfg.ModifyMap)
            {
                TargetUnit.LocalPropetyData.RemovePropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID);
            }
        }
        OnRemoveAction?.Invoke(TargetUnit);
    }
}
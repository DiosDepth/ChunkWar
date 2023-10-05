using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ��ʱModifer
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

    public uint TargetUnitUID;

    public TimerModiferData_Unit(ModifyTriggerData parent, MTEC_AddUnitTimerModifier cfg, uint uid, uint targetUnitID) : base(parent, cfg.DurationTime, cfg.UseDuration, uid)
    {
        this._cfg = cfg;
        this.TargetUnitUID = targetUnitID;
    }

    public override void OnAdded()
    {
        base.OnAdded();
        var targetUnit = RogueManager.Instance.GetPlayerShipUnit(TargetUnitUID);
        if (targetUnit == null)
            return;

        if (_cfg.ModifyMap != null)
        {
            foreach (var item in _cfg.ModifyMap)
            {
                targetUnit.LocalPropetyData.AddPropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID, item.Value);
            }
        }
    }

    public override void OnRemove()
    {
        base.OnRemove();
        var targetUnit = RogueManager.Instance.GetPlayerShipUnit(TargetUnitUID);
        if (targetUnit == null)
            return;

        if (_cfg.ModifyMap != null)
        {
            foreach (var item in _cfg.ModifyMap)
            {
                targetUnit.LocalPropetyData.RemovePropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID);
            }
        }
    }
}
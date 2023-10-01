using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public TimerModiferData(float totalTime, uint uid)
    {
        this.TotalTime = totalTime;
        this.UID = uid;
        _timer = 0;
    }

    public virtual void OnAdded()
    {

    }

    public virtual void OnRemove()
    {

    }

    public void OnUpdate()
    {
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

    public TimerModiferData_Global(MTEC_AddGlobalTimerModifier cfg, uint uid) : base(cfg.DurationTime, uid)
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
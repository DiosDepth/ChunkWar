using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModifyTriggerData : IPropertyModify
{
    public ModifyTriggerConfig Config;
    public uint UID { get; set; }
    public PropertyModifyCategory Category
    {
        get
        {
            return PropertyModifyCategory.ModifyTrigger;
        }
    }

    protected int currentTriggerCount;

    public ModifyTriggerData(ModifyTriggerConfig cfg, uint uid)
    {
        this.Config = cfg;
        this.UID = uid;
    }

    /// <summary>
    /// ´´½¨Trigger
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public static ModifyTriggerData CreateTrigger(ModifyTriggerConfig cfg, uint uid)
    {
        switch (cfg.TriggerType)
        {
            case ModifyTriggerType.OnKillEnemy:
                return new MT_ShipDie(cfg, uid);
            case ModifyTriggerType.PropertyTransfer:
                return new MT_PropertyTransfer(cfg, uid);
        }

        Debug.LogError("ModifyTriggerData Create Error ! Type = " + cfg.TriggerType);
        return null;
    }

    public virtual void OnTriggerAdd()
    {

    }

    public virtual void OnTriggerRemove()
    {

    }

    public virtual void OnUpdateBattle()
    {

    }

    public virtual void Reset()
    {

    }

    protected virtual void Trigger()
    {
        currentTriggerCount++;
        var effects = Config.Effects;
        for(int i = 0; i < effects.Length; i++)
        {
            effects[i].Excute();
        }
    }

    private void ClearTrigger()
    {

    }

}

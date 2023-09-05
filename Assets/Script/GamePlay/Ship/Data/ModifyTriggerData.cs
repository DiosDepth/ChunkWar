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
        }

        Debug.LogError("ModifyTriggerData Create Error ! Type = " + cfg.TriggerType);
        return null;
    }

    public virtual void OnTriggerAdd()
    {

    }

    public virtual void OnUpdateBattle()
    {

    }

    public virtual void Trigger()
    {

    }

    private void ClearTrigger()
    {

    }

}

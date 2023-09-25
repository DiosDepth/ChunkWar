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
    /// 创建Trigger
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
            case ModifyTriggerType.ItemTransfer:
                return new MT_ItemTransfer(cfg, uid);
            case ModifyTriggerType.OnWaveEnd:
            case ModifyTriggerType.OnWaveStart:
                return new MT_WaveState(cfg, uid);
            case ModifyTriggerType.Timer:
                return new MT_Timer(cfg, uid);
            case ModifyTriggerType.OnAdd:
                return new MT_OnAdd(cfg, uid);
            case ModifyTriggerType.OnPlayerShipMove:
                return new MT_PlayerShipMove(cfg, uid);
            case ModifyTriggerType.OnRefreshShop:
                return new MT_OnShopRefresh(cfg, uid);
            case ModifyTriggerType.ItemRarityCount:
                return new MT_ItemRarityCount(cfg, uid);
            case ModifyTriggerType.OnEnterHarbor:
                return new MT_OnEnterHarbor(cfg, uid);
            case ModifyTriggerType.OnShieldRecover:
                return new MT_OnShieldRecover(cfg, uid);

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
        if (currentTriggerCount <= 0 && currentTriggerCount >= Config.TriggerCount)
            return;

        currentTriggerCount++;
        var effects = Config.Effects;
        for(int i = 0; i < effects.Length; i++)
        {
            effects[i].Excute(this);
        }
    }

    /// <summary>
    /// 生效触发，仅用于来回触发的情景
    /// </summary>
    protected void EffectTrigger()
    {
        var effects = Config.Effects;
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].Excute(this);
        }
    }

    protected void UnEffectTrigger()
    {
        var effects = Config.Effects;
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].UnExcute(this);
        }
    }

}

public class MT_OnAdd : ModifyTriggerData
{
    public MT_OnAdd(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {

    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        Trigger();
    }
}

/// <summary>
/// 商店刷新
/// </summary>
public class MT_OnShopRefresh : ModifyTriggerData
{
    public MT_OnShopRefresh(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {

    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnShopRefresh += OnShopRefresh;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        RogueManager.Instance.OnShopRefresh -= OnShopRefresh;   
    }

    private void OnShopRefresh(int refreshCount)
    {
        Trigger();
    }
}

public class MT_OnEnterHarbor : ModifyTriggerData
{
    public MT_OnEnterHarbor(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {

    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnEnterHarbor += OnEnterHarbor;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        RogueManager.Instance.OnEnterHarbor -= OnEnterHarbor;
    }

    private void OnEnterHarbor()
    {
        Trigger();
    }
}
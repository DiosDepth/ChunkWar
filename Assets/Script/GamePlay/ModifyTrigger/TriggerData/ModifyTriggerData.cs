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

    private List<TimerModiferData> _timerModifiers;

    public ModifyTriggerData(ModifyTriggerConfig cfg, uint uid)
    {
        this.Config = cfg;
        this.UID = uid;
        _timerModifiers = new List<TimerModiferData>();
    }

    /// <summary>
    /// ����Trigger
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
            case ModifyTriggerType.OnWeaponHitTarget:
                return new MT_OnWeaponHitTarget(cfg, uid);
            case ModifyTriggerType.OnPlayerWeaponReload:
                return new MT_OnWeaponReload(cfg, uid);
            case ModifyTriggerType.OnPlayerWeaponFire:
                return new MT_OnWeaponFire(cfg, uid);

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
        for (int i = _timerModifiers.Count - 1; i >= 0; i--) 
        {
            var modifier = _timerModifiers[i];
            modifier.OnUpdate();
            if (modifier.IsNeedToRemove)
            {
                modifier.OnRemove();
                _timerModifiers.RemoveAt(i);
            }
        }
    }

    public virtual void Reset()
    {

    }

    #region Function

    public void AddTimerModifier_Global(MTEC_AddGlobalTimerModifier config)
    {
        TimerModiferData_Global modifer = new TimerModiferData_Global(config, UID);
        modifer.OnAdded();
        _timerModifiers.Add(modifer);
    }

    public void AddTimerModifier_Unit(MTEC_AddUnitTimerModifier config, uint parentUnitUID)
    {
        TimerModiferData_Unit modifier = new TimerModiferData_Unit(config, UID, parentUnitUID);
        modifier.OnAdded();
        _timerModifiers.Add(modifier);
    }

    public void RemoveTimerModifier_Global(MTEC_AddGlobalTimerModifier config)
    {
        ///�Ƴ�һ��
        for(int i = _timerModifiers.Count - 1; i >=0; i--)
        {
            var modifier = _timerModifiers[i];
            if (modifier is TimerModiferData_Global)
            {
                modifier.OnRemove();
                _timerModifiers.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveTimerModifier_Unit(MTEC_AddUnitTimerModifier config, uint parentUnitUID)
    {
        ///�Ƴ�һ��
        for (int i = _timerModifiers.Count - 1; i >= 0; i--)
        {
            var modifier = _timerModifiers[i];
            if (modifier is TimerModiferData_Unit && (modifier as TimerModiferData_Unit).TargetUnitUID == parentUnitUID) 
            {
                modifier.OnRemove();
                _timerModifiers.RemoveAt(i);
                break;
            }
        }
    }

    #endregion

    protected virtual bool Trigger(uint parentUnitID = 0)
    {
        if (currentTriggerCount <= 0 && currentTriggerCount >= Config.TriggerCount)
            return false;

        if (Config.UsePercent && !Utility.RandomResult(0, Config.Percent)) 
        {
            ///Percent not vaild
            return false;
        }

        currentTriggerCount++;
        var effects = Config.Effects;
        for(int i = 0; i < effects.Length; i++)
        {
            effects[i].Excute(this, parentUnitID);
        }
        return true;
    }

    /// <summary>
    /// ��Ч���������������ش������龰
    /// </summary>
    protected void EffectTrigger(uint parentUnitID = 0)
    {
        var effects = Config.Effects;
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].Excute(this, parentUnitID);
        }
    }

    protected void UnEffectTrigger(uint parentUnitID = 0)
    {
        var effects = Config.Effects;
        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].UnExcute(this, parentUnitID);
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
/// �̵�ˢ��
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
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

    public string GetName
    {
        get { return string.Empty; }
    }

    protected int currentTriggerCount;

    /// <summary>
    /// 是否来源于Unit网格效果
    /// </summary>
    public bool FromUnitSlotEffect = false;

    /// <summary>
    /// 加成正在生效
    /// </summary>
    public bool _uniqueEffecting = false;

    private List<TimerModiferData> _timerModifiers;

    public ModifyTriggerData(ModifyTriggerConfig cfg, uint uid)
    {
        this.Config = cfg;
        this.UID = uid;
        _timerModifiers = new List<TimerModiferData>();
    }

    public virtual void OnTriggerAdd()
    {

    }

    public virtual void OnTriggerRemove()
    {

    }

    public virtual void OnUpdateBattle()
    {
        if (_timerModifiers.Count <= 0)
            return;

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
        _uniqueEffecting = false;
    }

    #region Function

    public void AddTimerModifier_Global(MTEC_AddGlobalTimerModifier config)
    {
        TimerModiferData_Global modifer = new TimerModiferData_Global(this, config, UID);
        modifer.OnAdded();
        _timerModifiers.Add(modifer);
    }

    public void AddTimerModifier_Unit(MTEC_AddUnitTimerModifier config, uint parentUnitUID)
    {
        TimerModiferData_Unit modifier = new TimerModiferData_Unit(this, config, UID, parentUnitUID);
        modifier.OnAdded();
        _timerModifiers.Add(modifier);
    }

    public void AddModifier_Unit(MTEC_AddUnitModifier config, uint parentUnitUID)
    {
        var targetUnit = RogueManager.Instance.GetPlayerShipUnit(parentUnitUID);
        if (targetUnit == null)
            return;

        if (config.ModifyMap != null)
        {
            foreach (var item in config.ModifyMap)
            {
                targetUnit.LocalPropetyData.AddPropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID, item.Value);
            }
        }
    }

    public void RemoveTimerModifier_Global(MTEC_AddGlobalTimerModifier config)
    {
        ///移除一个
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
        ///移除一个
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

    public void RemoveModifier_Unit(MTEC_AddUnitModifier config, uint parentUnitUID)
    {
        var targetUnit = RogueManager.Instance.GetPlayerShipUnit(parentUnitUID);
        if (targetUnit == null)
            return;

        if (config.ModifyMap != null)
        {
            foreach (var item in config.ModifyMap)
            {
                targetUnit.LocalPropetyData.RemovePropertyModifyValue(item.Key, LocalPropertyModifyType.Modify, UID);
            }
        }
    }

    /// <summary>
    /// 创建爆炸
    /// </summary>
    /// <param name="config"></param>
    /// <param name="parentUnitUID"></param>
    public void CreateExplode(MTEC_CreateExplode config, uint parentUnitUID)
    {
        bool createSuccess = false;

        ///Calculate Damage
        bool isCritial = false;
        var damage = GameHelper.CalculatePlayerDamageWithModify(config.ExplodeDamageBase, config.DamageModifyFrom, out isCritial);
        var explodeRange = GameHelper.CalculateExplodeRange(config.ExplodeRange);

        Vector3 targetHitpoint = Vector3.zero;
        if (config.PointTarget == PointTargetType.HitPoint)
        {
            var data = this as MT_OnWeaponHitTarget;
            if (data == null || data.DamageInfo == null)
                return;

            targetHitpoint = data.DamageInfo.HitPoint;
           
        }
        else if(config.PointTarget == PointTargetType.RandomEnemyUnitPoint)
        {
            var randomEnemyUnit = ECSManager.Instance.GetRandomUnitList(OwnerType.AI, 1);
            if (randomEnemyUnit.Count <= 0)
                return;

            targetHitpoint = randomEnemyUnit[0].transform.position;
        }

        ///创造爆炸
        var allaiPos = ECSManager.Instance.activeAIUnitData.unitPos;
        var targetInfos = GameHelper.FindTargetsByPoint(targetHitpoint, explodeRange, allaiPos);
        var allUnits = ECSManager.Instance.GetActiveUnitReferenceByTargetInfo(OwnerType.AI, targetInfos);

        for (int i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i].reference;
            if (unit != null)
            {
                ///Create Damage
                DamageResultInfo info = new DamageResultInfo()
                {
                    attackerUnit = null,
                    Target = unit,
                    Damage = damage,
                    IsCritical = isCritial,
                    DamageType = WeaponDamageType.NONE,
                    IsPlayerAttack = true
                };
                (unit as IDamageble).TakeDamage(info);
            }
        }

        LevelManager.Instance.PlayerCreateExplode();
    }

    public void CreateDamage(MTEC_CreateDamage config, uint parentUnitID)
    {
        ///Damage
        bool isCritial = false;
        var damage = GameHelper.CalculatePlayerDamageWithModify(config.Damage, config.DamageModifyFrom, out isCritial);
        
        if (config.TargetType == EffectDamageTargetType.Target)
        {
        
            var targetUnit = ECSManager.Instance.GetUnitByUID(OwnerType.AI, parentUnitID);
            if (targetUnit == null)
                return;

            DamageResultInfo info = new DamageResultInfo()
            {
                attackerUnit = null,
                Target = targetUnit,
                Damage = damage,
                IsCritical = isCritial,
                DamageType = WeaponDamageType.NONE,
                IsPlayerAttack = true
            };

            (targetUnit as IDamageble).TakeDamage(info);
        }
        else if(config.TargetType == EffectDamageTargetType.All)
        {
            var allUnit = ECSManager.Instance.activeAIUnitData.unitList;
            for(int i = 0; i < allUnit.Count; i++)
            {
                var unit = allUnit[i];
                if (unit == null)
                    continue;

                DamageResultInfo info = new DamageResultInfo()
                {
                    attackerUnit = null,
                    Target = unit,
                    Damage = damage,
                    IsCritical = isCritial,
                    DamageType = WeaponDamageType.NONE,
                    IsPlayerAttack = true
                };

                (unit as IDamageble).TakeDamage(info);
            }
        }
        else if (config.TargetType == EffectDamageTargetType.Random)
        {
            ///随机敌人
            var randomUnit = ECSManager.Instance.GetRandomUnitList(OwnerType.AI, config.RandomTargetCount);
            for (int i = 0; i < randomUnit.Count; i++)
            {
                var unit = randomUnit[i];
                if (unit == null)
                    continue;

                DamageResultInfo info = new DamageResultInfo()
                {
                    attackerUnit = null,
                    Target = unit,
                    Damage = damage,
                    IsCritical = isCritial,
                    DamageType = WeaponDamageType.NONE,
                    IsPlayerAttack = true
                };

                (unit as IDamageble).TakeDamage(info);
            }
        }
        else if (config.TargetType == EffectDamageTargetType.TargetSortByMaxHP)
        {
            ///最大HP 排序X位
            var targetUnits = ECSManager.Instance.GetUnitsWithCondition(OwnerType.AI, FindCondition.MaximumHP, config.RandomTargetCount);
            //var targetUnits = AIManager.Instance.GetAIUnitsWithCondition(FindCondition.MaximumHP, config.RandomTargetCount);
            if (targetUnits == null || targetUnits.Count <= 0)
                return;

            for (int i = 0; i < targetUnits.Count; i++)
            {
                var unit = targetUnits[i];
                if (unit == null)
                    continue;

                DamageResultInfo info = new DamageResultInfo()
                {
                    attackerUnit = null,
                    Target = unit,
                    Damage = damage,
                    IsCritical = isCritial,
                    DamageType = WeaponDamageType.NONE,
                    IsPlayerAttack = true
                };

                (unit as IDamageble).TakeDamage(info);
            }
        }
    }

    public void ModifyDamageByTargetDistance(MTEC_ModifyDamgeByTargetDistance cfg, uint targetUID)
    {
        if (Config.TriggerType != ModifyTriggerType.OnWeaponHitTarget)
            return;

        var damageData = (this as MT_OnWeaponHitTarget).DamageInfo;
        if (damageData == null)
            return;

        var distance = GameHelper.GetUnitDistance(damageData.Target, damageData.attackerUnit);
        var per = distance * 10 / (float)cfg.DistancePer;
        var percent = Mathf.RoundToInt(per * cfg.DamagePercent) / 100f;

        percent = Mathf.Clamp(percent, -100f, cfg.DamagePercentMax);

        var damage = damageData.Damage * (1 + percent / 100f);
        damageData.Damage = Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// 治疗Unit
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="targetUID"></param>
    public void HealUnitHP(MTEC_HealUnitHP cfg, uint targetUID)
    {
        if(cfg.TargetType == MTEC_HealUnitHP.HealTargetType.AllPlayerUnit)
        {
            var allPlayerUnits = RogueManager.Instance.currentShip.UnitList;
            for(int i = 0; i < allPlayerUnits.Count; i++)
            {
                var target = allPlayerUnits[i];
                if(target != null)
                {
                    target.Heal((int)cfg.HealValue);
                }
            }
        }
    }

    #endregion

    #region Condition

    public bool ByWasteCount(MCC_ByWasteCount cfg)
    {
        var currentWasteCount = RogueManager.Instance.GetDropWasteCount;
        return MathExtensionTools.CalculateCompareType(currentWasteCount, cfg.Value, cfg.Compare);
    }


    #endregion

    protected virtual bool Trigger(uint parentUnitID = 0)
    {
        if (Config.TriggerCount > 0 && (currentTriggerCount <= 0 && currentTriggerCount >= Config.TriggerCount))
            return false;

        if (Config.UsePercent) 
        {
            var rowPercent = Config.Percent;
            if (Config.UseLuckModify)
            {
                ///使用幸运比例修正
                var luckValue = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Luck);
                var addPercent = luckValue * Config.LuckModifyRate;
                rowPercent += addPercent;
            }

            ///Percent not vaild
            if (!Utility.CalculateRate100(rowPercent))
                return false;
        }

        ///加成唯一并且正在生效
        if (Config.SelfUnique && _uniqueEffecting)
            return false;

        ///CheckCondition
        var effects = Config.Effects;
        for(int i = 0; i < effects.Length; i++)
        {
            bool trigger = false;
            var effect = effects[i];
            if (CheckCondition(effect)) ///有一个触发即为触发
            {
                trigger = true;
                effect.Excute(this, parentUnitID);
            }

            if (trigger)
            {
                currentTriggerCount++;
            }
        }
        return true;
    
    }

    /// <summary>
    /// 检测条件
    /// </summary>
    /// <param name="effectCfg"></param>
    /// <returns></returns>
    private bool CheckCondition(ModifyTriggerEffectConfig effectCfg)
    {
        if (effectCfg.Conditions == null || effectCfg.Conditions.Length <= 0)
            return true;

        bool result = true;
        for(int i = 0; i < effectCfg.Conditions.Length; i++)
        {
            result &= effectCfg.Conditions[i].GetResult(this);
        }
        return result;
    }

    /// <summary>
    /// 生效触发，仅用于来回触发的情景
    /// </summary>
    protected void EffectTrigger(uint parentUnitID = 0)
    {
        var effects = Config.Effects;
        for (int i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (CheckCondition(effect))
            {
                effect.Excute(this, parentUnitID);
            }
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

public class MT_OnEnterShop : ModifyTriggerData
{
    public MT_OnEnterShop(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {

    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnEnterShop += OnEnterShop;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        RogueManager.Instance.OnEnterShop -= OnEnterShop;
    }

    private void OnEnterShop()
    {
        Trigger();
    }
}

public class MT_OnBuyShopItem : ModifyTriggerData
{
    private MTC_OnBuyShopItem _buyCfg;

    public MT_OnBuyShopItem(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _buyCfg = cfg as MTC_OnBuyShopItem;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnBuyShopItem += OnBuyItem;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        RogueManager.Instance.OnBuyShopItem -= OnBuyItem;
    }

    private void OnBuyItem(int targetGoodsID)
    {
        if (_buyCfg.UseSpecialShopItemID)
        {
            if (targetGoodsID != _buyCfg.ShopItemID)
                return;
        }

        Trigger();
    }
}

public class MT_OnCollectPickable : ModifyTriggerData
{
    private MTC_OnCollectPickable _pickCfg;

    public MT_OnCollectPickable(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _pickCfg = cfg as MTC_OnCollectPickable;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        LevelManager.Instance.OnCollectPickUp += OnCollectPickable;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        LevelManager.Instance.OnCollectPickUp -= OnCollectPickable;
    }

    private void OnCollectPickable(AvaliablePickUp type)
    {
        if (_pickCfg.PickUpType != type)
        {
            return;
        }

        Trigger();
    }
}
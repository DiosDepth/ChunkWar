using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 修正触发效果
/// </summary>
[System.Serializable]
public abstract class ModifyTriggerEffectConfig 
{
    [ReadOnly]
    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(50)]
    public ModifyTriggerEffectType EffectType;

    [LabelText("条件")]
    [HideReferenceObjectPicker]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetConditionList", DrawDropdownForListElements = false)]
    public ModifyConditionConfig[] Conditions = new ModifyConditionConfig[0];

    public ModifyTriggerEffectConfig(ModifyTriggerEffectType type)
    {
        this.EffectType = type;
    }

    public abstract void Excute(ModifyTriggerData data, uint parentUnitUID);
    public abstract void UnExcute(ModifyTriggerData data, uint parentUnitUID);

    public static ValueDropdownList<ModifyTriggerEffectConfig> GetModifyEffectTriggerList()
    {
        ValueDropdownList<ModifyTriggerEffectConfig> result = new ValueDropdownList<ModifyTriggerEffectConfig>();
        foreach (ModifyTriggerEffectType type in System.Enum.GetValues(typeof(ModifyTriggerEffectType)))
        {
            if (type == ModifyTriggerEffectType.AddPropertyValue)
            {
                result.Add(type.ToString(), new MTEC_AddPropertyValue(type));
            }
            else if (type == ModifyTriggerEffectType.SetPropertyMaxValue)
            {
                result.Add(type.ToString(), new MTEC_SetPropertyMaxValue(type));
            }
            else if (type == ModifyTriggerEffectType.SetPropertyValue)
            {
                result.Add(type.ToString(), new MTEC_SetPropertyValue(type));
            }
            else if (type == ModifyTriggerEffectType.TempReduceShopPrice)
            {
                result.Add(type.ToString(), new MTEC_TempReduceShopPrice(type));
            }
            else if (type == ModifyTriggerEffectType.AddPropertyValueBySpecialCount)
            {
                result.Add(type.ToString(), new MTEC_AddPropertyValueBySpecialCount(type));
            }
            else if (type == ModifyTriggerEffectType.SetUnitPropertyModifyKey)
            {
                result.Add(type.ToString(), new MTEC_SetUnitPropertyValue(type));
            }
            else if (type == ModifyTriggerEffectType.AddGlobalTimerModifier)
            {
                result.Add(type.ToString(), new MTEC_AddGlobalTimerModifier(type));
            }
            else if (type == ModifyTriggerEffectType.AddUnitTimerModifier)
            {
                result.Add(type.ToString(), new MTEC_AddUnitTimerModifier(type));
            }
            else if (type == ModifyTriggerEffectType.EnterUnitState)
            {
                result.Add(type.ToString(), new MTEC_AddUnitTimerModifier(type));
            }
            else if(type == ModifyTriggerEffectType.GainDropWaste)
            {
                result.Add(type.ToString(), new MTEC_GainDropWaste(type));
            }
            else if (type == ModifyTriggerEffectType.CreateExplode)
            {
                result.Add(type.ToString(), new MTEC_CreateExplode(type));
            }
            else if (type == ModifyTriggerEffectType.CreateDamage)
            {
                result.Add(type.ToString(), new MTEC_CreateDamage(type));
            }
            else if(type == ModifyTriggerEffectType.GainCurrency)
            {
                result.Add(type.ToString(), new MTEC_GainCurrency(type));
            }
            else if (type == ModifyTriggerEffectType.ModifyDamgeByTargetDistance)
            {
                result.Add(type.ToString(), new MTEC_ModifyDamgeByTargetDistance(type));
            }
            else if(type == ModifyTriggerEffectType.AddUnitModifier)
            {
                result.Add(type.ToString(), new MTEC_AddUnitModifier(type));
            }
            else if(type == ModifyTriggerEffectType.HealUnitHP)
            {
                result.Add(type.ToString(), new MTEC_HealUnitHP(type));
            }
            else if(type == ModifyTriggerEffectType.AddAISpawn)
            {
                result.Add(type.ToString(), new MTEC_AddAISpawn(type));
            }
            else if(type == ModifyTriggerEffectType.ActiveEnemyExtraUnitGroup)
            {
                result.Add(type.ToString(), new MTEC_ActiveEnemyUnitGroup(type));
            }
        }

        return result;
    }

    private static ValueDropdownList<ModifyConditionConfig> GetConditionList()
    {
        return ModifyConditionConfig.GetModifyConditionList();
    }
}

public  class MTEC_SetPropertyValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    public MTEC_SetPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        ///使用特殊数量修正
        if (data is MT_ItemRarityCount)
        {
            var itemData = data as MT_ItemRarityCount;
            int count = itemData.ItemCount;
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value * count);

            return;
        }

        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        RogueManager.Instance.MainPropertyData.RemovePropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
    }
}

public class MTEC_AddPropertyValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    public MTEC_AddPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        ///使用特殊数量修正
        if(data is MT_ItemRarityCount)
        {
            var itemData = data as MT_ItemRarityCount;
            int count = itemData.ItemCount;
            RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value * count);

            return;
        }

        RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        RogueManager.Instance.MainPropertyData.RemovePropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
    }
}

public class MTEC_SetPropertyMaxValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public float Value;

    public MTEC_SetPropertyMaxValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        RogueManager.Instance.MainPropertyData.SetPropertyMaxValue(ModifyKey, Value);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_TempReduceShopPrice : ModifyTriggerEffectConfig
{
    /// <summary>
    /// 如果为0，则所有的都生效
    /// </summary>
    [HorizontalGroup("AA", 120)]
    [LabelText("生效数量")]
    [LabelWidth(40)]
    public byte ReduceCount;

    [HorizontalGroup("AA", 120)]
    [LabelText("打折比例")]
    [LabelWidth(40)]
    public byte DiscountValue;

    public MTEC_TempReduceShopPrice(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        if(ReduceCount == 0)
        {
            var allcurrentItems = RogueManager.Instance.CurrentRogueShopItems;
            for(int i = 0; i < allcurrentItems.Count; i++)
            {
                allcurrentItems[i].SetDiscountValue(DiscountValue);
            }
        }
        else
        {
            var randomItems = RogueManager.Instance.GetRandomCurrentRogueShopItems(ReduceCount);
            if(randomItems != null && randomItems.Count > 0)
            {
                for(int i = 0; i < randomItems.Count; i++)
                {
                    randomItems[i].SetDiscountValue(DiscountValue);
                }
            }
        }
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_AddPropertyValueBySpecialCount : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    [HorizontalGroup("AA", 150)]
    [LabelText("特殊Key")]
    [LabelWidth(40)]
    public string SpecialKey;

    public MTEC_AddPropertyValueBySpecialCount(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        int targetCount = 0;
        switch (SpecialKey)
        {
            case "WreckageCount":
                targetCount = RogueManager.Instance.GetInLevelTotalWreckageDropCount();
                break;
        }

        float tagetValue = Value * targetCount;
        RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(ModifyKey, ModifyType, data.UID, tagetValue);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

/// <summary>
/// 设置Unit本地属性
/// </summary>
public class MTEC_SetUnitPropertyValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public UnitPropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(40)]
    public LocalPropertyModifyType ModifyType;

    public MTEC_SetUnitPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        Unit targetUnit = null;
        if(parentUnitUID != 0)
        {
            targetUnit = RogueManager.Instance.GetPlayerShipUnit(parentUnitUID);
        }

        if(targetUnit == null)
        {
            Debug.LogError("SetUnitPropertyValue Null! Unit Null!");
            return;
        }

        targetUnit.LocalPropetyData.SetPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);

    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        Unit targetUnit = null;
        if (parentUnitUID != 0)
        {
            targetUnit = RogueManager.Instance.GetPlayerShipUnit(parentUnitUID);
        }

        if (targetUnit == null)
        {
            Debug.LogError("UnSetUnitPropertyValue Null! Unit Null!");
            return;
        }

        targetUnit.LocalPropetyData.RemovePropertyModifyValue(ModifyKey, ModifyType, data.UID);
    }
}

/// <summary>
/// 全局计时属性
/// </summary>
public class MTEC_AddGlobalTimerModifier : ModifyTriggerEffectConfig
{
    [HideReferenceObjectPicker]
    [LabelText("叠加规则")]
    public TimerModifierStackConfig StackConfig = new TimerModifierStackConfig();

    [DictionaryDrawerSettings()]
    public Dictionary<PropertyModifyKey, float> ModifyMap = new Dictionary<PropertyModifyKey, float>();

    [HorizontalGroup("AA", 200)]
    [LabelText("使用时间")]
    [LabelWidth(80)]
    public bool UseDuration = true;

    [HorizontalGroup("AA", 200)]
    [LabelText("持续时间")]
    [LabelWidth(80)]
    [ShowIf("UseDuration")]
    public float DurationTime;

    public MTEC_AddGlobalTimerModifier(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.AddTimerModifier_Global(this);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.RemoveTimerModifier_Global(this);
    }
}

public enum TimerModifierStackType
{
    RefreshTime,
}

[System.Serializable]
public class TimerModifierStackConfig
{
    [LabelWidth(200)]
    public bool GlobalUnique = false;

    [LabelWidth(200)]
    public string UniqueKey;

    [LabelWidth(200)]
    public TimerModifierStackType StackType;

    [LabelText("覆盖优先级")]
    [LabelWidth(200)]
    public int StackOrder;

}

public class MTEC_AddUnitTimerModifier : ModifyTriggerEffectConfig
{
    [HideReferenceObjectPicker]
    [LabelText("叠加规则")]
    public TimerModifierStackConfig StackConfig;

    [DictionaryDrawerSettings()]
    public Dictionary<UnitPropertyModifyKey, float> ModifyMap = new Dictionary<UnitPropertyModifyKey, float>();

    [HorizontalGroup("AA", 140)]
    [LabelText("使用时间")]
    [LabelWidth(80)]
    public bool UseDuration = true;

    [HorizontalGroup("AA", 200)]
    [LabelText("持续时间")]
    [LabelWidth(80)]
    [ShowIf("UseDuration")]
    public float DurationTime;

    public MTEC_AddUnitTimerModifier(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.AddTimerModifier_Unit(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.RemoveTimerModifier_Unit(this, parentUnitUID);
    }
}

public class MTEC_AddUnitModifier : ModifyTriggerEffectConfig
{
    [DictionaryDrawerSettings()]
    public Dictionary<UnitPropertyModifyKey, float> ModifyMap = new Dictionary<UnitPropertyModifyKey, float>();

    public MTEC_AddUnitModifier(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.AddModifier_Unit(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.RemoveModifier_Unit(this, parentUnitUID);
    }
}

public class MTEC_GainDropWaste : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public int Value;

    [HorizontalGroup("AA", 200)]
    [LabelText("增加百分比")]
    [LabelWidth(80)]
    public bool GainPercent;

    [HorizontalGroup("AA", 120)]
    [LabelText("百分比")]
    [LabelWidth(40)]
    [ShowIf("GainPercent")]
    public float PercentValue;

    public MTEC_GainDropWaste(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        if (GainPercent)
        {

        }
        else
        {
            RogueManager.Instance.AddDropWasteCount(Value);
        }
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_GainCurrency : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 120)]
    [LabelText("值")]
    [LabelWidth(40)]
    public int Value;

    public MTEC_GainCurrency(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        RogueManager.Instance.AddCurrency(Value);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public enum PointTargetType
{
    NONE,
    HitPoint,
    RandomEnemyUnitPoint,
}

public class MTEC_CreateExplode : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 300)]
    [LabelText("点类型")]
    [LabelWidth(40)]
    public PointTargetType PointTarget;

    [HorizontalGroup("AA", 120)]
    [LabelText("爆炸范围")]
    [LabelWidth(60)]
    public int ExplodeRange;

    [HorizontalGroup("AA", 120)]
    [LabelText("爆炸伤害")]
    [LabelWidth(60)]
    public int ExplodeDamageBase;

    [HorizontalGroup("AD", 800)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();


    public MTEC_CreateExplode(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.CreateExplode(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public enum EffectDamageTargetType
{
    All,
    Target,
    Random,
    TargetSortByMaxHP,
}

public class MTEC_CreateDamage : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 300)]
    [LabelText("目标类型")]
    [LabelWidth(80)]
    public EffectDamageTargetType TargetType;

    [HorizontalGroup("AA", 120)]
    [LabelText("伤害")]
    [LabelWidth(60)]
    public int Damage;

    [HorizontalGroup("AA", 150)]
    [LabelText("目标个数")]
    [LabelWidth(80)]
    public int RandomTargetCount = 1;

    [HorizontalGroup("AD", 800)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();

    public MTEC_CreateDamage(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.CreateDamage(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_ModifyDamgeByTargetDistance : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 120)]
    [LabelText("距离")]
    [LabelWidth(40)]
    public int DistancePer;

    [HorizontalGroup("AA", 120)]
    [LabelText("伤害")]
    [LabelWidth(40)]
    public float DamagePercent;

    [HorizontalGroup("AA", 120)]
    [LabelText("最大百分比")]
    [LabelWidth(40)]
    public float DamagePercentMax;

    public MTEC_ModifyDamgeByTargetDistance(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.ModifyDamageByTargetDistance(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_HealUnitHP : ModifyTriggerEffectConfig
{
    public enum HealTargetType
    {
        Target,
        AllPlayerUnit
    }

    [LabelText("目标")]
    [LabelWidth(120)]
    [HorizontalGroup("AA", 300)]
    public HealTargetType TargetType;

    [LabelText("治疗值")]
    [LabelWidth(120)]
    [HorizontalGroup("AA", 300)]
    public float HealValue;

    public MTEC_HealUnitHP(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.HealUnitHP(this, parentUnitUID);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

[System.Serializable]
public class AttachPointConfig
{
    [LabelWidth(200)]
    public bool RandomNode;

    [HideIf("RandomNode")]
    [LabelWidth(200)]
    public string NodeName;
}

public class MTEC_AddAISpawn : ModifyTriggerEffectConfig
{
    public List<WaveEnemySpawnConfig> SpawnCfgList = new List<WaveEnemySpawnConfig>();

    [LabelText("位置使用创生AI船Node")]
    [LabelWidth(200)]
    public bool UseShipSpawnNode = false;

    [ShowIf("UseShipSpawnNode")]
    [HideReferenceObjectPicker]
    public AttachPointConfig PointCfg;

    public MTEC_AddAISpawn(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.AddAISpawn(this);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.RemoveAISpawn(this);
    }
}

public class MTEC_ActiveEnemyUnitGroup : ModifyTriggerEffectConfig
{
    [LabelWidth(200)]
    public string UnitGroupName;

    public MTEC_ActiveEnemyUnitGroup(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        data.ActiveEnemyUnitGroup(this);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}
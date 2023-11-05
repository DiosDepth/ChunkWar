using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModifyConditionType
{
    ByWasteCount,
    ByShopSellWasteCount,
}

[System.Serializable]
public abstract class ModifyConditionConfig 
{
    [ReadOnly]
    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(50)]
    public ModifyConditionType EffectType;

    public ModifyConditionConfig(ModifyConditionType type)
    {
        this.EffectType = type;
    }

    public static ValueDropdownList<ModifyConditionConfig> GetModifyConditionList()
    {
        ValueDropdownList<ModifyConditionConfig> result = new ValueDropdownList<ModifyConditionConfig>();
        foreach (ModifyConditionType type in System.Enum.GetValues(typeof(ModifyConditionType)))
        {
            if (type == ModifyConditionType.ByWasteCount)
            {
                result.Add(type.ToString(), new MCC_ByWasteCount(type));
            }
            else if(type == ModifyConditionType.ByShopSellWasteCount)
            {
                result.Add(type.ToString(), new MCC_ByShopSellWasteCount(type));
            }
        }

        return result;
    }

    public abstract bool GetResult(ModifyTriggerData Data);
}

public class MCC_ByWasteCount : ModifyConditionConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("比较")]
    [LabelWidth(40)]
    public CompareType Compare;

    [HorizontalGroup("AA", 200)]
    [LabelText("值")]
    [LabelWidth(40)]
    public int Value;

    public MCC_ByWasteCount(ModifyConditionType type) : base(type)
    {

    }

    public override bool GetResult(ModifyTriggerData Data)
    {
        return Data.ByWasteCount(this);
    }
}


public class MCC_ByShopSellWasteCount : ModifyConditionConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("比较")]
    [LabelWidth(40)]
    public CompareType Compare;

    [HorizontalGroup("AA", 200)]
    [LabelText("值")]
    [LabelWidth(40)]
    public int Value;

    public MCC_ByShopSellWasteCount(ModifyConditionType type) : base(type)
    {

    }

    public override bool GetResult(ModifyTriggerData Data)
    {
        return Data.ByShopSellWasteCount(this);
    }
}

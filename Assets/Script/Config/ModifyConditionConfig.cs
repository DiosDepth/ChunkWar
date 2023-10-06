using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModifyConditionType
{
    ByWasteCount,
}

[System.Serializable]
public abstract class ModifyConditionConfig 
{
    [ReadOnly]
    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
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
        }

        return result;
    }

    public abstract bool GetResult(ModifyTriggerData Data);
}

public class MCC_ByWasteCount : ModifyConditionConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("�Ƚ�")]
    [LabelWidth(40)]
    public CompareType Compare;

    [HorizontalGroup("AA", 200)]
    [LabelText("ֵ")]
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

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

    public ModifyTriggerEffectConfig(ModifyTriggerEffectType type)
    {
        this.EffectType = type;
    }

    public abstract void Excute(ModifyTriggerData data);

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
        }

        return result;
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

    public override void Excute(ModifyTriggerData data)
    {
        RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
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

    public override void Excute(ModifyTriggerData data)
    {
        RogueManager.Instance.MainPropertyData.SetPropertyMaxValue(ModifyKey, Value);
    }
}
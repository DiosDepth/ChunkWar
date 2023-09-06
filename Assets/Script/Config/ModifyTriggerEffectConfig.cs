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
    public ModifyTriggerEffectType EffectType;

    public ModifyTriggerEffectConfig(ModifyTriggerEffectType type)
    {
        this.EffectType = type;
    }

    public abstract void Excute();

    public static ValueDropdownList<ModifyTriggerEffectConfig> GetModifyEffectTriggerList()
    {
        ValueDropdownList<ModifyTriggerEffectConfig> result = new ValueDropdownList<ModifyTriggerEffectConfig>();
        foreach (ModifyTriggerEffectType type in System.Enum.GetValues(typeof(ModifyTriggerEffectType)))
        {
            if (type == ModifyTriggerEffectType.AddPropertyValue)
            {
                result.Add(type.ToString(), new MTEC_AddPropertyValue(type));
            }
        }

        return result;
    }
}

public class MTEC_AddPropertyValue : ModifyTriggerEffectConfig
{
    public MTEC_AddPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute()
    {

    }

}

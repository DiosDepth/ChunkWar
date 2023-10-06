using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitSlotEffectApplyTarget
{
    Target,
    Self
}

public class UnitSlotEffectConfig 
{
    [LabelText("主条件")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [HideReferenceObjectPicker]
    [ValueDropdown("GetConditions")]
    public UnitSlotEffectConditionConfig[] MainConditions = new UnitSlotEffectConditionConfig[0];

    [LabelText("目标")]
    [LabelWidth(80)]
    [HorizontalGroup("ZZ", 250)]
    public UnitSlotEffectApplyTarget ApplyTarget;

    [LabelText("应用效果")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [HideReferenceObjectPicker]
    [ValueDropdown("GetEffects")]
    public ModifyTriggerConfig[] Effects = new ModifyTriggerConfig[0];

    private ValueDropdownList<UnitSlotEffectConditionConfig> GetConditions()
    {
        return UnitSlotEffectConditionConfig.GetModifyConditionList();
    }

    private ValueDropdownList<ModifyTriggerEffectConfig> GetEffects()
    {
        return ModifyTriggerEffectConfig.GetModifyEffectTriggerList();
    }
}

public enum UnitSlotEffectConditionType
{
    ByTargetSlotUnitTag,
}

[System.Serializable]
public abstract class UnitSlotEffectConditionConfig
{
    [ReadOnly]
    [HorizontalGroup("AA", 150)]
    [LabelText("类型")]
    [LabelWidth(50)]
    public UnitSlotEffectConditionType EffectType;

    public UnitSlotEffectConditionConfig(UnitSlotEffectConditionType type)
    {
        this.EffectType = type;
    }

    public static ValueDropdownList<UnitSlotEffectConditionConfig> GetModifyConditionList()
    {
        ValueDropdownList<UnitSlotEffectConditionConfig> result = new ValueDropdownList<UnitSlotEffectConditionConfig>();
        foreach (UnitSlotEffectConditionType type in System.Enum.GetValues(typeof(UnitSlotEffectConditionType)))
        {
            if(type == UnitSlotEffectConditionType.ByTargetSlotUnitTag)
            {
                result.Add(type.ToString(), new USEC_ByTargetSlotUnitTag(type));
            }
        }

        return result;
    }

    public abstract bool GetResult(Unit targetUnit);
}

public class USEC_ByTargetSlotUnitTag : UnitSlotEffectConditionConfig
{
    public ItemTag ContainTag;

    public USEC_ByTargetSlotUnitTag(UnitSlotEffectConditionType type) : base(type)
    {
        this.EffectType = type;
    }

    public override bool GetResult(Unit targetUnit)
    {
        if (targetUnit._baseUnitConfig.HasUnitTag(ContainTag))
            return true;

        return false;
    }
}

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
    public ModifyTriggerConfig[] ApplyEffects = new ModifyTriggerConfig[0];

    [LabelText("属性修正")]
    [LabelWidth(80)]
    [DictionaryDrawerSettings()]
    public Dictionary<UnitPropertyModifyKey, float> ModifyMap = new Dictionary<UnitPropertyModifyKey, float>();

#if UNITY_EDITOR
    private ValueDropdownList<UnitSlotEffectConditionConfig> GetConditions()
    {
        return UnitSlotEffectConditionConfig.GetModifyConditionList();
    }

    private ValueDropdownList<ModifyTriggerConfig> GetEffects()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }

#endif
}

public enum UnitSlotEffectConditionType
{
    ByTargetSlotUnitTag,
    ByTargetUnitGroupID,
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
            else if(type == UnitSlotEffectConditionType.ByTargetUnitGroupID)
            {
                result.Add(type.ToString(), new USEC_ByTargetUnitGroupID(type));
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

public class USEC_ByTargetUnitGroupID : UnitSlotEffectConditionConfig
{
    public int[] VaildGroupIDs = new int[0];

    public USEC_ByTargetUnitGroupID(UnitSlotEffectConditionType type) : base(type)
    {
        this.EffectType = type;
    }

    public override bool GetResult(Unit targetUnit)
    {
        var unitGroupID = targetUnit._baseUnitConfig.GroupID;
        for(int i = 0; i < VaildGroupIDs.Length; i++)
        {
            if (VaildGroupIDs[i] == unitGroupID)
                return true;
        }

        return false;
    }
}
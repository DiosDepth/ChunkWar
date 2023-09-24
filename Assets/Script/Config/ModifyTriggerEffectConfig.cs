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
    public abstract void UnExcute(ModifyTriggerData data);

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
        }

        return result;
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

    public override void Excute(ModifyTriggerData data)
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

    public override void UnExcute(ModifyTriggerData data)
    {
        RogueManager.Instance.MainPropertyData.RemovePropertyModifyValue(ModifyKey, ModifyType, data.UID);
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

    public override void UnExcute(ModifyTriggerData data)
    {
        RogueManager.Instance.MainPropertyData.RemovePropertyModifyValue(ModifyKey, ModifyType, data.UID);
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

    public override void UnExcute(ModifyTriggerData data)
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

    public override void Excute(ModifyTriggerData data)
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

    public override void UnExcute(ModifyTriggerData data)
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

    public override void Excute(ModifyTriggerData data)
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

    public override void UnExcute(ModifyTriggerData data)
    {

    }
}
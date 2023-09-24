using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������Ч��
/// </summary>
[System.Serializable]
public abstract class ModifyTriggerEffectConfig 
{
    [ReadOnly]
    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
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
            else if (type == ModifyTriggerEffectType.SetPropertyValue)
            {
                result.Add(type.ToString(), new MTEC_SetPropertyValue(type));
            }
            else if (type == ModifyTriggerEffectType.TempReduceShopPrice)
            {
                result.Add(type.ToString(), new MTEC_TempReduceShopPrice(type));
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
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    public MTEC_SetPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data)
    {
        ///ʹ��������������
        if (data is MT_ItemRarityCount)
        {
            var itemData = data as MT_ItemRarityCount;
            int count = itemData.ItemCount;
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value * count);

            return;
        }

        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value);
    }

}

public class MTEC_AddPropertyValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    public MTEC_AddPropertyValue(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data)
    {
        ///ʹ��������������
        if(data is MT_ItemRarityCount)
        {
            var itemData = data as MT_ItemRarityCount;
            int count = itemData.ItemCount;
            RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(ModifyKey, ModifyType, data.UID, Value * count);

            return;
        }

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
    [LabelText("ֵ")]
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

public class MTEC_TempReduceShopPrice : ModifyTriggerEffectConfig
{
    /// <summary>
    /// ���Ϊ0�������еĶ���Ч
    /// </summary>
    [HorizontalGroup("AA", 120)]
    [LabelText("��Ч����")]
    [LabelWidth(40)]
    public byte ReduceCount;

    [HorizontalGroup("AA", 120)]
    [LabelText("���۱���")]
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
}
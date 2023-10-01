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
            else if (type == ModifyTriggerEffectType.EnterUnitState)
            {
                result.Add(type.ToString(), new MTEC_EnterUnitState(type));
            }
            else if(type == ModifyTriggerEffectType.GainDropWaste)
            {
                result.Add(type.ToString(), new MTEC_GainDropWaste(type));
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

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
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

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
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
    [LabelText("ֵ")]
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
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
    [LabelWidth(40)]
    public PropertyModifyType ModifyType;

    [HorizontalGroup("AA", 150)]
    [LabelText("����Key")]
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
/// ����Unit��������
/// </summary>
public class MTEC_SetUnitPropertyValue : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public UnitPropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 150)]
    [LabelText("����")]
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
/// ȫ�ּ�ʱ����
/// </summary>
public class MTEC_AddGlobalTimerModifier : ModifyTriggerEffectConfig
{
    [DictionaryDrawerSettings()]
    public Dictionary<PropertyModifyKey, float> ModifyMap = new Dictionary<PropertyModifyKey, float>();

    [HorizontalGroup("AA", 200)]
    [LabelText("����ʱ��")]
    [LabelWidth(80)]
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

public class MTEC_EnterUnitState : ModifyTriggerEffectConfig
{

    [HorizontalGroup("AA", 200)]
    [LabelText("Key")]
    [LabelWidth(40)]
    public UnitPropertyModifyKey ModifyKey;

    [HorizontalGroup("AA", 120)]
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public float Value;

    [HorizontalGroup("AA", 200)]
    [LabelText("����ʱ��")]
    [LabelWidth(40)]
    public float DurationTime;

    public MTEC_EnterUnitState(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {

    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}

public class MTEC_GainDropWaste : ModifyTriggerEffectConfig
{
    [HorizontalGroup("AA", 120)]
    [LabelText("ֵ")]
    [LabelWidth(40)]
    public int Value;

    public MTEC_GainDropWaste(ModifyTriggerEffectType type) : base(type)
    {

    }

    public override void Excute(ModifyTriggerData data, uint parentUnitUID)
    {
        RogueManager.Instance.AddDropWasteCount(Value);
    }

    public override void UnExcute(ModifyTriggerData data, uint parentUnitUID)
    {

    }
}
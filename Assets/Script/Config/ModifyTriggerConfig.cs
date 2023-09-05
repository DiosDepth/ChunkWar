using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ModifyTriggerConfig 
{
    [HorizontalGroup("AA", 250)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [ReadOnly]
    public ModifyTriggerType TriggerType;

    [HorizontalGroup("AA", 120)]
    [LabelText("��������")]
    [LabelWidth(80)]
    public int TriggerCount;

    public ModifyTriggerConfig(ModifyTriggerType type)
    {
        this.TriggerType = type;
    }

    public static ValueDropdownList<ModifyTriggerConfig> GetModifyTriggerList()
    {
        ValueDropdownList<ModifyTriggerConfig> result = new ValueDropdownList<ModifyTriggerConfig>();
        foreach(ModifyTriggerType type in System.Enum.GetValues(typeof(ModifyTriggerType)))
        {
            if(type == ModifyTriggerType.OnKillEnemy)
            {
                result.Add(type.ToString(), new MTC_OnKillEnemy(type));
            }
        }

        return result;
    }
}

[InlineEditor]
public class MTC_OnKillEnemy : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("�Ƿ�ѭ������")]
    [LabelWidth(100)]
    public bool IsLoop;

    [HorizontalGroup("AB", 150)]
    [LabelText("������ɱ��")]
    [LabelWidth(100)]
    public int TriggerRequireCount;

    public MTC_OnKillEnemy(ModifyTriggerType type) : base(type)
    {

    }
}

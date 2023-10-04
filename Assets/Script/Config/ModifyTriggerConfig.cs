using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoolType
{
    True,
    False,
    All
}

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

    [HorizontalGroup("AA", 120)]
    [LabelText("���ʴ���")]
    [LabelWidth(80)]
    public bool UsePercent;

    [ShowIf("UsePercent")]
    [HorizontalGroup("AA", 140)]
    [LabelText("����")]
    [LabelWidth(50)]
    public float Percent;

    [LabelText("����Ч��")]
    [HorizontalGroup("AC", Order = 1000)]
    [HideReferenceObjectPicker]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetEffectList", DrawDropdownForListElements = false)]
    public ModifyTriggerEffectConfig[] Effects = new ModifyTriggerEffectConfig[0];

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
            else if(type == ModifyTriggerType.PropertyTransfer)
            {
                result.Add(type.ToString(), new MTC_PropertyTransfer(type));
            }
            else if (type == ModifyTriggerType.ItemTransfer)
            {
                result.Add(type.ToString(), new MTC_ItemTransfer(type));
            }
            else if (type == ModifyTriggerType.WaveState)
            {
                result.Add(type.ToString(), new MTC_WaveState(type));
            }
            else if (type == ModifyTriggerType.Timer)
            {
                result.Add(type.ToString(), new MTC_Timer(type));
            }
            else if (type == ModifyTriggerType.OnAdd)
            {
                result.Add(type.ToString(), new MTC_OnAdd(type));
            }
            else if (type == ModifyTriggerType.OnPlayerShipMove)
            {
                result.Add(type.ToString(), new MTC_OnPlayerShipMove(type));
            }
            else if (type == ModifyTriggerType.OnRefreshShop)
            {
                result.Add(type.ToString(), new MTC_OnRefreshShop(type));
            }
            else if(type == ModifyTriggerType.ItemRarityCount)
            {
                result.Add(type.ToString(), new MTC_ByItemRarityCount(type));
            }
            else if (type == ModifyTriggerType.OnEnterHarbor)
            {
                result.Add(type.ToString(), new MTC_OnEnterHarbor(type));
            }
            else if (type == ModifyTriggerType.ByCoreHPPercent)
            {
                result.Add(type.ToString(), new MTC_CoreHPPercent(type));
            }
            else if (type == ModifyTriggerType.OnShieldRecover)
            {
                result.Add(type.ToString(), new MTC_OnShieldRecover(type));
            }
            else if (type == ModifyTriggerType.OnWeaponHitTarget)
            {
                result.Add(type.ToString(), new MTC_OnWeaponHitTarget(type));
            }
            else if (type == ModifyTriggerType.OnPlayerWeaponReload)
            {
                result.Add(type.ToString(), new MTC_OnWeaponReload(type));
            }
            else if (type == ModifyTriggerType.OnPlayerWeaponFire)
            {
                result.Add(type.ToString(), new MTC_OnWeaponFire(type));
            }
        }

        return result;
    }

    private static ValueDropdownList<ModifyTriggerEffectConfig> GetEffectList()
    {
        return ModifyTriggerEffectConfig.GetModifyEffectTriggerList();
    }
}

public class MTC_OnAdd : ModifyTriggerConfig
{
    public MTC_OnAdd(ModifyTriggerType type) : base(type)
    {

    }
}

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

    [HorizontalGroup("AB", 150)]
    [LabelText("������ɱ")]
    [LabelWidth(100)]
    public BoolType CheckCriticalKill = BoolType.All;

    public MTC_OnKillEnemy(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_WaveState : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("���ο�ʼ")]
    [LabelWidth(100)]
    public bool IsStart;

    public MTC_WaveState(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_Timer : ModifyTriggerConfig
{
    public enum TimerType
    {
        EveryPerTime,
        OnceTime
    }

    public enum SpecialTimeType
    {
        NONE,
        /// <summary>
        /// ������X��
        /// </summary>
        LastSeconds,
        /// <summary>
        /// ��ʼ��X��
        /// </summary>
        StartSeconds,
    }

    [HorizontalGroup("AB", 200)]
    [LabelText("ʱ���������")]
    [LabelWidth(50)]
    public TimerType TimerTriggerType;

    [HorizontalGroup("AB", 200)]
    [LabelText("����ʱ������")]
    [LabelWidth(90)]
    public SpecialTimeType SpecialType;

    [HorizontalGroup("AB", 120)]
    [LabelText("ʱ�����")]
    [LabelWidth(50)]
    public float TimeParam;

    public MTC_Timer(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_PropertyTransfer : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("��Դ")]
    [LabelWidth(50)]
    public PropertyModifyKey BaseProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("��Դֵ")]
    [LabelWidth(50)]
    public float BaseValuePer;

    [HorizontalGroup("AB", 200)]
    [LabelText("Ŀ��")]
    [LabelWidth(50)]
    public PropertyModifyKey TargetProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("Ŀ��ֵ")]
    [LabelWidth(50)]
    public float TargetValuePer;

    public MTC_PropertyTransfer(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_ItemTransfer : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("��Դ")]
    [LabelWidth(50)]
    public GoodsItemType ItemType;

    [HorizontalGroup("AB", 200)]
    [LabelText("��ƷID")]
    [InfoBox("�к� Ϊ -1")]
    [LabelWidth(50)]
    public int ItemID;

    [HorizontalGroup("AB", 150)]
    [LabelText("��Դֵ")]
    [LabelWidth(50)]
    public float BaseValuePer;

    [HorizontalGroup("AB", 200)]
    [LabelText("Ŀ��")]
    [LabelWidth(50)]
    public PropertyModifyKey TargetProperty;

    [HorizontalGroup("AB", 150)]
    [LabelText("Ŀ��ֵ")]
    [LabelWidth(50)]
    public float TargetValuePer;

    public MTC_ItemTransfer(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnPlayerShipMove : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("�Ƿ��ƶ�")]
    [LabelWidth(50)]
    public bool IsMoving;

    public MTC_OnPlayerShipMove(ModifyTriggerType type) : base(type)
    {

    }
}

/// <summary>
/// �����������ϡ�ж���������
/// </summary>
public class MTC_ByItemRarityCount : ModifyTriggerConfig
{
    public enum ItemType
    {
        Unit,
        Plug,
        All
    }

    [HorizontalGroup("AB", 200)]
    [LabelText("����")]
    [LabelWidth(50)]
    public ItemType Type;
    public List<GoodsItemRarity> VaildRarity = new List<GoodsItemRarity>();

    public MTC_ByItemRarityCount(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnRefreshShop : ModifyTriggerConfig
{
    public MTC_OnRefreshShop(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnEnterHarbor : ModifyTriggerConfig
{
    public MTC_OnEnterHarbor(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_CoreHPPercent : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 200)]
    [LabelText("����")]
    [LabelWidth(50)]
    public byte HPPercent;

    [HorizontalGroup("AB", 200)]
    [LabelText("�Ƚ�")]
    [LabelWidth(50)]
    public CompareType Compare;

    public MTC_CoreHPPercent(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnShieldRecover : ModifyTriggerConfig
{

    public MTC_OnShieldRecover(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnWeaponHitTarget : ModifyTriggerConfig
{
    [HorizontalGroup("AB", 150)]
    [LabelText("�ж��˺�����")]
    [LabelWidth(80)]
    public BoolType DamageTypeBool = BoolType.All;

    [HorizontalGroup("AB", 200)]
    [LabelText("�˺�����")]
    [LabelWidth(50)]
    public WeaponDamageType DamageType;

    [HorizontalGroup("AB", 120)]
    [LabelText("�жϱ���")]
    [LabelWidth(50)]
    public BoolType CriticalBool = BoolType.All;

    [HorizontalGroup("AD", 150)]
    [LabelText("���������˺�")]
    [LabelWidth(80)]
    public bool ModifyFinalDamage = false;

    [HorizontalGroup("AD", 120)]
    [LabelText("�˺�����%")]
    [LabelWidth(50)]
    [ShowIf("ModifyFinalDamage")]
    public float DamageAddPercent;

    public MTC_OnWeaponHitTarget(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnWeaponReload : ModifyTriggerConfig
{
    public MTC_OnWeaponReload(ModifyTriggerType type) : base(type)
    {

    }
}

public class MTC_OnWeaponFire : ModifyTriggerConfig
{
    [HorizontalGroup("AD", 120)]
    [LabelText("�������")]
    [LabelWidth(50)]
    public bool CheckFireCount;

    [HorizontalGroup("AD", 120)]
    [LabelText("�������")]
    [LabelWidth(50)]
    [ShowIf("CheckFireCount")]
    public int FireCount;

    public MTC_OnWeaponFire(ModifyTriggerType type) : base(type)
    {

    }
}
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponDamageType
{
    Physics,
    Energy,
}

[CreateAssetMenu(fileName = "Configs_Weapon_", menuName = "Configs/Unit/WeaponConfig")]
public class WeaponConfig : BaseUnitConfig
{
    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("基础伤害")]
    [LabelWidth(80)]
    public int DamageBase;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("伤害比例浮动")]
    [LabelWidth(80)]
    public bool UseDamageRatio = false;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("伤害比例Min")]
    [LabelWidth(80)]
    [ShowIf("UseDamageRatio")]
    public float DamageRatioMin;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("伤害比例Max")]
    [LabelWidth(80)]
    [ShowIf("UseDamageRatio")]
    public float DamageRatioMax;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/A", 200)]
    [LabelText("伤害类型")]
    [LabelWidth(80)]
    public WeaponDamageType DamageType;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("基础范围")]
    [LabelWidth(80)]
    public int BaseRange;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("基础暴击")]
    [LabelWidth(80)]
    public float BaseCriticalRate;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("暴击伤害")]
    [LabelWidth(80)]
    public float CriticalDamage;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("伤害数量")]
    [LabelWidth(80)]
    public byte TotalDamageCount;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("单次发射数量")]
    [LabelWidth(80)]
    public byte ShootPerCount;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/C", 200)]
    [LabelText("装填CD")]
    [LabelWidth(80)]
    public float CD;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/C", 200)]
    [LabelText("伤害间隔")]
    [LabelWidth(80)]
    public float FireCD;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/D", 200)]
    [LabelText("护盾穿透")]
    [LabelWidth(80)]
    public bool ShieldTransfixion;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/D", 200)]
    [LabelText("护盾伤害")]
    [LabelWidth(80)]
    public float ShieldDamage;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/D", 200)]
    [LabelText("基础贯通")]
    [LabelWidth(80)]
    public byte BaseTransfixion;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/D", 200)]
    [LabelText("贯通衰减")]
    [LabelWidth(80)]
    public float TransfixionReduce;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/E")]
    [LabelText("伤害来源修正")]
    [LabelWidth(100)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();

    [FoldoutGroup("音频配置")]
    [LabelText("开火音效")]
    [LabelWidth(100)]
    public string FireAudioClip;
}


using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponDamageType
{
    Physics,
    Energy,
    NONE,
}

[System.Serializable]
public class CameraShakeConfig
{
    [HorizontalGroup("Z", 120)]
    [LabelText("震动")]
    [LabelWidth(80)]
    public bool CameraShake;

    [HorizontalGroup("Z", 120)]
    [LabelText("预设")]
    [LabelWidth(80)]
    public bool UsePreset = true;

    [HorizontalGroup("Z", 300)]
    [LabelText("预设名")]
    [LabelWidth(80)]
    [ShowIf("UsePreset")]
    public string PresetName;

    [HorizontalGroup("Z", 200)]
    [LabelText("时长")]
    [LabelWidth(80)]
    [HideIf("UsePreset")]
    public float Duration = 0.4f;

    [HorizontalGroup("Z", 350)]
    [LabelText("频率")]
    [LabelWidth(50)]
    [MinMaxSlider(0, 5)]
    public Vector2 Frequency = new Vector2(1, 1.5f);

    [HorizontalGroup("Z", 350)]
    [LabelText("震幅")]
    [LabelWidth(50)]
    [MinMaxSlider(0, 6)]
    public Vector2 Amplitude = new Vector2(2, 2.5f);
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
    [LabelText("敌人攻击比例")]
    [LabelWidth(100)]
    public float EnemyAttackModify;

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
    [HorizontalGroup("基础属性/B", 200)]
    [LabelText("可修正伤害数")]
    [LabelWidth(80)]
    public bool CanModifyDamageCount = false;

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
    [LabelText("散射角度")]
    [LabelWidth(80)]
    public int Scatter = 0;

    [FoldoutGroup("基础属性")]
    [HorizontalGroup("基础属性/E")]
    [LabelText("伤害来源修正")]
    [LabelWidth(100)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();

    [FoldoutGroup("效果配置")]
    [LabelText("开火音效")]
    [LabelWidth(100)]
    public string FireAudioClip;

    [FoldoutGroup("效果配置")]
    [LabelText("开火特效")]
    [LabelWidth(100)]
    public GeneralEffectConfig FireEffect;

    [FoldoutGroup("效果配置")]
    [LabelText("命中相机震动")]
    public CameraShakeConfig HitShakeCfg;

    [FoldoutGroup("效果配置")]
    [LabelText("开火相机震动")]
    public CameraShakeConfig FireShakeCfg;

    [FoldoutGroup("效果配置")]
    [LabelText("持续伤害展示")]
    [LabelWidth(100)]
    public bool PersistentDamageDisplay = false;

}


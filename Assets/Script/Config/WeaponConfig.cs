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
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�����˺�")]
    [LabelWidth(80)]
    public int DamageBase;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����Min")]
    [LabelWidth(80)]
    public float DamageRatioMin;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����Max")]
    [LabelWidth(80)]
    public float DamageRatioMax;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����")]
    [LabelWidth(80)]
    public WeaponDamageType DamageType;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("������ͨ")]
    [LabelWidth(80)]
    public byte BaseTransfixion;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 200)]
    [LabelText("������Χ")]
    [LabelWidth(80)]
    public int BaseRange;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 200)]
    [LabelText("��������")]
    [LabelWidth(80)]
    public float BaseCriticalRate;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 200)]
    [LabelText("�����˺�")]
    [LabelWidth(80)]
    public float CriticalDamage;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 200)]
    [LabelText("�˺�����")]
    [LabelWidth(80)]
    public byte DamageCount;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/C", 500)]
    [LabelText("�˺���Դ����")]
    [LabelWidth(100)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();
}

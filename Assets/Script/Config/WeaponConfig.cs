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
    [LabelText("���˹�������")]
    [LabelWidth(100)]
    public float EnemyAttackModify;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺���������")]
    [LabelWidth(80)]
    public bool UseDamageRatio = false;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����Min")]
    [LabelWidth(80)]
    [ShowIf("UseDamageRatio")]
    public float DamageRatioMin;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����Max")]
    [LabelWidth(80)]
    [ShowIf("UseDamageRatio")]
    public float DamageRatioMax;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [LabelText("�˺�����")]
    [LabelWidth(80)]
    public WeaponDamageType DamageType;

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
    public byte TotalDamageCount;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 200)]
    [LabelText("���η�������")]
    [LabelWidth(80)]
    public byte ShootPerCount;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/C", 200)]
    [LabelText("װ��CD")]
    [LabelWidth(80)]
    public float CD;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/C", 200)]
    [LabelText("�˺����")]
    [LabelWidth(80)]
    public float FireCD;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/D", 200)]
    [LabelText("���ܴ�͸")]
    [LabelWidth(80)]
    public bool ShieldTransfixion;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/D", 200)]
    [LabelText("�����˺�")]
    [LabelWidth(80)]
    public float ShieldDamage;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/D", 200)]
    [LabelText("������ͨ")]
    [LabelWidth(80)]
    public byte BaseTransfixion;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/D", 200)]
    [LabelText("��ͨ˥��")]
    [LabelWidth(80)]
    public float TransfixionReduce;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/D", 200)]
    [LabelText("ɢ��Ƕ�")]
    [LabelWidth(80)]
    public int Scatter = 0;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/E")]
    [LabelText("�˺���Դ����")]
    [LabelWidth(100)]
    public List<UnitPropertyModifyFrom> DamageModifyFrom = new List<UnitPropertyModifyFrom>();

    [FoldoutGroup("Ч������")]
    [LabelText("������Ч")]
    [LabelWidth(100)]
    public string FireAudioClip;

    [FoldoutGroup("Ч������")]
    [LabelText("������Ч")]
    [LabelWidth(100)]
    public string FireEffectPath;
}


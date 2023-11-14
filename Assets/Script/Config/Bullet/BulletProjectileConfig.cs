using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectileConfig : BulletConfig
{
    [HorizontalGroup("Info")]
    [BoxGroup("Info/��ը��Χ")]
    [LabelWidth(150)]
    [HideLabel]
    public float ExplodeRange;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/ʱ��")]
    [LabelWidth(150)]
    [HideLabel]
    public float LifeTime;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/����ٶ�")]
    [LabelWidth(150)]
    [HideLabel]
    public float MaxSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/��ת�ٶ�")]
    [LabelWidth(150)]
    [HideLabel]
    public float RotationSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/���ٶ�")]
    [LabelWidth(150)]
    [HideLabel]
    public float InitialSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/���ٶ�")]
    [LabelWidth(150)]
    [HideLabel]
    public float Acceleration;

    [FoldoutGroup("Effect")]
    [LabelText("������Ч")]
    [LabelWidth(60)]
    public string DestroyAudio;

    [HorizontalGroup("Info2", 200)]
    [BoxGroup("Info2/����Ѫ��")]
    [LabelWidth(150)]
    [HideLabel]
    [ShowIf("Type", BulletType.Missile)]
    public int MissileHP;
}

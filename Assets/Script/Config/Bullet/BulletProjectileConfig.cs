using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectileConfig : BulletConfig
{
    [HorizontalGroup("Info")]
    [BoxGroup("Info/爆炸范围")]
    [LabelWidth(150)]
    [HideLabel]
    public float ExplodeRange;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/时长")]
    [LabelWidth(150)]
    [HideLabel]
    public float LifeTime;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/最大速度")]
    [LabelWidth(150)]
    [HideLabel]
    public float MaxSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/旋转速度")]
    [LabelWidth(150)]
    [HideLabel]
    public float RotationSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/初速度")]
    [LabelWidth(150)]
    [HideLabel]
    public float InitialSpeed;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/加速度")]
    [LabelWidth(150)]
    [HideLabel]
    public float Acceleration;

    [HorizontalGroup("Info2", 200)]
    [BoxGroup("Info2/导弹血量")]
    [LabelWidth(150)]
    [HideLabel]
    public int MissileHP;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BulletInstanceHitConfig : BulletConfig
{
    [HorizontalGroup("Info")]
    [BoxGroup("Info/��ը��Χ")]
    [LabelWidth(150)]
    [HideLabel]
    public float ExplodeRange;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/������")]
    [LabelWidth(150)]
    [HideLabel]
    public float MaxDistance;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/���")]
    [LabelWidth(150)]
    [HideLabel]
    public float Width;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/�ٶ�")]
    [LabelWidth(150)]
    [HideLabel]
    public float EmitTime;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/����ʱ��")]
    [LabelWidth(150)]
    [HideLabel]
    public float Duration;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/����ʱ��")]
    [LabelWidth(150)]
    [HideLabel]
    public float DeathTime;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BulletInstanceHitConfig : BulletConfig
{
    [HorizontalGroup("Info")]
    [BoxGroup("Info/爆炸范围")]
    [LabelWidth(150)]
    [HideLabel]
    public float ExplodeRange;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/最大距离")]
    [LabelWidth(150)]
    [HideLabel]
    public float MaxDistance;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/宽度")]
    [LabelWidth(150)]
    [HideLabel]
    public float Width;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/速度")]
    [LabelWidth(150)]
    [HideLabel]
    public float EmitTime;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/持续时间")]
    [LabelWidth(150)]
    [HideLabel]
    public float Duration;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/死亡时间")]
    [LabelWidth(150)]
    [HideLabel]
    public float DeathTime;
}

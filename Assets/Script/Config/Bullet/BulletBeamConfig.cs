using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BulletBeamConfig : BulletConfig
{
    [HorizontalGroup("Info")]
    [BoxGroup("Info/最大距离")]
    [LabelWidth(150)]
    [HideLabel]
    public float MaxDistance;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/激光宽度")]
    [LabelWidth(150)]
    [HideLabel]
    public float Width;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/激光速度时间")]
    [LabelWidth(150)]
    [HideLabel]
    public float EmiteTime;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/持续时间")]
    [LabelWidth(150)]
    [HideLabel]
    public float Duration;

    [HorizontalGroup("Info")]
    [BoxGroup("Info/死亡时长")]
    [LabelWidth(150)]
    [HideLabel]
    public float DeathTime;

}
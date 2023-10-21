using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


/// <summary>
/// 无人机配置
/// </summary>
[CreateAssetMenu(fileName = "Configs_Drone_", menuName = "Configs/Drones")]
public class DroneConfig : BaseShipConfig
{

    [FoldoutGroup("无人机配置")]
    [HorizontalGroup("无人机配置/A", 300)]
    [LabelText("所有者")]
    [LabelWidth(80)]
    public OwnerType Owner;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/最大速度")]
    [HideLabel]
    public float MaxVelocity;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/最大加速度")]
    [HideLabel]
    public float MaxAcceleration;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/最大转速度")]
    [HideLabel]
    public float MaxAngularAcceleration = 3f;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/最大旋转速度")]
    [HideLabel]
    public float MaxAngularVelocity = 1f;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/半径大小")]
    [HideLabel]
    public float boidRadius = 1f;

    [FoldoutGroup("行为配置")]
    [HorizontalGroup("行为配置/A")]
    [BoxGroup("行为配置/A/搜索半径")]
    [HideLabel]
    public float targetSerchingRadius = 15f;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A")]
    public string DieAudio;


    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }

#if UNITY_EDITOR
    [OnInspectorInit]
    protected override void InitData()
    {
        base.InitData();

    }
#endif
}

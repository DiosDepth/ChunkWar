using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Configs_DroneFactory_", menuName = "Configs/Unit/DroneFactory")]
public class DroneFactoryConfig : BuildingConfig
{
    [FoldoutGroup("机库配置")]
    [HorizontalGroup("机库配置/A", 250)]
    [BoxGroup("机库配置/A/无人机ID")]
    [HideLabel]
    public int ID;

    [FoldoutGroup("机库配置")]
    [HorizontalGroup("机库配置/A", 250)]
    [BoxGroup("机库配置/A/最大数量")]
    [HideLabel]
    public byte MaxDroneCount;

    [FoldoutGroup("机库配置")]
    [HorizontalGroup("机库配置/A", 250)]
    [BoxGroup("机库配置/A/创生间隔")]
    [HideLabel]
    public float DroneSpawnTime;

    [FoldoutGroup("机库配置")]
    [HorizontalGroup("机库配置/A", 250)]
    [BoxGroup("机库配置/A/搜寻范围")]
    [HideLabel]
    public float SearchingRadius;
}

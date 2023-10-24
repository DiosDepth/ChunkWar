using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Configs_DroneFactory_", menuName = "Configs/Unit/DroneFactory")]
public class DroneFactoryConfig : BuildingConfig
{
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 250)]
    [BoxGroup("��������/A/���˻�ID")]
    [HideLabel]
    public int DroneID;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 250)]
    [BoxGroup("��������/A/�������")]
    [HideLabel]
    public byte MaxDroneCount;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 250)]
    [BoxGroup("��������/A/�������")]
    [HideLabel]
    public float DroneSpawnTime;
}

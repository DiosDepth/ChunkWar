using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_SelfExplosiveBuilding_", menuName = "Configs/Unit/SelfExplosiveBuilding")]
public class SelfExplosiveBuildingConfig : BuildingConfig
{
    [FoldoutGroup("自爆建筑配置")]
    [HorizontalGroup("自爆建筑配置/A", 250)]
    [BoxGroup("自爆建筑配置/A/爆炸伤害范围")]
    [HideLabel]
    public float DamageRange;

    [FoldoutGroup("自爆建筑配置")]
    [HorizontalGroup("自爆建筑配置/A", 250)]
    [BoxGroup("自爆建筑配置/A/爆炸伤害")]
    [HideLabel]
    public int DamageValue;

    [FoldoutGroup("自爆建筑配置")]
    [HorizontalGroup("自爆建筑配置/A", 250)]
    [BoxGroup("自爆建筑配置/A/爆炸延迟")]
    [HideLabel]
    public float ExplodeDelay;

}

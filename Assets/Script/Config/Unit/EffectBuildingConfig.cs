using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Configs_EffectBuilding_", menuName = "Configs/Unit/EffectBuilding")]
public class EffectBuildingConfig : BuildingConfig
{
    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A", 250)]
    [BoxGroup("效果配置/A/效果范围")]
    [HideLabel]
    public float EffectRadius;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A", 250)]
    [BoxGroup("效果配置/A/触发间隔")]
    [HideLabel]
    public float TriggerInterval;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A", 250)]
    [BoxGroup("效果配置/A/初始间隔")]
    [HideLabel]
    public float FirstSpawnCD;

    [FoldoutGroup("效果配置")]
    [ListDrawerSettings(CustomAddFunction = "AddModifier")]
    [HideReferenceObjectPicker]
    public MTEC_AddUnitTimerModifier[] Modifiers = new MTEC_AddUnitTimerModifier[0];


    private MTEC_AddUnitTimerModifier AddModifier()
    {
        return new MTEC_AddUnitTimerModifier(ModifyTriggerEffectType.AddUnitTimerModifier);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Configs_EffectBuilding_", menuName = "Configs/Unit/EffectBuilding")]
public class EffectBuildingConfig : BuildingConfig
{
    [FoldoutGroup("Ч������")]
    [HorizontalGroup("Ч������/A", 250)]
    [BoxGroup("Ч������/A/Ч����Χ")]
    [HideLabel]
    public float EffectRadius;

    [FoldoutGroup("Ч������")]
    [HorizontalGroup("Ч������/A", 250)]
    [BoxGroup("Ч������/A/�������")]
    [HideLabel]
    public float TriggerInterval;

    [FoldoutGroup("Ч������")]
    [ListDrawerSettings(CustomAddFunction = "AddModifier")]
    [HideReferenceObjectPicker]
    public MTEC_AddUnitTimerModifier[] Modifiers = new MTEC_AddUnitTimerModifier[0];


    private MTEC_AddUnitTimerModifier AddModifier()
    {
        return new MTEC_AddUnitTimerModifier(ModifyTriggerEffectType.AddUnitTimerModifier);
    }
}

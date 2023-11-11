using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_SelfExplosiveBuilding_", menuName = "Configs/Unit/SelfExplosiveBuilding")]
public class SelfExplosiveBuildingConfig : BuildingConfig
{
    [FoldoutGroup("�Ա���������")]
    [HorizontalGroup("�Ա���������/A", 250)]
    [BoxGroup("�Ա���������/A/��ը�˺���Χ")]
    [HideLabel]
    public float DamageRange;

    [FoldoutGroup("�Ա���������")]
    [HorizontalGroup("�Ա���������/A", 250)]
    [BoxGroup("�Ա���������/A/��ը�˺�")]
    [HideLabel]
    public int DamageValue;

    [FoldoutGroup("�Ա���������")]
    [HorizontalGroup("�Ա���������/A", 250)]
    [BoxGroup("�Ա���������/A/��ը�ӳ�")]
    [HideLabel]
    public float ExplodeDelay;

}

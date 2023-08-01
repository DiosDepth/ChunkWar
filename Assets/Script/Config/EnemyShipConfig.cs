using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_Enemy_", menuName = "Configs/Unit/EnemyConfig")]
public class EnemyShipConfig : SerializedScriptableObject
{
    [LabelText("敌人ID")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 150)]
    public int EnemyID;

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 300)]
    public GameObject Prefab;

    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [HideLabel]
    [TitleGroup("属性配置", Alignment =  TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/核心血量")]
    public int CoreHP;


    [HideLabel]
    [TitleGroup("属性配置", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/基础速度")]
    public int SpeedBase;

    [TableList]
    [LabelText("敌人难度等级")]
    public List<EnemyHardLevelMap> HardLevelMap = new List<EnemyHardLevelMap>();
}

[System.Serializable]
public class EnemyHardLevelMap
{
    public float CoreHPRatio;
    public float DamageRatio;
}
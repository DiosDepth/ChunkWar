using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "Configs_AIShip_", menuName = "Configs/Unit/AIShipConfig")]
public class AIShipConfig : BaseShipConfig
{

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

    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }


    [OnInspectorInit]
    protected override void InitData()
    {
        base.InitData();

    }
}

[System.Serializable]
public class EnemyHardLevelMap
{
    public float CoreHPRatio;
    public float DamageRatio;
}


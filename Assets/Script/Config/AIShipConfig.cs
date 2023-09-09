using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum EnemyHPBillBoardType
{
    Boss_UI,
    Elite_Scene,
}

[CreateAssetMenu(fileName = "Configs_AIShip_", menuName = "Configs/Unit/AIShipConfig")]
public class AIShipConfig : BaseShipConfig
{

    [HideLabel]
    [TitleGroup("属性配置", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/HP血条显示")]
    public bool ShowHPBillboard = false;

    [HideLabel]
    [TitleGroup("属性配置", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/血条类型")]
    public EnemyHPBillBoardType BillboardType;

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
    public EnemyHardLevelMap HardLevelCfg;

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
    public float HPRatio;
    public float DamageRatio;
    public float DropValueRatio;
}


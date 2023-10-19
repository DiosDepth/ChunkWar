using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyHPBillBoardType
{
    Boss_UI,
    Elite_Scene,
}

public enum EnemyClassType
{
    Normal,
    Elite,
    Boss
}

[CreateAssetMenu(fileName = "Configs_AIShip_", menuName = "Configs/EnemyShips")]
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
    [BoxGroup("属性配置/AA/等级")]
    public EnemyClassType ClassLevel;

    [HideLabel]
    [TitleGroup("属性配置", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/血条类型")]
    [ShowIf("ShowHPBillboard")]
    public EnemyHPBillBoardType BillboardType;

    [HideLabel]
    [TitleGroup("属性配置", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/基础攻击")]
    public int AttackBase;

    [LabelText("敌人难度等级")]
    public int HardLevelGroupID;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/最大速度")]
    [HideLabel]
    public float MaxVelocity;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/最大加速度")]
    [HideLabel]
    public float MaxAcceleration;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/最大转速度")]
    [HideLabel]
    public float MaxAngularAcceleration = 3f;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/最大旋转速度")]
    [HideLabel]
    public float MaxAngularVelocity = 1f;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/半径大小")]
    [HideLabel]
    public float boidRadius = 1f;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/搜索半径")]
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
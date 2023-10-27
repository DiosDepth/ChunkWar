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


    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/难度等级")]
    [HideLabel]
    public int HardLevelGroupID;

    [HorizontalGroup("属性配置/AA", 150)]
    [BoxGroup("属性配置/AA/数量可以修正")]
    [HideLabel]
    public bool EnemyCountModify;

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

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/到达玩家最小距离")]
    [HideLabel]
    public float ArrivalRadius = 15f;

    [FoldoutGroup("AI配置")]
    [HorizontalGroup("AI配置/A")]
    [BoxGroup("AI配置/A/到达减速距离")]
    [HideLabel]
    public float SlotRadius = 5f;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A")]
    [LabelText("死亡音效")]
    [LabelWidth(100)]
    public string DieAudio;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A")]
    [LabelText("改变OrthographicSize")]
    [LabelWidth(150)]
    public bool AppearChangeCameraOrthographicSize = false;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/A")]
    [LabelText("改变FOV")]
    [LabelWidth(100)]
    public int CameraTargetOrthographicSize = 40;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/B")]
    [LabelText("出现警告")]
    [LabelWidth(100)]
    public bool AppearWarning = false;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/B")]
    [LabelText("警告文本")]
    [LabelWidth(100)]
    [ShowIf("AppearWarning")]
    public string AppearWarningText;

    [FoldoutGroup("效果配置")]
    [HorizontalGroup("效果配置/B")]
    [LabelText("警告音效")]
    [LabelWidth(100)]
    [ShowIf("AppearWarning")]
    public string AppearWarningAudio;


    [FoldoutGroup("掉落配置")]
    public List<DropInfo> DropList = new List<DropInfo>();

    [FoldoutGroup("掉落配置")]
    public Dictionary<GoodsItemRarity, ShopGoodsRarityConfig> WreckageRarityCfg = new Dictionary<GoodsItemRarity, ShopGoodsRarityConfig>();

    [LabelText("单位特殊技能")]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] Skills = new ModifyTriggerConfig[0];


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

    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }

#endif
}
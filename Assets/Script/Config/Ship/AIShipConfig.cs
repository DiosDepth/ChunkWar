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
    [TitleGroup("��������", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/HPѪ����ʾ")]
    public bool ShowHPBillboard = false;

    [HideLabel]
    [TitleGroup("��������", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/�ȼ�")]
    public EnemyClassType ClassLevel;

    [HideLabel]
    [TitleGroup("��������", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/Ѫ������")]
    [ShowIf("ShowHPBillboard")]
    public EnemyHPBillBoardType BillboardType;

    [HideLabel]
    [TitleGroup("��������", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/��������")]
    public int AttackBase;


    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/�Ѷȵȼ�")]
    [HideLabel]
    public int HardLevelGroupID;

    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/������������")]
    [HideLabel]
    public bool EnemyCountModify;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/����ٶ�")]
    [HideLabel]
    public float MaxVelocity;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/�����ٶ�")]
    [HideLabel]
    public float MaxAcceleration;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/���ת�ٶ�")]
    [HideLabel]
    public float MaxAngularAcceleration = 3f;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/�����ת�ٶ�")]
    [HideLabel]
    public float MaxAngularVelocity = 1f;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/�뾶��С")]
    [HideLabel]
    public float boidRadius = 1f;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/�����뾶")]
    [HideLabel]
    public float targetSerchingRadius = 15f;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/���������С����")]
    [HideLabel]
    public float ArrivalRadius = 15f;

    [FoldoutGroup("AI����")]
    [HorizontalGroup("AI����/A")]
    [BoxGroup("AI����/A/������پ���")]
    [HideLabel]
    public float SlotRadius = 5f;

    [FoldoutGroup("Ч������")]
    [HorizontalGroup("Ч������/A")]
    public string DieAudio;

    [FoldoutGroup("��������")]
    public List<DropInfo> DropList = new List<DropInfo>();

    [FoldoutGroup("��������")]
    public Dictionary<GoodsItemRarity, ShopGoodsRarityConfig> WreckageRarityCfg = new Dictionary<GoodsItemRarity, ShopGoodsRarityConfig>();

    [LabelText("��λ���⼼��")]
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
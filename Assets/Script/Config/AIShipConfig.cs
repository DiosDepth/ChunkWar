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

[CreateAssetMenu(fileName = "Configs_AIShip_", menuName = "Configs/Unit/AIShipConfig")]
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
    public EnemyHPBillBoardType BillboardType;

    [HideLabel]
    [TitleGroup("��������", Alignment =  TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/����Ѫ��")]
    public int CoreHP;


    [HideLabel]
    [TitleGroup("��������", Alignment = TitleAlignments.Centered)]
    [HorizontalGroup("��������/AA", 150)]
    [BoxGroup("��������/AA/�����ٶ�")]
    public int SpeedBase;

    [LabelText("�����Ѷȵȼ�")]
    public int HardLevelGroupID;

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
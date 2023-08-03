using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "Configs_AIShip_", menuName = "Configs/Unit/AIShipConfig")]
public class AIShipConfig : BaseShipConfig
{

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

    [TableList]
    [LabelText("�����Ѷȵȼ�")]
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


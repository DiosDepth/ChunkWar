using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configs_Enemy_", menuName = "Configs/Unit/EnemyConfig")]
public class EnemyShipConfig : SerializedScriptableObject
{
    [LabelText("����ID")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 150)]
    public int EnemyID;

    [LabelText("Prefab")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 300)]
    public GameObject Prefab;

    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

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
}

[System.Serializable]
public class EnemyHardLevelMap
{
    public float CoreHPRatio;
    public float DamageRatio;
}
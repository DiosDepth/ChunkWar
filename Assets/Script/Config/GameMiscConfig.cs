using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMiscConfig", menuName = "Configs/Main")]
public class GameMiscConfig : SerializedScriptableObject
{
    public float Harbor_Teleport_RandomRangeMin;
    public float Harbor_Teleport_RandomRangeMax;

    [HideReferenceObjectPicker]
    public ShipClassConfig[] ShipClasses = new ShipClassConfig[0];

    [HideReferenceObjectPicker]
    public ShipControlConfig ShipControlCfg = new ShipControlConfig();

    public List<AchievementGroupItemConfig> AchievementGroupConfig = new List<AchievementGroupItemConfig>();
    public CollectionMenuKey[] CollectionMenu = new CollectionMenuKey[0];

    public AchievementGroupItemConfig GetAchievementGroupConfig(AchievementGroupType type)
    {
        return AchievementGroupConfig.Find(x => x.Type == type);
    }

    public ShipClassConfig GetShipClassConfig(ShipClassType type)
    {
        for(int i = 0; i < ShipClasses.Length; i++)
        {
            if (ShipClasses[i].ClassType == type)
                return ShipClasses[i];
        }
        return null;
    }
}

[System.Serializable]
public class AchievementGroupItemConfig
{
    public AchievementGroupType Type;
    public string GroupName;
    public Sprite GroupIcon;
}

[System.Serializable]
public class CollectionMenuKey
{
    public string Key;
    public string Name;
    public int Order;

    public CollectionMenuKey[] SubMenus = new CollectionMenuKey[0];
}

public enum ShipClassType
{
    /// <summary>
    /// ������
    /// </summary>
    Corvette,
    /// <summary>
    /// ����
    /// </summary>
    Destroyer,
    /// <summary>
    /// Ѳ��
    /// </summary>
    Cruiser,
    /// <summary>
    /// ս��Ѳ��
    /// </summary>
    BattleCruiser,
    /// <summary>
    /// ս�н�
    /// </summary>
    Battleship
}

public class ShipClassConfig
{
    public ShipClassType ClassType;
    public string ClassName;
    public Sprite ClassIcon;

    public float BaseSpeedRatio;
    public float BaseParry;
    public float MaxParry;
}

public class ShipControlConfig
{
    [LabelText("����ٶ�")]
    [LabelWidth(100)]
    public float MaxSpeed_Base;
    [LabelText("���ٶ�")]
    [LabelWidth(100)]
    public float Acceleration_Base;
    [LabelText("��ת�ٶ�")]
    [LabelWidth(100)]
    public float MaxRotateSpeed_Base;
    [LabelText("��ת���ٶ�")]
    [LabelWidth(100)]
    public float RotationAcceleration;
}
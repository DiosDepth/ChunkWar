using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameMiscConfig", menuName = "Configs/Main")]
public class GameMiscConfig : SerializedScriptableObject
{
    [HideReferenceObjectPicker]
    public BattleMiscRefreshConfig RefreshConfig = new BattleMiscRefreshConfig();

    [HideReferenceObjectPicker]
    public ShipClassConfig[] ShipClasses = new ShipClassConfig[0];

    [HideReferenceObjectPicker]
    public ShipControlConfig ShipControlCfg = new ShipControlConfig();

    public List<AchievementGroupItemConfig> AchievementGroupConfig = new List<AchievementGroupItemConfig>();
    public CollectionMenuKey[] CollectionMenu = new CollectionMenuKey[0];

    public PropertyModifyConfig[] EnergyOverloadBuff = new PropertyModifyConfig[0];
    public PropertyModifyConfig[] WreckageOverloadBuff = new PropertyModifyConfig[0];

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

#if UNITY_EDITOR

    [OnInspectorDispose]
    private void OnDispose()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endif
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
    /// 护卫舰
    /// </summary>
    Corvette,
    /// <summary>
    /// 驱逐舰
    /// </summary>
    Destroyer,
    /// <summary>
    /// 巡洋舰
    /// </summary>
    Cruiser,
    /// <summary>
    /// 战列巡洋舰
    /// </summary>
    BattleCruiser,
    /// <summary>
    /// 战列舰
    /// </summary>
    Battleship
}

public class BattleMiscRefreshConfig
{
    public int LevelUpRefreshCostBase;
    public int LevelUpCostWaveMultiple;
    public int LevelUpRefreshCountMultiple;

    public float Harbor_Teleport_RandomRangeMin;
    public float Harbor_Teleport_RandomRangeMax;
    public float Shop_Teleport_RandomRangeMin;
    public float Shop_Teleport_RandomRangeMax;
    public byte Shop_Teleport_StayTime;
    public byte Shop_Teleport_WarningTime;
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
    [LabelText("最大速度")]
    [LabelWidth(100)]
    public float MaxSpeed_Base;
    [LabelText("加速度")]
    [LabelWidth(100)]
    public float Acceleration_Base;
    [LabelText("旋转速度")]
    [LabelWidth(100)]
    public float MaxRotateSpeed_Base;
    [LabelText("旋转加速度")]
    [LabelWidth(100)]
    public float RotationAcceleration;
}
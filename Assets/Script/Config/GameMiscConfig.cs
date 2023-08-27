using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMiscConfig", menuName = "Configs/Main")]
public class GameMiscConfig : SerializedScriptableObject
{
    public List<AchievementGroupItemConfig> AchievementGroupConfig = new List<AchievementGroupItemConfig>();
    public CollectionMenuKey[] CollectionMenu = new CollectionMenuKey[0];

    public AchievementGroupItemConfig GetAchievementGroupConfig(AchievementGroupType type)
    {
        return AchievementGroupConfig.Find(x => x.Type == type);
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
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMiscConfig", menuName = "Configs/Main")]
public class GameMiscConfig : SerializedScriptableObject
{
    public List<AchievementGroupItemConfig> AchievementGroupConfig = new List<AchievementGroupItemConfig>();

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
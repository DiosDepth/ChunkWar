using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[GlobalConfig("EditorRes/DataOverview")]
public class AchievementDataOverview : GlobalConfig<AchievementDataOverview>
{
    private Dictionary<int, AchievementItemConfig> _refDic;

    public bool IsAchievementIDExists(int ID)
    {
        return _refDic.ContainsKey(ID);
    }

    public void AddInfoToRefDic(int ID, AchievementItemConfig info)
    {
        _refDic.Add(ID, info);
    }

    public AchievementItemConfig GetAchievementItem(int id)
    {
        if (_refDic.ContainsKey(id))
            return _refDic[id];

        return null;
    }

    public int GetNextIndex()
    {
        int index = 1;
        while (_refDic.ContainsKey(index))
        {
            index++;
        }
        return index;
    }

    [ReadOnly]
    [ListDrawerSettings(Expanded = true)]
    public AchievementItemConfig[] Achievements;

#if UNITY_EDITOR
    [Button("刷新", ButtonSizes.Medium), PropertyOrder(-1)]
    public void RefreshData()
    {
        Achievements = AssetDatabase.FindAssets("t:AchievementItemConfig")
           .Select(guid => AssetDatabase.LoadAssetAtPath<AchievementItemConfig>(AssetDatabase.GUIDToAssetPath(guid)))
           .ToArray();
    }

    public void RefreshAndLoad()
    {
        RefreshData();
        LoadData();
    }
#endif
    public void LoadData()
    {
        _refDic = new Dictionary<int, AchievementItemConfig>();
        if (Achievements == null)
            return;
        for (int i = 0; i < Achievements.Length; i++)
        {
            if (_refDic.ContainsKey(Achievements[i].AchievementID))
            {
                Debug.LogError("SameID ,ID = " + Achievements[i].AchievementID);
                continue;
            }

            _refDic.Add(Achievements[i].AchievementID, Achievements[i]);
        }
    }
}

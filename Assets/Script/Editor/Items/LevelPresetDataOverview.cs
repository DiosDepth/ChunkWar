using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[GlobalConfig("EditorRes/DataOverview")]
public class LevelPresetDataOverview : GlobalConfig<LevelPresetDataOverview>
{
    private Dictionary<int, LevelSpawnConfig> _refDic;

    public bool IsLevelPresetIDExists(int ID)
    {
        return _refDic.ContainsKey(ID);
    }

    public void AddInfoToRefDic(int ID, LevelSpawnConfig info)
    {
        _refDic.Add(ID, info);
    }

    public LevelSpawnConfig GetItem(int id)
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
    public LevelSpawnConfig[] Levels;

#if UNITY_EDITOR
    [Button("Ë¢ÐÂ", ButtonSizes.Medium), PropertyOrder(-1)]
    public void RefreshData()
    {
        Levels = AssetDatabase.FindAssets("t:LevelSpawnConfig")
           .Select(guid => AssetDatabase.LoadAssetAtPath<LevelSpawnConfig>(AssetDatabase.GUIDToAssetPath(guid)))
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
        _refDic = new Dictionary<int, LevelSpawnConfig>();
        if (Levels == null)
            return;
        for (int i = 0; i < Levels.Length; i++)
        {
            if (_refDic.ContainsKey(Levels[i].LevelPresetID))
            {
                Debug.LogError("SameID ,ID = " + Levels[i].LevelPresetID);
                continue;
            }

            _refDic.Add(Levels[i].LevelPresetID, Levels[i]);
        }
    }
}

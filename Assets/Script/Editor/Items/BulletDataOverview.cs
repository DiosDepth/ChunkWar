using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[GlobalConfig("EditorRes/DataOverview")]
public class BulletDataOverview : Sirenix.Utilities.GlobalConfig<BulletDataOverview>
{
    private Dictionary<int, BulletConfig> _refDic;

    public bool IsBulletIDExists(int ID)
    {
        return _refDic.ContainsKey(ID);
    }

    public void AddInfoToRefDic(int ID, BulletConfig info)
    {
        _refDic.Add(ID, info);
    }

    public BulletConfig GetBulletItem(int id)
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
    public BulletConfig[] Bullets;

#if UNITY_EDITOR
    [Button("Ë¢ÐÂ", ButtonSizes.Medium), PropertyOrder(-1)]
    public void RefreshData()
    {
        Bullets = AssetDatabase.FindAssets("t:BulletConfig")
           .Select(guid => AssetDatabase.LoadAssetAtPath<BulletConfig>(AssetDatabase.GUIDToAssetPath(guid)))
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
        _refDic = new Dictionary<int, BulletConfig>();
        if (Bullets == null)
            return;
        for (int i = 0; i < Bullets.Length; i++)
        {
            if (_refDic.ContainsKey(Bullets[i].ID))
            {
                Debug.LogError("SameID ,ID = " + Bullets[i].ID);
                continue;
            }

            _refDic.Add(Bullets[i].ID, Bullets[i]);
        }
    }
}

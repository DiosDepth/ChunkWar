using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class DropInfo
{
    public AvaliablePickUp pickuptype;
    public int count;
    public float dropRate;
}


public class BaseShipConfig : BaseConfig
{
    public List<DropInfo> DropList;

    /// <summary>
    /// Unit掉落稀有度权重
    /// </summary>
    public Dictionary<GoodsItemRarity, float> UnitDropRate = new Dictionary<GoodsItemRarity, float>
    {
        { GoodsItemRarity.Tier1, 0 },
        { GoodsItemRarity.Tier2, 0 },
        { GoodsItemRarity.Tier3, 0 },
        { GoodsItemRarity.Tier4, 0 }
    };

    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();

        _mapSize = GetMapSize();
    }
    protected override Vector2Int GetMapPivot()
    {
        if (Map == null)
            return Vector2Int.zero;
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (Map[x, y] == 1)
                {
                    return GameHelper.CoordinateArrayToMap(new Vector2Int(x, y), GameGlobalConfig.ShipMapSize);
                }
            }
        }
        return Vector2Int.zero;
    }



    public override Vector2 GetMapSize()
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        Vector2Int tempvalue;
        for (int row = 0; row < Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < Map.GetLength(1); colume++)
            {
                if (Map[row, colume] != 0)
                {
                    tempvalue = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                    if (tempvalue.x > max.x)
                    {
                        max.x = tempvalue.x;
                    }
                    else if (tempvalue.x < min.x)
                    {
                        min.x = tempvalue.x;
                    }
                    if (tempvalue.y > max.y)
                    {
                        max.y = tempvalue.y;
                    }
                    else if (tempvalue.y < min.y)
                    {
                        min.y = tempvalue.y;
                    }
                }
            }
        }
        return max - min + Vector2.one;
    }

    [OnInspectorInit]
    protected override void InitData()
    {
        base.InitData();
        if (Map == null)
        {
            Map = new int[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}

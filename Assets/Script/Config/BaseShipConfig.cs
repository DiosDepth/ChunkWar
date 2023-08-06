using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BaseShipConfig : BaseConfig
{



    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
        _mapSize = GetShipSize();
    }
    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }

    public virtual Vector2 GetShipSize()
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        Vector2Int tempvalue;
        for (int row = 0; row < Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < Map.GetLength(1); colume++)
            {
                if(Map[row, colume] != 0)
                {
                    tempvalue = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                    if(tempvalue.x > max.x)
                    {
                        max.x = tempvalue.x;
                    }
                    else if(tempvalue.x < min.x)
                    {
                        min.x = tempvalue.x;
                    }
                    if(tempvalue.y > max.y)
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

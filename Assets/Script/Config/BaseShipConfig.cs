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
    public float weight;
}


public class BaseShipConfig : BaseConfig
{


    public List<DropInfo> DropList;
    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
        _mapSize = GetMapSize();
    }
    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }


    public override Vector2 GetMapSize()
    {
        return base.GetMapSize();
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

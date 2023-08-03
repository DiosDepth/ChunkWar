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
    }
    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
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

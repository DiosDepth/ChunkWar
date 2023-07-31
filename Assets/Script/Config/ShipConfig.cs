using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Configs_Ship_", menuName = "Configs/Unit/ShipConfig")]
public class ShipConfig : BaseConfig
{
    [LabelText("��������")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public ShipType ShipType;

    [LabelText("������ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int MainWeaponID;

    [LabelText("���Ĳ��ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int CorePlugID;


    protected override Vector2Int GetMapPivot()
    {
        //base.GetMapPivot();
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
    [OnInspectorInit]
    private void InitData()
    {
        if (Map == null) 
        {
            Map = new int[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }

}

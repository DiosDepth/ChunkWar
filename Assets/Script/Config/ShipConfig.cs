using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Configs_Ship_", menuName = "Configs/Unit/ShipConfig")]
public class ShipConfig : BaseConfig
{
    [LabelText("Ω¢¥¨¿‡–Õ")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public ShipType ShipType;


    public AvalibalMainWeapon MainWeapon;

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

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Configs_PlayerShip_", menuName = "Configs/Unit/PlayerShipConfig")]
public class PlayerShipConfig : BaseShipConfig
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

    }

}

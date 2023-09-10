using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Configs_PlayerShip_", menuName = "Configs/Unit/PlayerShipConfig")]
public class PlayerShipConfig : BaseShipConfig
{

    [LabelText("主武器ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int MainWeaponID;

    [LabelText("核心插件ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int CorePlugID;

    [LabelText("阵营ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 200)]
    public int PlayerShipCampID;

    [LabelText("EditorPrefab")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public GameObject EditorPrefab;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameGlobalConfig
{
    /// <summary>
    /// max ship size 
    /// </summary>
    public const int ShipMaxSize = 21;
    /// <summary>
    /// map size base on center of ship mas size
    /// </summary>
    public const int ShipMapSize = (ShipMaxSize - 1) / 2;

    public const int ShipSlotPix = 44;

    /// <summary>
    /// building max size
    /// </summary>
    public const int UnitMaxSize = 11;

    /// <summary>
    ///  map size base on center of building max size
    /// </summary>
    public const int UnitMapSize = (UnitMaxSize - 1) / 2;

    public const uint PropertyModifyUID_ShipClass = 1;
    public const uint PropertyModifyUID_EnergyOverload_GlobalBuff = 2;
    public const uint PropertyModifyUID_WreckageOverload_GlobalBuff = 3;

    /// <summary>
    /// 舰船速度最大比例
    /// </summary>
    public const float ShipSpeedModify_Protected_MaxSpeed = 10;

    public const int BuildingShadowMaxSize = 21;
    public const int BuildingShadowMapSize = (BuildingShadowMaxSize - 1) / 2;
    public const string VFXPath = "Prefab/VFX/";
    public const string SFXPath = "Prefab/SFX/";
    public const string AIShipPath = "Prefab/Ship/AI/";
    public const string AIFactoryPath = "Prefab/GameplayPrefab/AIFactory";

}

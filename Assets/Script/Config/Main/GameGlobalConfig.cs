using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameGlobalConfig
{
    public const int CameraDefault_OrthographicSize = 40;

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

    public const int HardLevelWaveIndexMultiple = 1000;
    public const int HardLevelMultiple_TimeDelta = 30;

    /// <summary>
    ///  map size base on center of building max size
    /// </summary>
    public const int UnitMapSize = (UnitMaxSize - 1) / 2;

    public const uint PropertyModifyUID_ShipClass = 1;
    public const uint PropertyModifyUID_EnergyOverload_GlobalBuff = 2;
    public const uint PropertyModifyUID_WreckageOverload_GlobalBuff = 3;
    public const uint PropertyModifyUID_HardLevel = 4;

    /// <summary>
    /// 舰船速度最大比例
    /// </summary>
    public const float ShipSpeedModify_Protected_MaxSpeed = 10;
    public const byte ShopGoods_MaxCount = 5;
    /// <summary>
    /// 最大伤害数量
    /// </summary>
    public const byte DamageCount_ProtectMax = 99;

    public const byte Ship_MaxLevel = 99;
    public const byte MeteoriteGenerate_MaxCount = 100;
    public const byte AncientUnit_Generate_MaxCount = 30;
    public const byte AIShipFactory_SpawnMaxCount = 30;
    public const byte LevelSector_Count = 18;
    /// <summary>
    /// 相邻扇区Index
    /// </summary>
    public const byte LevelSector_AdjacentCount = 2;
    public const byte SectorThread_UpdateSecond = 3;

    public const int BuildingShadowMaxSize = 21;
    public const int BuildingShadowMapSize = (BuildingShadowMaxSize - 1) / 2;
    public const string VFXPath = "Prefab/VFX/";
    public const string SFXPath = "Prefab/SFX/";
    public const string AIShipPath = "Prefab/Ship/AI/";
    public const string DroneShipPath = "Prefab/Ship/Drones/";
    public const string AIShipSpawnAgentPath = "Prefab/SpawnAgent/AIShipSpawnAgent";
    public const string PlayerDroneSpawnAgentPath = "Prefab/SpawnAgent/PlayerBaseDroneSpawnAgent";
    public const string ShieldPath = "Prefab/Unit/Items/";

}

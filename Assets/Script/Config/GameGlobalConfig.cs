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


    /// <summary>
    /// building max size
    /// </summary>
    public const int BuildingMaxSize = 21;

    /// <summary>
    ///  map size base on center of building max size
    /// </summary>
    public const int BuildingMapSize = (BuildingMaxSize - 1) / 2;


    public const int BuildingShadowMaxSize = 21;
    public const int BuildingShadowMapSize = (BuildingShadowMaxSize - 1) / 2;


}

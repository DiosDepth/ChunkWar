using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BuildingConfig : BaseUnitConfig
{

    [OnInspectorInit]
    private void InitData()
    {
        if (Map == null)
        {
            Map = new int[GameGlobalConfig.BuildingMaxSize, GameGlobalConfig.BuildingMaxSize];
        }
    }

}

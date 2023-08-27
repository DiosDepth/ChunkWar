using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Configs_Building_", menuName = "Configs/Unit/BuildingConfig")]
public class BuildingConfig : BaseUnitConfig
{
    [LabelText("»¤¶Ü×é¼þÅäÖÃ")]
    public BuildingShieldConfig ShieldConfig = new BuildingShieldConfig();


}

[System.Serializable]
public class BuildingShieldConfig
{
    public bool GenerateShield;

    public int ShieldHP;
    public int ShieldRecoverValue;
    public float ShieldRecoverTime;
    public float ShieldBaseRatio;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("Battle Log Export Data")]
[CSVComment("Comment")]
public class BattleLog
{
    [CSVTag("Game_Version")]
    public string GameVersion { get; set; }

    [CSVTag("Start_System_Time")]
    public string StartTime { get; set; }
    [CSVTag("End_System_Time")]
    public string EndTime { get; set; }

    [CSVTag("Cost Seconds")]
    public int TotalCostSeconds { get; set; }

    [CSVTag("Battle Finish State")]
    public string BattleState { get; set; }

    [CSVTag("Max Reach Wave")]
    public int MaxReachWave { get; set; }

    [CSVTag("HardLevel")]
    public int HardLevelID { get; set; }

    [CSVTag("Ship Info")]
    public string SelectShip { get; set; }

    [CSVTag("MainWeaponInfo")]
    public string SelectMainWeapon { get; set; }

    [CSVTag("Shop Enter Count")]
    public int TotalShopEnterCount;

    [CSVTag("Total Waste Count")]
    public int TotalWaste;

    [CSVTag("Gain Wreckage Info")]
    public string TotalWreckageGain;

    [CSVTag("Gain Currency")]
    public int TotalGainCurrency;

    [CSVTag("Cost Currency")]
    public int TotalCostCurrency;

    [CSVTag("Ship Level")]
    public byte ShipLevel;

    [CSVTag("Player Equipment Info")]
    public List<UnitLogData> UnitLogDatas { get; set; }

    [CSVTag("Plug Gain Log")]
    public List<ShipPlugLogData> ShipPlugLog { get; set; }

    [CSVTag("Player Property Data")]
    public List<PropertyLogData> PlayerPropertyDatas { get; set; }

}

public class UnitLogData 
{
    [CSVTag("UnitID")]
    public int UnitID { get; set; }

    [CSVTag("UID")]
    public uint UID { get; set; }

    [CSVTag("Unit_Name")]
    public string UnitName { get; set; }

    [CSVTag("Create_Wave")]
    public int CreateWave { get; set; }

    [CSVTag("Remov_Wave")]
    public int RemoveWave { get; set; }

    [CSVTag("Total_Damage")]
    public int DamageCreateTotal { get; set; }

    [CSVTag("Total_Damage_Take")]
    public int DamageTakeTotal { get; set; }
}

public class PropertyLogData
{
    [CSVTag("Key")]
    public PropertyModifyKey ModifyKey { get; set; }

    [CSVTag("Name")]
    public string PropertyName { get; set; }

    [CSVTag("Value")]

    public float Value { get; set; }
}

public class ShipPlugLogData
{
    [CSVTag("ID")]
    public int PlugID { get; set; }

    [CSVTag("Name")]
    public string PlugName { get;set; }

    [CSVTag("GainCount")]
    public byte PlugGainCount { get; set; }

    [CSVTag("GainWaveInfo")]
    public string GainWaveCountList { get; set; }

    [CSVTag("GainCurrencyCostInfo")]
    public string GainCostList { get; set; }
}
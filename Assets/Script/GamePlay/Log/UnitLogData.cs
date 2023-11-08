using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("Battle Log Export Data")]
[CSVComment("Comment")]
public class BattleLog
{
    [CSVTag("Start_System_Time")]
    public string StartTime { get; set; }
    [CSVTag("End_System_Time")]
    public string EndTime { get; set; }

    [CSVTag("Player Equipment Info")]
    public List<UnitLogData> UnitLogDatas { get; set; }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("战斗信息导出")]
[CSVComment("备注")]
public class BattleLog
{
    [CSVTag("开始时间")]
    public string StartTime { get; set; }
    [CSVTag("结束时间")]
    public string EndTime { get; set; }

    [CSVTag("玩家装备信息")]
    public List<UnitLogData> UnitLogDatas { get; set; }

}

public class UnitLogData 
{
    [CSVTag("UnitID")]
    public int UnitID { get; set; }

    [CSVTag("UID")]
    public uint UID { get; set; }

    [CSVTag("名称")]
    public string UnitName { get; set; }

    [CSVTag("创建波次")]
    public int CreateWave { get; set; }

    [CSVTag("移除波次")]
    public int RemoveWave { get; set; }

    [CSVTag("累积伤害")]
    public int DamageCreateTotal { get; set; }

    [CSVTag("累积承受伤害")]
    public int DamageTakeTotal { get; set; }
}

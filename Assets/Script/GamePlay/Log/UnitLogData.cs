using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("战斗信息导出")]
[CSVComment("备注")]
public class BattleLog
{
    [CSVTag("开始时间")]
    public string StartTime;
    [CSVTag("结束时间")]
    public string EndTime;

    [CSVTag("玩家装备信息")]
    public List<UnitLogData> UnitLogDatas = new List<UnitLogData>();

}

public class UnitLogData 
{
    [CSVTag("UnitID")]
    public int UnitID;

    [CSVTag("UID")]
    public uint UID;

    [CSVTag("名称")]
    public string UnitName;

    [CSVTag("创建波次")]
    public int CreateWave;

    [CSVTag("移除波次")]
    public int RemoveWave;

    [CSVTag("累积伤害")]
    public int DamageCreateTotal;

    [CSVTag("累积承受伤害")]
    public int DamageTakeTotal;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("ս����Ϣ����")]
[CSVComment("��ע")]
public class BattleLog
{
    [CSVTag("��ʼʱ��")]
    public string StartTime { get; set; }
    [CSVTag("����ʱ��")]
    public string EndTime { get; set; }

    [CSVTag("���װ����Ϣ")]
    public List<UnitLogData> UnitLogDatas { get; set; }

}

public class UnitLogData 
{
    [CSVTag("UnitID")]
    public int UnitID { get; set; }

    [CSVTag("UID")]
    public uint UID { get; set; }

    [CSVTag("����")]
    public string UnitName { get; set; }

    [CSVTag("��������")]
    public int CreateWave { get; set; }

    [CSVTag("�Ƴ�����")]
    public int RemoveWave { get; set; }

    [CSVTag("�ۻ��˺�")]
    public int DamageCreateTotal { get; set; }

    [CSVTag("�ۻ������˺�")]
    public int DamageTakeTotal { get; set; }
}

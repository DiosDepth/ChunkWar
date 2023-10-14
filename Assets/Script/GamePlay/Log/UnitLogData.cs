using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CSVTag("ս����Ϣ����")]
[CSVComment("��ע")]
public class BattleLog
{
    [CSVTag("��ʼʱ��")]
    public string StartTime;
    [CSVTag("����ʱ��")]
    public string EndTime;

    [CSVTag("���װ����Ϣ")]
    public List<UnitLogData> UnitLogDatas = new List<UnitLogData>();

}

public class UnitLogData 
{
    [CSVTag("UnitID")]
    public int UnitID;

    [CSVTag("UID")]
    public uint UID;

    [CSVTag("����")]
    public string UnitName;

    [CSVTag("��������")]
    public int CreateWave;

    [CSVTag("�Ƴ�����")]
    public int RemoveWave;

    [CSVTag("�ۻ��˺�")]
    public int DamageCreateTotal;

    [CSVTag("�ۻ������˺�")]
    public int DamageTakeTotal;
}

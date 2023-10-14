using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * װ��ͳ��
 */
public class UnitStatisticsData 
{
    public uint OwnerUID;

    /// <summary>
    /// �ۼ���ɵ��˺�
    /// </summary>
    public int DamageCreateTotal;

    public int DamageTakeTotal;

    public int CreateWaveIndex;
    public int RemoveWaveIndex;

    public Dictionary<int, int> WaveCreateDamageTotal;
    public Dictionary<int, int> WaveDamageTakeTotal;

    public UnitStatisticsData(uint uid)
    {
        OwnerUID = uid;
        WaveCreateDamageTotal = new Dictionary<int, int>();
        WaveDamageTakeTotal = new Dictionary<int, int>();
    }

    /// <summary>
    /// ���˺�ͳ��
    /// </summary>
    /// <param name="waveIndex"></param>
    /// <param name="damage"></param>
    public void AddDamageTotal(int waveIndex, int damage)
    {
        DamageCreateTotal += damage;
        if (WaveCreateDamageTotal.ContainsKey(waveIndex))
        {
            WaveCreateDamageTotal[waveIndex] += damage;
        }
        else
        {
            WaveCreateDamageTotal.Add(waveIndex, damage);
        }
    }

    /// <summary>
    /// �ܳ����˺�ͳ��
    /// </summary>
    /// <param name="waveIndex"></param>
    /// <param name="damage"></param>
    public void AddDamageTakeTotal(int waveIndex, int damage)
    {
        DamageTakeTotal += damage;
        if (WaveDamageTakeTotal.ContainsKey(waveIndex))
        {
            WaveDamageTakeTotal[waveIndex] += damage;
        }
        else
        {
            WaveDamageTakeTotal.Add(waveIndex, damage);
        }
    }
}

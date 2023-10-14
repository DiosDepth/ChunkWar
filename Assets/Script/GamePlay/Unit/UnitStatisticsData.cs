using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 装备统计
 */
public class UnitStatisticsData 
{
    public uint OwnerUID;

    /// <summary>
    /// 累计造成的伤害
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
    /// 总伤害统计
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
    /// 总承受伤害统计
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

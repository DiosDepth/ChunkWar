using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * �ؿ�����
 */
public class LevelDataInfo 
{
    /// <summary>
    /// �ؿ�����
    /// </summary>
    public int LevelWaveIndex
    {
        get;
        private set;
    }

    private LevelData _levelCfg;

    public static LevelDataInfo CreateInfo(LevelData cfgData)
    {
        LevelDataInfo info = new LevelDataInfo();
        info._levelCfg = cfgData;
        info.InitLevel();
        return info;
    }

    private void InitLevel()
    {
        LevelWaveIndex = 1;
    }

    public bool IsLastWave()
    {
        return LevelWaveIndex > DataManager.Instance.battleCfg.RogueNormal_Max_Wave;
    }

    public void TryEnterNextWave()
    {
        LevelWaveIndex++;
        if (IsLastWave())
            return;
    }
}

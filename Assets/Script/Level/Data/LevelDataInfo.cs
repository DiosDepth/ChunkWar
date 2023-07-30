using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 关卡数据
 */
public class LevelDataInfo 
{
    /// <summary>
    /// 关卡波次
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
        return false;
    }

    public void TryEnterNextWave()
    {
        LevelWaveIndex++;
        if (IsLastWave())
            return;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardLevelInfo 
{
    public int HardLevelID;
    public HardLevelConfig Cfg;

    private LevelSpawnConfig _spawnConfig;

    public bool Unlock
    {
        get { return _unlock; }
    }
    private bool _unlock = true;

    public string HardLevelName
    {
        get { return LocalizationManager.Instance.GetTextValue(Cfg.Name); }
    }

    public string HardLevelDesc
    {
        get { return LocalizationManager.Instance.GetTextValue(Cfg.Desc); }
    }

    public string UnlockDesc
    {
        get
        {
            if (string.IsNullOrEmpty(Cfg.UnlockDesc))
                return string.Empty;

            return LocalizationManager.Instance.GetTextValue(Cfg.UnlockDesc);
        }
    }

    /// <summary>
    /// 机制描述
    /// </summary>
    public string PropertyDesc
    {
        get
        {
            if (string.IsNullOrEmpty(Cfg.PropertyDesc))
                return string.Empty;

            return LocalizationManager.Instance.GetTextValue(Cfg.PropertyDesc);
        }
    }

    /// <summary>
    /// 波次数量
    /// </summary>
    public int WaveCount
    {
        get
        {
            return _spawnConfig.WaveConfig.Count;
        }
    }

    public HardLevelInfo(HardLevelConfig cfg)
    {
        _spawnConfig = DataManager.Instance.GetLevelSpawnConfig(cfg.LevelPresetID);
        HardLevelID = cfg.HardLevelID;
        Cfg = cfg;
    }

    public HardLevelInfo(int hardLevelID)
    {
        var hardLevelCfg = DataManager.Instance.battleCfg.GetHardLevelConfig(hardLevelID);
        if (hardLevelCfg == null)
            return;

        _spawnConfig = DataManager.Instance.GetLevelSpawnConfig(hardLevelCfg.LevelPresetID);
        HardLevelID = hardLevelID;
        Cfg = hardLevelCfg;
    }

    public WaveConfig GetWaveConfig(int waveIndex)
    {
        var waveConfigs = _spawnConfig.WaveConfig;

        return waveConfigs.Find(x => x.WaveIndex == waveIndex);
    }

    /// <summary>
    /// 刷新每个舰船的hardLevel信息
    /// </summary>
    /// <param name="shipID"></param>
    public void RefreshShipHardLevel(int shipID)
    {

    }
}

public class EnemyHardLevelData
{
    public int GroupID;
    private Dictionary<int, EnemyHardLevelItem> _hardLevelItems = new Dictionary<int, EnemyHardLevelItem>();

    public void AddHardLevelItems(int index, EnemyHardLevelItem item)
    {
        if (!_hardLevelItems.ContainsKey(index))
        {
            _hardLevelItems.Add(index, item);
        }
    }

    public EnemyHardLevelItem GetHardLevelItemByIndex(int index)
    {
        EnemyHardLevelItem result = null;
        _hardLevelItems.TryGetValue(index, out result);
        Debug.Assert(result != null, "GetHardLevelItemByIndex Null! ID= " + index);
        return result;
    }
}
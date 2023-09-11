using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CampData 
{
    public int CampID;

    public CampConfig Config;

    private bool _unlock;
    public bool Unlock
    {
        get { return _unlock; }
    }

    public string CampName
    {
        get { return LocalizationManager.Instance.GetTextValue(Config.CampName); }
    }

    public string CampDesc
    {
        get { return LocalizationManager.Instance.GetTextValue(Config.CampDesc); }
    }

    private byte _campLevel = 0;
    public byte GetCampLevel
    {
        get { return _campLevel; }
    }

    public int CurrentRemainScore
    {
        get;
        private set;
    }

    private ChangeValue<int> CampTotalScore;

    /// <summary>
    /// 阵营总积分
    /// </summary>
    public int TotalScore
    {
        get { return CampTotalScore.Value; }
    }

    private int _currentUpgradeRequireEXP;
    private int _currentEXP;

    private CampLevelConfig[] LevelConfigs;

    public Dictionary<PropertyModifyKey, byte> BuffLevelMap
    {
        get;
        private set;
    }

    public static CampData CreateData(CampConfig cfg)
    {
        CampData data = new CampData();
        data.Config = cfg;
        data.CampID = cfg.CampID;
        data._unlock = cfg.Unlock;
        data.Init();
        return data;
    }

    private void Init()
    {
        CampTotalScore = new ChangeValue<int>(0, 0, int.MaxValue);
        LevelConfigs = Config.LevelConfigs.OrderBy(x => x.LevelIndex).ToArray();
        BuffLevelMap = new Dictionary<PropertyModifyKey, byte>();
        for(int i = 0; i < Config.BuffItems.Length; i++)
        {
            var buffCfg = Config.BuffItems[i];
            if (!BuffLevelMap.ContainsKey(buffCfg.ModifyKey))
            {
                BuffLevelMap.Add(buffCfg.ModifyKey, 1);
            }
        }
        RefreshUpgradeEXP();
    }

    public float GetCurrentLevelEXPProgress()
    {
        if (IsMaxCampLevel)
            return 1;
        return _currentEXP / (float)_currentUpgradeRequireEXP;
    }

    public List<uint> GenerateBuffItemUIDs()
    {
        List<uint> result = new List<uint>();
        foreach(var item in BuffLevelMap)
        {
            var uid = (uint)(CampID * 10000 + (int)item.Key);
            result.Add(uid);
        }
        return result;
    }

    public List<uint> GenerateLevelItemUIDs()
    {
        List<uint> result = new List<uint>();
        for(int i = 0; i < Config.LevelConfigs.Length; i++)
        {
            var uid = (uint)(CampID * 10000 + Config.LevelConfigs[i].LevelIndex);
            result.Add(uid);
        }
        return result;
    }

    public CampLevelConfig GetLevelConfig(int levelIndex)
    {
        var cfgs = Config.LevelConfigs;
        for(int i = 0; i < cfgs.Length; i++)
        {
            if (cfgs[i].LevelIndex == levelIndex)
                return cfgs[i];
        }

        return null;
    }

    /// <summary>
    /// 是否最大等级
    /// </summary>
    public bool IsMaxCampLevel
    {
        get { return _campLevel >= Config.LevelConfigs.Length; }
    }

    public void AddCampScore(int value)
    {
        var newScore = CampTotalScore.Value + value;
        CampTotalScore.Set(newScore);
        CurrentRemainScore += value;

        int safeLoop = 0;

        if (!IsMaxCampLevel)
        {
            _currentEXP += value;
            while (_currentEXP >= _currentUpgradeRequireEXP && safeLoop <= 50)
            {
                safeLoop++;
                _currentEXP -= _currentUpgradeRequireEXP;
                LevelUp();

                if (IsMaxCampLevel)
                    break;
            }
        }
    }

    /// <summary>
    /// 获取修正等级
    /// </summary>
    /// <param name="key"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public float GetValueModify(PropertyModifyKey key, byte level)
    {
        var buffCfgs = Config.BuffItems;
        for (int i = 0; i < buffCfgs.Length; i++) 
        {
            if(buffCfgs[i].ModifyKey == key)
            {
                var levelMap = buffCfgs[i].LevelMap;
                if (levelMap.Length < level)
                    return 0;

                return levelMap[level - 1];
            }
        }
        return 0;
    }



    public int GetCurrentCostValue(PropertyModifyKey key)
    {
        if (!BuffLevelMap.ContainsKey(key))
            return 0;

        return GetCostValue(key, BuffLevelMap[key]);
    }

    /// <summary>
    /// 获取花费等级
    /// </summary>
    /// <param name="key"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetCostValue(PropertyModifyKey key, byte level)
    {
        var buffCfgs = Config.BuffItems;
        for (int i = 0; i < buffCfgs.Length; i++)
        {
            if (buffCfgs[i].ModifyKey == key)
            {
                var costMap = buffCfgs[i].CostMap;
                if (costMap.Length < level)
                    return 0;

                return costMap[level - 1];
            }
        }
        return 0;
    }

    private void LevelUp()
    {
        if (IsMaxCampLevel)
            return;

        _campLevel++;
        RefreshUpgradeEXP();
    }

    private void RefreshUpgradeEXP()
    {
        if (IsMaxCampLevel)
        {
            _currentUpgradeRequireEXP = 0;
            return;
        }

        _currentUpgradeRequireEXP = LevelConfigs[GetCampLevel].RequireTotalEXP;
    }

    #region CampBuff

    /// <summary>
    /// BUFF是否最高等级
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsMaxBuffLevel(PropertyModifyKey key)
    {
        if (!BuffLevelMap.ContainsKey(key))
            return true;

        var cfg = Config.GetBuffItem(key);
        if (cfg == null)
            return true;

        return BuffLevelMap[key] >= cfg.LevelMap.Length;
    }

    /// <summary>
    /// BUFF升级点数是否足够
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool CheckBuffUpgradeCostEnough(PropertyModifyKey key)
    {
        if (!BuffLevelMap.ContainsKey(key))
            return false;

        var buffCfg = Config.GetBuffItem(key);
        if (buffCfg == null)
            return false;

        var currentLevel = BuffLevelMap[key];
        ///CheckMax Level
        if (BuffLevelMap[key] >= buffCfg.LevelMap.Length)
            return true;

        int cost = buffCfg.CostMap[currentLevel - 1];
        return CurrentRemainScore >= cost;
    }

    public void GetNextBuffUpgradeInfo(PropertyModifyKey key, out int nextLevel, out float nextValue)
    {
        nextLevel = -1;
        nextValue = -1;

        if (!BuffLevelMap.ContainsKey(key) || IsMaxBuffLevel(key))
            return;

        var buffCfg = Config.GetBuffItem(key);
        if (buffCfg == null)
            return;

        var oldLevel = BuffLevelMap[key];
        nextLevel = oldLevel + 1;
        nextValue = buffCfg.LevelMap[nextLevel - 1];
    }

    public bool UpgradeBuff(PropertyModifyKey key, out CampBuffUpgradeDialog.BuffUpgradeInfo info)
    {
        if (!BuffLevelMap.ContainsKey(key) || IsMaxBuffLevel(key))
        {
            info = default(CampBuffUpgradeDialog.BuffUpgradeInfo);
            return false;
        }

        var buffCfg = Config.GetBuffItem(key);
        if (buffCfg == null) 
        {
            info = default(CampBuffUpgradeDialog.BuffUpgradeInfo);
            return false;
        }

        var oldLevel = BuffLevelMap[key];
        float oldValue = buffCfg.LevelMap[oldLevel - 1];
        int oldCost = buffCfg.CostMap[oldLevel - 1];
        ///Reduce Score
        if (CurrentRemainScore < oldCost)
        {
            info = default(CampBuffUpgradeDialog.BuffUpgradeInfo);
            return false;
        }
        CurrentRemainScore -= oldCost;

        BuffLevelMap[key]++;
        float newValue = buffCfg.LevelMap[oldLevel];

        info = new CampBuffUpgradeDialog.BuffUpgradeInfo()
        {
            CampName = LocalizationManager.Instance.GetTextValue(Config.CampName),
            currentValue = oldValue,
            nextValue = newValue,
            PropertyKey = key
        };
        RogueEvent.Trigger(RogueEventType.CampBuffUpgrade);
        return true;
    }

    #endregion
}

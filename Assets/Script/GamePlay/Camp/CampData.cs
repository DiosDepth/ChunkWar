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

    private byte _campLevel = 1;
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
    }

    public float GetCurrentLevelEXPProgress()
    {
        var levelCfg = LevelConfigs[GetCampLevel - 1];
        var delta = TotalScore - levelCfg.RequireTotalEXP;

        int preEXP = 0;
        byte preLevel = (byte)(GetCampLevel - 1);
        if(preLevel > 0)
        {
            preEXP = LevelConfigs[preLevel - 1].RequireTotalEXP;
        }

        return delta / (levelCfg.RequireTotalEXP - preEXP);
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
        var delta = CheckLevelUp();
        if(delta > 0)
        {
            for(int i = 0; i < delta; i++)
            {
                LevelUp();
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

    private int CheckLevelUp()
    {
        CampLevelConfig outCfg = null;
        for (int i = LevelConfigs.Length - 1; i >= 0; i--) 
        {
            var cfg = LevelConfigs[i];
            if(CampTotalScore.Value >= cfg.RequireTotalEXP)
            {
                outCfg = cfg;
                break;
            }
        }

        if (outCfg == null)
            return 0;

        return outCfg.LevelIndex - _campLevel;
    }

    private void LevelUp()
    {
        if (IsMaxCampLevel)
            return;

        _campLevel++;
        
    }
}

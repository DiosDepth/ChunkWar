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

    public int CurrentUseScore
    {
        get;
        private set;
    }

    private ChangeValue<int> CampTotalScore;

    private CampLevelConfig[] LevelConfigs;

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
        CurrentUseScore += value;
        var delta = CheckLevelUp();
        if(delta > 0)
        {
            for(int i = 0; i < delta; i++)
            {
                LevelUp();
            }
        }
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

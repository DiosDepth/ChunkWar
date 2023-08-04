using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardLevelInfo 
{
    public int HardLevelID;
    public HardLevelConfig Cfg;

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

    public HardLevelInfo(HardLevelConfig cfg)
    {
        HardLevelID = cfg.HardLevelID;
        Cfg = cfg;
    }

    public HardLevelInfo(int hardLevelID)
    {
        var hardLevelCfg = DataManager.Instance.battleCfg.GetHardLevelConfig(hardLevelID);
        if (hardLevelCfg == null)
            return;

        HardLevelID = hardLevelID;
        Cfg = hardLevelCfg;
    }

    /// <summary>
    /// 刷新每个舰船的hardLevel信息
    /// </summary>
    /// <param name="shipID"></param>
    public void RefreshShipHardLevel(int shipID)
    {

    }
}

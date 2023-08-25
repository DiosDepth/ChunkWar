using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AchievementEventType
{
    UI_GroupSelected,
    UI_ShowAchievementFinish,
}

public enum GameStatisticsType
{
    Int_Sum,
    Int_Max,
    Bool
}

public struct AchievementEvent
{
    public AchievementEventType evtType;
    public object[] param;

    public AchievementEvent(AchievementEventType m_type, params object[] param)
    {
        this.evtType = m_type;
        this.param = param;
    }

    public static void Trigger(AchievementEventType m_type, params object[] param)
    {
        AchievementEvent evt = new AchievementEvent(m_type, param);
        EventCenter.Instance.TriggerEvent<AchievementEvent>(evt);
    }
}

[System.Serializable]
public class AchievementSaveData
{
    public int AchievementID;
    public bool Unlock;
    public string FinishTime;
}

/// <summary>
/// 统计数据
/// </summary>
public class GameStatisticsData
{
    public GameStatisticsType StatisticsType;

    public ChangeValue<int> ValueInt;

    public bool ValueBool = false;

    private List<AchievementItemConfig> watcherAchievements;

    public GameStatisticsData(GameStatisticsType type)
    {
        this.StatisticsType = type;
        ValueInt = new ChangeValue<int>(0, 0, int.MaxValue);
        watcherAchievements = new List<AchievementItemConfig>();
    }

    public void AddWatcher(AchievementItemConfig cfg)
    {
        if (!watcherAchievements.Contains(cfg))
        {
            watcherAchievements.Add(cfg);
        }
    }

    public void AddValue(int value)
    {
        int newValue = ValueInt.Value + value;
        ValueInt.Set(newValue);
        CheckAchievementVaild();
    }

    public void SetValue(bool value)
    {
        if(ValueBool != value)
        {
            ValueBool = value;
            CheckAchievementVaild();
        }
    }

    public void SetValueMax(int value)
    {
        if (ValueInt.Value <= value)
            return;

        ValueInt.Set(value);
        CheckAchievementVaild();
    }

    private void CheckAchievementVaild()
    {
        for (int i = 0; i < watcherAchievements.Count; i++) 
        {
            AchievementManager.Instance.CheckAchievementFinish(watcherAchievements[i]);
        }
    }
}

public class AchievementManager : Singleton<AchievementManager>
{

    private Dictionary<AchievementWatcherType, IAchievementWatcher> _watcherDic;

    private Dictionary<string, GameStatisticsData> _game_statistics_data;

    public AchievementManager()
    {
       
    }


    public override void Initialization()
    {
        base.Initialization();
        _watcherDic = new Dictionary<AchievementWatcherType, IAchievementWatcher>();
        _game_statistics_data = new Dictionary<string, GameStatisticsData>();
        BindWatcherListener();
    }

    /// <summary>
    /// 触发Watcher
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="content"></param>
    public void Trigger<T>(AchievementWatcherType type, T content)
    {
        if (_watcherDic.ContainsKey(type))
        {
            var watcher = _watcherDic[type] as AchievementWatcher<T>;
            if(watcher != null)
            {
                watcher.onValueChange?.Invoke(content);
            }
        }
    }

    public void AddWatcherListener<T>(AchievementWatcherType type, Action<T> action)
    {
        if (_watcherDic.ContainsKey(type))
        {
            (_watcherDic[type] as AchievementWatcher<T>).onValueChange += action;
        }
        else
        {
            _watcherDic.Add(type, new AchievementWatcher<T>(action, type));
        }
    }

    /// <summary>
    /// 成就是否完成
    /// </summary>
    /// <param name="cfg"></param>
    public void CheckAchievementFinish(AchievementItemConfig cfg)
    {
        ///CheckUnlock 如果已经解锁，跳过
        var sav = SaveLoadManager.Instance.GetAchievementSaveDataByID(cfg.AchievementID);
        if (sav == null || sav.Unlock)
            return;


        bool result = true;
        var con = cfg.Conditions;
        for(int i = 0; i < con.Count; i++)
        {
            if(cfg.ListType == AchievementConLstType.And)
            {
                result &= AchievementConditionResult(con[i]);
            }
            else
            {
                result |= AchievementConditionResult(con[i]);
            }
        }

        if (result)
        {
            ///Finish
            Debug.Log(string.Format("Achievement Finish ! ID = {0}, Name = {1}, Desc = {2}", cfg.AchievementID, LocalizationManager.Instance.GetTextValue(cfg.AchievementName), LocalizationManager.Instance.GetTextValue(cfg.AchievementDesc)));

            ///DisplayUI
            AchievementEvent.Trigger(AchievementEventType.UI_ShowAchievementFinish, cfg);

            sav.Unlock = true;
            sav.FinishTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }

    private void BindWatcherListener()
    {
        AddWatcherListener<BaseShip>(AchievementWatcherType.EnemyKill, Watcher_OnEnmeyDie);
        RegisterAchievementWatcher();
    }

    private void RegisterAchievementWatcher()
    {
        var allAchievement = DataManager.Instance.GetAllAchievementConfigs();
        for (int i = 0; i < allAchievement.Count; i++) 
        {
            var cfg = allAchievement[i];
            _RegisterAchievementConditionWatcher(cfg);
        }
    }

    private void _RegisterAchievementConditionWatcher(AchievementItemConfig cfg)
    {
        var con = cfg.Conditions;
        for(int i = 0; i < con.Count; i++)
        {
            var key = con[i].AchievementKey;
            var content = GetOrCreateStatistics_Content(key);
            content.AddWatcher(cfg);
        }
    }

    /// <summary>
    /// 成就条件判断
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private bool AchievementConditionResult(AchievementConditionConfig cfg)
    {
        var content = GetOrCreateStatistics_Content(cfg.AchievementKey);
        if (cfg.CheckBoolValue)
        {
            return content.ValueBool;
        }

        var value = content.ValueInt.Value;
        return MathExtensionTools.CalculateCompareType(value, cfg.Value, cfg.Compare);
    }

    #region Statistics

    /***敌人总击杀***/
    public const string StatisticsKey_ENEMY_TOTAL_KILL = "ENEMY_TOTAL_KILL";

    public void ChangeStatistics_Int(string key, int value)
    {
        var content = GetOrCreateStatistics_Content(key);
        if(content.StatisticsType == GameStatisticsType.Int_Max)
        {
            content.SetValueMax(value);
        }
        else if (content.StatisticsType == GameStatisticsType.Int_Sum)
        {
            content.AddValue(value);
        }
    }

    private GameStatisticsData GetOrCreateStatistics_Content(string key)
    {
        if (_game_statistics_data.ContainsKey(key))
            return _game_statistics_data[key];

        var content = new GameStatisticsData(GetGameStasticsDataType(key));
        _game_statistics_data.Add(key, content);
        return content;
    }

    private GameStatisticsType GetGameStasticsDataType(string key)
    {
        if (string.Compare(StatisticsKey_ENEMY_TOTAL_KILL, key) == 0) 
        {
            return GameStatisticsType.Int_Sum;
        }

        return GameStatisticsType.Int_Sum;
    }

    #endregion

    #region Watcher

    private void Watcher_OnEnmeyDie(BaseShip ship)
    {
        ChangeStatistics_Int(StatisticsKey_ENEMY_TOTAL_KILL, 1);
    }

    #endregion
}

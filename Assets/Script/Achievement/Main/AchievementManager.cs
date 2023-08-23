using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AchievementEventType
{
    UI_GroupSelected
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


public class AchievementManager : Singleton<AchievementManager>
{

    private Dictionary<AchievementWatcherType, IAchievementWatcher> _watcherDic;

    private Dictionary<string, ChangeValue<int>> _game_statistics_int;


    public AchievementManager()
    {
        _watcherDic = new Dictionary<AchievementWatcherType, IAchievementWatcher>();
    }


    public override void Initialization()
    {
        base.Initialization();
        _game_statistics_int = new Dictionary<string, ChangeValue<int>>();
        BindWatcherListener();
    }

    /// <summary>
    /// ´¥·¢Watcher
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

    private void BindWatcherListener()
    {
        AddWatcherListener<BaseShip>(AchievementWatcherType.EnemyKill, Watcher_OnEnmeyDie);
    }

    #region Statistics

    /***µÐÈË×Ü»÷É±***/
    public const string StatisticsKey_ENEMY_TOTAL_KILL = "ENEMY_TOTAL_KILL";

    public void AddStatistics_Int(string key, int value)
    {
        var content = GetOrCreateStatistics_IntContent(key);
        int newValue = content.Value + value;
        content.Set(newValue);
    }

    private ChangeValue<int> GetOrCreateStatistics_IntContent(string key)
    {
        if (_game_statistics_int.ContainsKey(key))
            return _game_statistics_int[key];

        var content = new ChangeValue<int>(0, 0, int.MaxValue);
        _game_statistics_int.Add(key, content);
        return content;
    }

    #endregion

    #region Watcher

    private void Watcher_OnEnmeyDie(BaseShip ship)
    {
        AddStatistics_Int(StatisticsKey_ENEMY_TOTAL_KILL, 1);
    }

    #endregion
}

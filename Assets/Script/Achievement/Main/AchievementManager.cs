using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public enum AchievementEventType
{
    UI_GroupSelected,
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

[System.Serializable]
public class GameStatisticsSaveData
{
    public string KeyName;
    public GameStatisticsType StatisticsType;
    public bool ValueBool;
    public int ValueInt;
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

    public GameStatisticsSaveData CreateSaveData()
    {
        GameStatisticsSaveData sav = new GameStatisticsSaveData();
        sav.StatisticsType = StatisticsType;
        sav.ValueInt = ValueInt.Value;
        sav.ValueBool = ValueBool;
        return sav;
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

    /// <summary>
    /// 局内Unit统计
    /// </summary>
    private Dictionary<uint, UnitStatisticsData> _runtimeUnitStatisticsData;

    private Queue<AchievementItemConfig> _achievementUIQueue;
    private bool _isShowingAchievement = false;
    private bool _hasInit = false;

    public AchievementManager()
    {
        _achievementUIQueue = new Queue<AchievementItemConfig>();
        _watcherDic = new Dictionary<AchievementWatcherType, IAchievementWatcher>();
        _game_statistics_data = new Dictionary<string, GameStatisticsData>();
        _runtimeUnitStatisticsData = new Dictionary<uint, UnitStatisticsData>();
    }

    public async void OnUpdate()
    {
        if (_achievementUIQueue.Count <= 0)
            return;

        if(_achievementUIQueue.Count > 0 && !_isShowingAchievement)
        {
            _isShowingAchievement = true;
            var achievement = _achievementUIQueue.Dequeue();
            AchievementUI m_ui = null;
            UIManager.Instance.CreatePoolerUI<AchievementUI>("AchievementUI", true, E_UI_Layer.Top, null, (panel) =>
            {
                m_ui = panel;
                panel.SetUp(achievement);
            });
            await UniTask.WaitUntil(() => m_ui != null);
            await UniTask.WaitUntil(() => m_ui.IsFinish);
            _isShowingAchievement = false;
        }
    }

    public override void Initialization()
    {
        base.Initialization();
        if (_hasInit)
            return;

        LoadSaveData();
        BindWatcherListener();
        _hasInit = true;
    }

    public void ClearRuntimeData()
    {
        _runtimeUnitStatisticsData.Clear();
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
            _achievementUIQueue.Enqueue(cfg);

            sav.Unlock = true;
            sav.FinishTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }

    private void BindWatcherListener()
    {
        AddWatcherListener<BaseShip>(AchievementWatcherType.EnemyKill, Watcher_OnEnmeyDie);
        AddWatcherListener<BaseUnitConfig>(AchievementWatcherType.WreckageSell, Watcher_HarborUnitSell);
        AddWatcherListener<int>(AchievementWatcherType.CurrencyChange, Watcher_CurrencyChange);
        AddWatcherListener<GoodsItemRarity>(AchievementWatcherType.WreckageGain, Watcher_WreckageGain);
        AddWatcherListener<DamageResultInfo>(AchievementWatcherType.UnitHit, Watcher_UnitHit);
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
    /***出售总数***/
    public const string StatisticsKey_HARBOR_SELLCOUNT_TOTAL = "HARBOR_SELLCOUNT_TOTAL";
    /***总金币***/
    public const string StatisticsKey_CURRENCY_GAIN_TOTAL = "CURRENCY_GAIN_TOTAL";
    /***单局最大金币***/
    public const string StatisticsKey_CURRENCY_MAX_PERBATTLE = "CURRENCY_MAX_PERBATTLE";
    /***累计残骸***/
    public const string StatisticsKey_WRECKAGE_GAIN_TOTAL = "WRECKAGE_GAIN_TOTAL";

    public int GetStatisticsValue(string key)
    {
        var content = GetOrCreateStatistics_Content(key);
        if(content.StatisticsType == GameStatisticsType.Bool)
        {
            return content.ValueBool ? 1 : 0;
        }

        return content.ValueInt.Value;
    }

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

    /// <summary>
    /// 局内Unit统计数据
    /// </summary>
    /// <param name="UID"></param>
    /// <returns></returns>
    public UnitStatisticsData GetOrCreateRuntimeUnitStatisticsData(uint UID)
    {
        if (_runtimeUnitStatisticsData.ContainsKey(UID))
        {
            return _runtimeUnitStatisticsData[UID];
        }
        else
        {
            var newData = new UnitStatisticsData(UID);
            _runtimeUnitStatisticsData.Add(UID, newData);
            return newData;
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
        switch (key)
        {
            case StatisticsKey_CURRENCY_MAX_PERBATTLE:
                return GameStatisticsType.Int_Max;
        }

        return GameStatisticsType.Int_Sum;
    }

    #endregion

    #region Watcher

    /// <summary>
    /// 单位死亡
    /// </summary>
    /// <param name="ship"></param>
    private void Watcher_OnEnmeyDie(BaseShip ship)
    {
        ChangeStatistics_Int(StatisticsKey_ENEMY_TOTAL_KILL, 1);
    }

    /// <summary>
    /// 出售单位
    /// </summary>
    /// <param name="unitCfg"></param>
    private void Watcher_HarborUnitSell(BaseUnitConfig unitCfg)
    {
        ChangeStatistics_Int(StatisticsKey_HARBOR_SELLCOUNT_TOTAL, 1);
    }

    private void Watcher_CurrencyChange(int value)
    {
        ChangeStatistics_Int(StatisticsKey_CURRENCY_GAIN_TOTAL, 1);
        var currentCurrency = RogueManager.Instance.CurrentCurrency;
        ChangeStatistics_Int(StatisticsKey_CURRENCY_MAX_PERBATTLE, currentCurrency);
    }

    private void Watcher_WreckageGain(GoodsItemRarity rarity)
    {
        ChangeStatistics_Int(StatisticsKey_WRECKAGE_GAIN_TOTAL, 1);
    }

    private void Watcher_UnitHit(DamageResultInfo info)
    {
        var currentWaveIndex = RogueManager.Instance.GetCurrentWaveIndex;
        var attackerUnit = info.attackerUnit;
        if(attackerUnit != null)
        {
            var targetData = GetOrCreateRuntimeUnitStatisticsData(attackerUnit.UID);
            targetData.AddDamageTotal(currentWaveIndex, info.Damage);
        }

        var targetUnit = info.Target;
        if(targetUnit != null && targetUnit is Unit)
        {
            var unit = targetUnit as Unit;
            var targetData = GetOrCreateRuntimeUnitStatisticsData(unit.UID);
            targetData.AddDamageTakeTotal(currentWaveIndex, info.Damage);
        }

    }

    #endregion

    #region Save

    public void LoadSaveData()
    {
        var allStatisticsData = SaveLoadManager.Instance.globalSaveData.StatisticsSaveData;
        if (allStatisticsData != null && allStatisticsData.Count > 0) 
        {
            for (int i = 0; i < allStatisticsData.Count; i++) 
            {
                    var sav = allStatisticsData[i];
                if (_game_statistics_data.ContainsKey(sav.KeyName))
                    continue;

                GameStatisticsData data = new GameStatisticsData(sav.StatisticsType);
                data.ValueBool = sav.ValueBool;
                data.ValueInt.Set(sav.ValueInt);
                _game_statistics_data.Add(sav.KeyName, data);
            }
        }
    }

    public List<GameStatisticsSaveData> GenerateGameStatisticsSaveData()
    {
        List<GameStatisticsSaveData> result = new List<GameStatisticsSaveData>();
        foreach (var data in _game_statistics_data)
        {
            GameStatisticsSaveData sav = data.Value.CreateSaveData();
            sav.KeyName = data.Key;
            result.Add(sav);
        }
        return result;
    }

    #endregion
}

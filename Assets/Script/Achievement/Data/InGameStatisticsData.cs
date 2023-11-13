using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameStatisticsData 
{
    public int TotalGainWasteCount;
    public int TotalGainCurrency;
    public int TotalCostCurrency;

    public Dictionary<GoodsItemRarity, int> WasteGainInfo = new Dictionary<GoodsItemRarity, int>
    {
        { GoodsItemRarity.Tier1, 0 } ,
        { GoodsItemRarity.Tier2, 0 } ,
        { GoodsItemRarity.Tier3, 0 } ,
        { GoodsItemRarity.Tier4, 0 } ,
    };

    public List<PlugGainInfo> PlugInfo = new List<PlugGainInfo>();

    public Dictionary<int, WaveStatisticsInfo> WaveInfoDic = new Dictionary<int, WaveStatisticsInfo>();

    public List<WaveDetailLogInfo> DetailLogInfos = new List<WaveDetailLogInfo>();

    public void Clear()
    {
        WaveInfoDic.Clear();
        PlugInfo.Clear();
        TotalGainWasteCount = 0;
        TotalGainCurrency = 0;
        TotalCostCurrency = 0;
        WasteGainInfo[GoodsItemRarity.Tier1] = 0;
        WasteGainInfo[GoodsItemRarity.Tier2] = 0;
        WasteGainInfo[GoodsItemRarity.Tier3] = 0;
        WasteGainInfo[GoodsItemRarity.Tier4] = 0;
    }

    public WaveStatisticsInfo GetOrCreateWaveInfo(int waveIndex)
    {
        if (WaveInfoDic.ContainsKey(waveIndex))
            return WaveInfoDic[waveIndex];

        WaveStatisticsInfo info = new WaveStatisticsInfo();
        info.WaveIndex = waveIndex;
        WaveInfoDic.Add(waveIndex, info);
        return info;
    }

    public List<WaveSecondsLogData> GenerateWaveSecondsLog()
    {
        var logLst = new List<WaveSecondsLogData>();
        for (int i = 0; i < DetailLogInfos.Count; i++)
        {
            var info = DetailLogInfos[i];
            WaveSecondsLogData log = new WaveSecondsLogData();
            log.WaveIndex = info.WaveIndex;
            log.Seconds = info.TimeSeconds;
            log.Frame = info.Frame;
            log.EnemyCount = info.EnemyCount;
            log.EnemyDroneCount = info.EnemyDroneCount;
            log.EnemyTotalThread = info.EnemyTotalThread;
            logLst.Add(log);
        }
        return logLst;
    }
}

public class PlugGainInfo
{
    public int PlugID;
    public int Wave;
    public int GainCurrencyCost;
}

public class WaveDetailLogInfo
{
    public int WaveIndex;
    public int TimeSeconds;
    public int Frame;
    public int EnemyCount;
    public int EnemyDroneCount;
    public float EnemyTotalThread;
}

/// <summary>
/// 波次统计数据
/// </summary>
public class WaveStatisticsInfo
{
    public int WaveIndex;
    public byte MeteoriteGenerateCount;
    public byte MeteoriteKillCount;
    public string Meteorite_Time;

    public byte WreckageGenerateCount;
    public string Wreckage_Time;
    public byte WreckageGainCount;
    public string WreckageGainRarityInfo;

    public int RefreshEnemyCount;
    public int KillEnemyCount;

    public string RefreshBossIDs;
    public string RefreshEliteIDs;

    public byte ShopRefreshCount;
    public byte ShopEnterCount;
    public string ShopItemRefreshCounts;
    public string ShopCurrencyCostMaps;
    public string ShopWasteSellMaps;
    public string ShopBuyInfo;

    public WaveInfoLogData GenerateLog()
    {
        WaveInfoLogData data = new WaveInfoLogData();
        data.WaveIndex = WaveIndex;
        data.MeteoriteGenerateCount = MeteoriteGenerateCount;
        data.MeteoriteKillCount = MeteoriteKillCount;
        data.Meteorite_Time = Meteorite_Time;
        data.WreckageGenerateCount = WreckageGenerateCount;
        data.Wreckage_Time = Wreckage_Time;
        data.WreckageGainCount = WreckageGainCount;
        data.WreckageGainRarityInfo = WreckageGainRarityInfo;
        data.RefreshEnemyCount = RefreshEnemyCount;
        data.KillEnemyCount = KillEnemyCount;
        data.RefreshBossIDs = RefreshBossIDs;
        data.RefreshEliteIDs = RefreshEliteIDs;
        data.ShopRefreshCount = ShopRefreshCount;
        data.ShopEnterCount = ShopEnterCount;
        data.ShopItemRefreshCounts = ShopItemRefreshCounts;
        data.ShopCurrencyCostMaps = ShopCurrencyCostMaps;
        data.ShopWasteSellMaps = ShopWasteSellMaps;
        data.ShopBuyInfo = ShopBuyInfo;
        return data;
    }
}
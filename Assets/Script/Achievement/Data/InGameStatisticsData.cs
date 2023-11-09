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

    public void Clear()
    {
        PlugInfo.Clear();
        TotalGainWasteCount = 0;
        TotalGainCurrency = 0;
        TotalCostCurrency = 0;
        WasteGainInfo[GoodsItemRarity.Tier1] = 0;
        WasteGainInfo[GoodsItemRarity.Tier2] = 0;
        WasteGainInfo[GoodsItemRarity.Tier3] = 0;
        WasteGainInfo[GoodsItemRarity.Tier4] = 0;
    }
}

public class PlugGainInfo
{
    public int PlugID;
    public int Wave;
    public int GainCurrencyCost;
}

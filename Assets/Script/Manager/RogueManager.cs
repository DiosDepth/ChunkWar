using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class RogueManager : Singleton<RogueManager>
{
    /// <summary>
    /// 所有商店物品
    /// </summary>
    private Dictionary<int, ShopGoodsInfo> goodsItems;

    private Dictionary<int, byte> _playerCurrentGoods;

    private int _currentRefreshCount = 0;

    public RogueManager()
    {
        Initialization();
    }

    public void InitRogueBattle()
    {
        InitAllGoodsItems();
        GenerateShopGoods(5, 10);
    }

    public override void Initialization()
    {
        base.Initialization();
        goodsItems = new Dictionary<int, ShopGoodsInfo>();
        _playerCurrentGoods = new Dictionary<int, byte>();
    }

    public void GainShopItem(ShopGoodsInfo info)
    {

    }

    public void RefreshShop()
    {

    }

    /// <summary>
    /// 获取当前拥有的物件数量
    /// </summary>
    /// <param name="goodsID"></param>
    /// <returns></returns>
    public byte GetCurrentGoodsCount(int goodsID)
    {
        if (_playerCurrentGoods.ContainsKey(goodsID))
            return _playerCurrentGoods[goodsID];

        return 0;
    }

    /// <summary>
    /// 生成商店物品
    /// </summary>
    /// <param name="count"></param>
    /// <param name="waveIndex"></param>
    /// <returns></returns>
    public List<ShopGoodsInfo> GenerateShopGoods(byte count, int waveIndex)
    {
        List<ShopGoodsInfo> result = new List<ShopGoodsInfo>();
        var allVaild = goodsItems.Values.ToList().FindAll(x => x.IsVaild);

        var tier2Rate = GetWeightByRarityAndWave(waveIndex, GoodsItemRarity.Tier2);
        var tier3Rate = GetWeightByRarityAndWave(waveIndex, GoodsItemRarity.Tier3);
        var tier4Rate = GetWeightByRarityAndWave(waveIndex, GoodsItemRarity.Tier4);
        var tier1Rate = 100 - tier2Rate - tier3Rate - tier4Rate;
        
        List<ShopGoodsRarityItem> rarityItems = new List<ShopGoodsRarityItem>();
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier2, tier2Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier3, tier3Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier4, tier4Rate));
        rarityItems.Add(new ShopGoodsRarityItem(GoodsItemRarity.Tier1, tier1Rate));

        for (int i = 0; i < count; i++) 
        {
            var rarityResult = Utility.GetRandomList<ShopGoodsRarityItem>(rarityItems, 1);
            if(rarityResult.Count <= 0)
            {
                Debug.LogError("Shop Item RandomError! WaveIndex = " + waveIndex);
                continue;
            }

            var rarity = rarityResult[0];
            var allVaildRarityItems = allVaild.FindAll(x => x.Rarity == rarity.Rarity);

            var goodsReusult = Utility.GetRandomList<ShopGoodsInfo>(allVaildRarityItems, 1);
            if (goodsReusult.Count > 0)
            {
                var goods = goodsReusult[0];
                result.Add(goods);
                ///如果数量到上限或者unique,从列表中去除
                if (goods._cfg.Unique)
                {
                    allVaild.Remove(goods);
                }
                else if (goods._cfg.MaxBuyCount > 0)
                {
                    var currentCount = GetCurrentGoodsCount(goods.GoodsID);
                    var currentRandomPoolCount = result.FindAll(x => x.GoodsID == goods.GoodsID).Count;
                    if(currentCount + currentRandomPoolCount + 1 >= goods._cfg.MaxBuyCount)
                    {
                        allVaild.Remove(goods);
                    }
                }
            }
        }

#if UNITY_EDITOR
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Count; i++) 
        {
            sb.Append(result[i].GetLog());
        }
        Debug.Log(sb.ToString());
#endif

        return result;
    }

    /// <summary>
    /// 根据波次和稀有度获取随机权重
    /// </summary>
    /// <param name="waveIndex"></param>
    /// <param name="rarity"></param>
    /// <returns></returns>
    private float GetWeightByRarityAndWave(int waveIndex, GoodsItemRarity rarity)
    {
        var rarityMap = DataManager.Instance.shopCfg.RarityMap;
        if (!rarityMap.ContainsKey(rarity))
            return 0;

        ///TODO
        var playerLuck = 0;

        var rarityCfg = rarityMap[rarity];
        if (waveIndex < rarityCfg.MinAppearWave)
            return 0;

        var waveIndexDelta = waveIndex - rarityCfg.LuckModifyMinWave;
        return rarityCfg.WeightAddPerWave * waveIndexDelta + rarityCfg.BaseWeight * (100 + playerLuck);
    }

    private void InitAllGoodsItems()
    {
        var allGoods = DataManager.Instance.shopCfg.Goods;
        for(int i = 0; i < allGoods.Count; i++)
        {
            var goodsCfg = allGoods[i];
            if (!goodsItems.ContainsKey(goodsCfg.GoodID))
            {
                var goodsInfo = ShopGoodsInfo.CreateGoods(goodsCfg.GoodID);
                goodsItems.Add(goodsInfo.GoodsID, goodsInfo);
            }
        }
    }
   
}

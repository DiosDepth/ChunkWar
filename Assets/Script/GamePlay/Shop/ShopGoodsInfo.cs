using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ShopGoodsRarityItem : RandomObject
{
    public GoodsItemRarity Rarity;
    public int Weight { get; set; }

    public ShopGoodsRarityItem(GoodsItemRarity rarity, float weight)
    {
        this.Rarity = rarity;
        this.Weight = Mathf.CeilToInt(weight * 100);
    }
}

public class ShopGoodsInfo : RandomObject
{
    public int GoodsID;

    public ShopGoodsItemConfig _cfg;
    
    /// <summary>
    /// 出现权重
    /// </summary>
    public int Weight
    {
        get;
        set;
    }

    public string ItemName
    {
        get { return LocalizationManager.Instance.GetTextValue(_cfg.Name); }
    }

    public string ItemDesc
    {
        get { return LocalizationManager.Instance.GetTextValue(_cfg.Desc); }
    }

    public GoodsItemRarity Rarity
    {
        get { return _cfg.Rarity; }
    }

    /// <summary>
    /// 是否可以购买
    /// </summary>
    /// <returns></returns>
    public bool CheckCanBuy()
    {
        return CostEnough && !_sold;
    }

    public bool CostEnough
    {
        get
        {
            var currency = RogueManager.Instance.CurrentCurrency;
            return Cost <= currency;
        }
    }

    public int Cost
    {
        get { return GetCost(); }
    }

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsVaild
    {
        get
        {
            return CheckIsVaild();
        }
    }

    /// <summary>
    /// Temp Slod
    /// </summary>
    private bool _sold = false;

    public static ShopGoodsInfo CreateGoods(int goodsID)
    {
        var cfg = DataManager.Instance.GetShopGoodsCfg(goodsID);
        if (cfg == null)
            return null;

        ShopGoodsInfo info = new ShopGoodsInfo();
        info.GoodsID = goodsID;
        info._cfg = cfg;

        return info;
    }

    public string GetLog()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("=====Generate Shop Item====== \n");
        sb.Append(string.Format("ID = {0} \n", GoodsID));
        sb.Append(string.Format("Rarity = {0} \n", Rarity));
        return sb.ToString();
    }

    public void OnItemAddToShop()
    {
        _sold = false;
    }

    public void OnItemSold()
    {
        _sold = true;
    }

    private bool CheckIsVaild()
    {
        if (_cfg.MaxBuyCount == -1)
            return true;

        var currentCount = RogueManager.Instance.GetCurrentGoodsCount(GoodsID);
        if (_cfg.Unique && currentCount == 1)
            return false;

        if (_cfg.MaxBuyCount > 1 && currentCount >= _cfg.MaxBuyCount)
            return false;

        return true;
    }

    private int GetCost()
    {
        ///PriceModify
        var shopPriceFinal = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShopCostPercent);
        shopPriceFinal = Mathf.Clamp(shopPriceFinal, -100f, float.MaxValue);

        var basePrice = _cfg.CostBase;
        var currentWave = RogueManager.Instance.GetCurrentWaveIndex;
        var price = (basePrice + (currentWave * basePrice * 0.1f)) * (1 + shopPriceFinal / 100f);
        price = Mathf.Clamp(price, 1f, price);
        return Mathf.RoundToInt(price);
    }
}

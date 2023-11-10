using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public struct GeneralRarityRandomItem : RandomObject
{
    public GoodsItemRarity Rarity;
    public int Weight { get; set; }

    public GeneralRarityRandomItem(GoodsItemRarity rarity, float weight)
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

    /// <summary>
    /// 是否残骸包
    /// </summary>
    public bool IsWreckage
    {
        get;
        private set;
    }

    /// <summary>
    /// 打折比例
    /// </summary>
    public byte DiscountValue
    {
        get;
        protected set;
    }

    /// <summary>
    /// 购买限制
    /// </summary>
    public int GetBuyLimit
    {
        get
        {
            return _cfg.MaxBuyCount;
        }
    }

    public GoodsItemRarity Rarity
    {
        get;
        private set;
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

    /// <summary>
    /// 是否锁定
    /// </summary>
    public bool IsLock
    {
        get;
        set;
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

    public string Name
    {
        get
        {
            if (IsWreckage)
            {
                var wreckageCfg = DataManager.Instance.shopCfg.WreckageShopBuyItemCfg.Find(x => x.Rarity == Rarity);
                if(wreckageCfg != null)
                {
                    return LocalizationManager.Instance.GetTextValue(wreckageCfg.ItemConfig.Name);
                }
            }
            else
            {
                var itemCfg = DataManager.Instance.GetShipPlugItemConfig(_cfg.TypeID);
                if (itemCfg != null)
                    return LocalizationManager.Instance.GetTextValue(itemCfg.GeneralConfig.Name);
            }

            return string.Empty;
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
        info.Weight = cfg.Weight;
        info.DiscountValue = 0;
        info.IsWreckage = false;

        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(cfg.TypeID);
        if (plugCfg != null)
        {
            info.Rarity = plugCfg.GeneralConfig.Rarity;
        }

        return info;
    }

    public static ShopGoodsInfo CreateWreckageGoods(WreckageShopBuyItemConfig cfg)
    {
        ShopGoodsInfo info = new ShopGoodsInfo();
        info.Weight = cfg.WeightBase;
        info.DiscountValue = 0;
        info.GoodsID = cfg.GetGoodsID();
        info._cfg = new ShopGoodsItemConfig()
        {
            CostBase = cfg.CostBase,
            GoodID = cfg.GetGoodsID(),
            MaxBuyCount = -1,
            TypeID = 0,
            Weight = cfg.WeightBase
        };
        info.IsWreckage = true;
        info.Rarity = cfg.Rarity;
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

    /// <summary>
    /// 获取描述
    /// </summary>
    /// <returns></returns>
    public string GetEffectDesc()
    {
        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(_cfg.TypeID);
        if (plugCfg == null)
            return string.Empty;

        return plugCfg.GetEffectDesc();
    }

    public void OnItemAddToShop()
    {
        _sold = false;
    }

    public void OnItemSold()
    {
        _sold = true;
        IsLock = false;
    }

    public void SetDiscountValue(byte value)
    {
        DiscountValue = value;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        DiscountValue = 0;
    }

    private bool CheckIsVaild()
    {
        if (_cfg.MaxBuyCount == -1)
            return true;

        var currentCount = RogueManager.Instance.GetCurrentPlugCount(GoodsID);

        if (_cfg.MaxBuyCount > 0 && currentCount >= _cfg.MaxBuyCount)
            return false;

        return true;
    }

    private int GetCost()
    {
        ///PriceModify
        var shopPriceFinal = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShopCostPercent);
        shopPriceFinal = Mathf.Clamp(shopPriceFinal, -100f, float.MaxValue);

        var basePrice = _cfg.CostBase;
        var currentShopEnterCount = RogueManager.Instance.GetShopEnterTotalCount;
        var price = (basePrice + currentShopEnterCount + (currentShopEnterCount * basePrice * 0.1f)) * (1 + shopPriceFinal / 100f);
        var disCountPercent = (100 - DiscountValue) / 100f;
        price *= disCountPercent;
        price = Mathf.Clamp(price, 1f, price);
        return Mathf.RoundToInt(price);
    }
}

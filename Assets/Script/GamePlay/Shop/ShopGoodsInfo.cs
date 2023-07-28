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
    /// 是否有效
    /// </summary>
    public bool IsVaild
    {
        get
        {
            return CheckIsVaild();
        }
    }

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
}

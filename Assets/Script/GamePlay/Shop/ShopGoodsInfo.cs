using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopGoodsInfo : RandomObject
{
    private ShopGoodsItemConfig _cfg;
    
    /// <summary>
    /// 出现权重
    /// </summary>
    public int Weight
    {
        get;
        set;
    }

    public static ShopGoodsInfo CreateGoods(int goodsID)
    {
        var cfg = DataManager.Instance.GetShopGoodsCfg(goodsID);
        if (cfg == null)
            return null;

        ShopGoodsInfo info = new ShopGoodsInfo();
        info._cfg = cfg;

        return info;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Ω¢¥¨ Ù–‘…˝º∂
 */
public class ShipLevelUpItem : RandomObject
{
    public int Weight
    {
        get;
        set;
    }

    public GoodsItemRarity Rarity;

    public ShipLevelUpGrowthItemConfig Config;

    public ShipLevelUpItem(ShipLevelUpGrowthItemConfig cfg, GoodsItemRarity rarity)
    {
        this.Rarity = rarity;
        this.Config = cfg;
    }
}

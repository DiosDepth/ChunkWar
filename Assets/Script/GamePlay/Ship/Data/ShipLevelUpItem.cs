using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ������������
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

    public float GetModifyValue()
    {
        return Config.RarityValueMap[(int)Rarity];
    }

    public ShipLevelUpItem(ShipLevelUpGrowthItemConfig cfg, GoodsItemRarity rarity)
    {
        this.Rarity = rarity;
        this.Config = cfg;
    }
}

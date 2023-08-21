using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckageItemInfo : RandomObject
{
    public int UnitID;

    public uint UID;

    public WreckageDropItemConfig _cfg;

    public int Weight
    {
        get;
        set;
    }

    /// <summary>
    /// 出售价格
    /// </summary>
    public int SellPrice
    {
        get;
        private set;
    }

    public int LoadCost
    {
        get;
        private set;
    }

    public string Name
    {
        get { return LocalizationManager.Instance.GetTextValue(UnitConfig.GeneralConfig.Name); }
    }

    public string Desc
    {
        get { return LocalizationManager.Instance.GetTextValue(UnitConfig.GeneralConfig.Desc); }
    }

    public string TypeName
    {
        get
        {
            if (UnitConfig.unitType == UnitType.Weapons)
            {
                return LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipWeapon_Text);
            }
            else if (UnitConfig.unitType == UnitType.Buildings)
            {
                return LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipBuilding_Text);
            }
            return string.Empty;
        }
    }

    public Color RarityColor
    {
        get
        {
            return GameHelper.GetRarityColor(UnitConfig.GeneralConfig.Rarity);
        }
    }

    public GoodsItemRarity Rarity;

    public BaseUnitConfig UnitConfig;

    public WreckageItemInfo Clone()
    {
        WreckageItemInfo info = (WreckageItemInfo)this.MemberwiseClone();
        return info;
    }

    public static WreckageItemInfo CreateInfo(WreckageDropItemConfig cfg)
    {
        WreckageItemInfo info = new WreckageItemInfo();
        info.UnitID = cfg.UnitID;
        var unitCfg = DataManager.Instance.GetUnitConfig(cfg.UnitID);
        if(unitCfg != null)
        {
            info.Rarity = unitCfg.GeneralConfig.Rarity;
        }

        info.Weight = cfg.Weight;
        info._cfg = cfg;
        info.Init();
        return info;
    }

    public void OnRemove()
    {
        RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.SellPrice, CalculateSellPrice);
        RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.UnitLoadCost, CalculateLoadCost);
    }

    public string GetLog()
    {
        var unitCfg = DataManager.Instance.GetUnitConfig(UnitID);
        return string.Format("ID = {0}, Name = {1} \n", UnitID, LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name));
    }


    private void Init()
    {
        UnitConfig = DataManager.Instance.GetUnitConfig(_cfg.UnitID);
        CalculateSellPrice();
        CalculateLoadCost();
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.SellPrice, CalculateSellPrice);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.UnitLoadCost, CalculateLoadCost);
    }

    private void CalculateSellPrice()
    {
        var sellPriceAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.SellPrice);
        var price = Mathf.Clamp(_cfg.SellPrice * (1 + sellPriceAdd / 100f), 0, float.MaxValue);
        SellPrice = Mathf.CeilToInt(price);
    }

    private void CalculateLoadCost()
    {
        var loadCost = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.UnitLoadCost);
        var load = Mathf.Clamp(_cfg.SellPrice * (1 + loadCost / 100f), 0, float.MaxValue);
        LoadCost = Mathf.CeilToInt(load);
    }

}

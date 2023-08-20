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

    public GoodsItemRarity Rarity;

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
        return info;
    }

    public string GetLog()
    {
        var unitCfg = DataManager.Instance.GetUnitConfig(UnitID);
        return string.Format("ID = {0}, Name = {1} \n", UnitID, LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name));
    }

}

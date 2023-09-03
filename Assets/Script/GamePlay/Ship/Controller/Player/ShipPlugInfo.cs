using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 舰船部件
 */

public class ShipPlugInfo 
{
    public int PlugID;
    public uint PlugUID;

    /// <summary>
    /// 商品ID
    /// </summary>
    public int GoodsID;

    private ShipPlugItemConfig _cfg;

    private List<PropertyModifySpecialData> _modifySpecialDatas = new List<PropertyModifySpecialData>();

    public static ShipPlugInfo CreateInfo(int plugID, int goodsID)
    {
        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(plugID);
        if (plugCfg == null)
            return null;

        ShipPlugInfo info = new ShipPlugInfo();
        info.PlugID = plugID;
        info._cfg = plugCfg;
        info.GoodsID = goodsID;

        return info;
    }

    /// <summary>
    /// 修正属性
    /// </summary>
    public void OnAdded()
    {
        var propertyModify = _cfg.PropertyModify;
        if(propertyModify != null && propertyModify.Count > 0)
        {
            for(int i = 0; i < propertyModify.Count; i++)
            {
                var modify = propertyModify[i];
                if (!modify.BySpecialValue)
                {
                    RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(modify.ModifyKey, PlugUID, modify.Value);
                }
                else
                {
                    PropertyModifySpecialData specialData = new PropertyModifySpecialData(modify, PlugUID);
                    _modifySpecialDatas.Add(specialData);
                }
            }
        }

        var propertyPercentModify = _cfg.PropertyPercentModify;
        if(propertyPercentModify != null && propertyPercentModify.Count > 0)
        {
            for (int i = 0; i < propertyPercentModify.Count; i++)
            {
                var modify = propertyPercentModify[i];
                RogueManager.Instance.MainPropertyData.AddPropertyModifyPercentValue(modify.ModifyKey, PlugUID, modify.Value);
            }
        }

        ///Handle SpecialDatas
        for (int i = 0; i < _modifySpecialDatas.Count; i++)
        {
            _modifySpecialDatas[i].HandlePropetyModifyBySpecialValue();
        }
    }


}
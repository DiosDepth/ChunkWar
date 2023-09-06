using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ��������
 */

public class ShipPlugInfo : IPropertyModify
{
    public int PlugID;
    public uint UID { get; set; }

    public PropertyModifyCategory Category { get { return PropertyModifyCategory.ShipPlug; } }

    /// <summary>
    /// ��ƷID
    /// </summary>
    public int GoodsID;

    private ShipPlugItemConfig _cfg;

    private List<PropertyModifySpecialData> _modifySpecialDatas = new List<PropertyModifySpecialData>();
    private List<ModifyTriggerData> _triggerDatas = new List<ModifyTriggerData>();
    public List<ModifyTriggerData> AllTriggerDatas
    {
        get { return _triggerDatas; }
    }

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

    public void OnBattleUpdate()
    {
        for (int i = 0; i < _triggerDatas.Count; i++) 
        {
            _triggerDatas[i].OnUpdateBattle();
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void OnAdded()
    {
        var propertyModify = _cfg.PropertyModify;
        if(propertyModify != null && propertyModify.Length > 0)
        {
            for(int i = 0; i < propertyModify.Length; i++)
            {
                var modify = propertyModify[i];
                if (!modify.BySpecialValue)
                {
                    RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(modify.ModifyKey, PropertyModifyType.Modify, UID, modify.Value);
                }
                else
                {
                    PropertyModifySpecialData specialData = new PropertyModifySpecialData(modify, UID);
                    _modifySpecialDatas.Add(specialData);
                }
            }
        }

        var propertyPercentModify = _cfg.PropertyPercentModify;
        if(propertyPercentModify != null && propertyPercentModify.Length > 0)
        {
            for (int i = 0; i < propertyPercentModify.Length; i++)
            {
                var modify = propertyPercentModify[i];
                RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(modify.ModifyKey, PropertyModifyType.ModifyPercent, UID, modify.Value);
            }
        }

        ///Handle SpecialDatas
        for (int i = 0; i < _modifySpecialDatas.Count; i++)
        {
            _modifySpecialDatas[i].HandlePropetyModifyBySpecialValue();
        }

        InitModifyTrigger();
    }

    private void InitModifyTrigger()
    {
        var triggers = _cfg.ModifyTriggers;
        if (triggers != null && triggers.Length > 0) 
        {
            for (int i = 0; i < triggers.Length; i++) 
            {
                var triggerData = ModifyTriggerData.CreateTrigger(triggers[i], UID);
                if(triggerData != null)
                {
                    var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ModifyTrigger, triggerData);
                    triggerData.UID = uid;
                    triggerData.OnTriggerAdd();
                    _triggerDatas.Add(triggerData);
                }
            }
        }
    }
}
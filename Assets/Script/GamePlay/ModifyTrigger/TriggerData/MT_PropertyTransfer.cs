using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 属性转化
/// </summary>
public class MT_PropertyTransfer : ModifyTriggerData
{
    private MTC_PropertyTransfer _propertyCfg;

    private UnitPropertyData _propertyData;

    public MT_PropertyTransfer(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _propertyCfg = cfg as MTC_PropertyTransfer;
        _propertyData = RogueManager.Instance.MainPropertyData;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        _propertyData.BindPropertyChangeAction(_propertyCfg.BaseProperty, OnProeprtyChange);
    }

    /// <summary>
    /// 根据比例进行属性转化
    /// </summary>
    private void OnProeprtyChange()
    {
        var baseValue = _propertyData.GetPropertyFinalWithoutTransfer(_propertyCfg.BaseProperty);
        var rate = Mathf.RoundToInt(baseValue / _propertyCfg.BaseValuePer);
        var targetValue = rate * _propertyCfg.TargetValuePer;
        _propertyData.SetPropertyModifyValue(_propertyCfg.TargetProperty, PropertyModifyType.PropertyTransfer, UID, targetValue);
    }
}


/// <summary>
/// 物品数量转化
/// </summary>
public class MT_ItemTransfer : ModifyTriggerData
{
    private MTC_ItemTransfer _transferCfg;

    public MT_ItemTransfer(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _transferCfg = cfg as MTC_ItemTransfer;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        if(_transferCfg.ItemID == -1)
        {
            ///残骸转化
            RogueManager.Instance.OnWasteCountChange += OnWasteItemChange;
        }
        else if(_transferCfg.ItemType == GoodsItemType.ShipPlug)
        {
            RogueManager.Instance.OnShipPlugCountChange += OnItemChange;
        }
        else if (_transferCfg.ItemType == GoodsItemType.ShipUnit)
        {

        }
    }

    /// <summary>
    /// 根据比例进行属性转化
    /// </summary>
    private void OnWasteItemChange(int itemCount)
    {
        var rate = Mathf.RoundToInt(itemCount / _transferCfg.BaseValuePer);
        var targetValue = rate * _transferCfg.TargetValuePer;
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(_transferCfg.TargetProperty, PropertyModifyType.Modify, UID, targetValue);
    }

    private void OnItemChange(int itemID, int itemCount)
    {
        if (_transferCfg.ItemID != itemID)
            return;

        var rate = Mathf.RoundToInt(itemCount / _transferCfg.BaseValuePer);
        var targetValue = rate * _transferCfg.TargetValuePer;
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(_transferCfg.TargetProperty, PropertyModifyType.Modify, UID, targetValue);
    }
}

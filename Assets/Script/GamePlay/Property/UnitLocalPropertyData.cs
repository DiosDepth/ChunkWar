using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocalPropertyModifyType
{
    Modify,
    TempModify,
    EffectSlotModify,
}

public class UnitLocalPropertyData 
{
    private Dictionary<UnitPropertyModifyKey, UnitLocalPropertyPool> PropertyPool = new Dictionary<UnitPropertyModifyKey, UnitLocalPropertyPool>();

    public void Clear()
    {
        PropertyPool.Clear();
    }

    /// <summary>
    /// 移除Unit网格效果加成的修正
    /// </summary>
    public void ClearAllSlotEffectPropertyModify()
    {
        foreach(var item in PropertyPool.Values)
        {
            item.ClearModifySlotEffectValue();
        }
    }

    /// <summary>
    /// 绑定ChangeAction
    /// </summary>
    /// <param name="key"></param>
    /// <param name="changeAction"></param>
    public void BindPropertyChangeAction(UnitPropertyModifyKey key, Action changeAction)
    {
        var pool = GetOrCreatePropertyPool(key);
        pool.BindChangeAction(changeAction);
    }

    public void UnBindPropertyChangeAction(UnitPropertyModifyKey key, Action changeAction)
    {
        var pool = GetOrCreatePropertyPool(key);
        pool.UnBindChangeAction(changeAction);
    }

    /// <summary>
    /// 增加修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void AddPropertyModifyValue(UnitPropertyModifyKey propertyKey, LocalPropertyModifyType type, uint fromUID, float value)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.AddToPool(type, fromUID, value);
    }

    /// <summary>
    /// 设置限制最大值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="maxValue"></param>
    public void SetPropertyMaxValue(UnitPropertyModifyKey key, float maxValue)
    {
        var pool = GetOrCreatePropertyPool(key);
        if (pool == null)
            return;

        pool.SetProeprtyMaxValue(maxValue);
    }

    /// <summary>
    /// 设置修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void SetPropertyModifyValue(UnitPropertyModifyKey propertyKey, LocalPropertyModifyType type, uint fromUID, float value)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.SetValue(type, fromUID, value);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, propertyKey);
    }

    /// <summary>
    /// 移除修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    public void RemovePropertyModifyValue(UnitPropertyModifyKey propertyKey, LocalPropertyModifyType type, uint fromUID)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.RemoveFromPool_Modify(type, fromUID);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, propertyKey);
    }

    /// <summary>
    /// 获取最终属性值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public float GetPropertyFinal(UnitPropertyModifyKey key)
    {
        var pool = GetOrCreatePropertyPool(key);
        return pool.GetFinialValue();
    }

    private UnitLocalPropertyPool GetOrCreatePropertyPool(UnitPropertyModifyKey propertyKey)
    {
        if (propertyKey == UnitPropertyModifyKey.NONE)
            return null;

        if (PropertyPool.ContainsKey(propertyKey))
            return PropertyPool[propertyKey];

        UnitLocalPropertyPool pool = new UnitLocalPropertyPool(propertyKey);
        PropertyPool.Add(propertyKey, pool);
        return pool;
    }
}

public class UnitLocalPropertyPool
{
    public UnitPropertyModifyKey PropertyKey;

    private Hashtable _propertyTable_Modify;
    private Hashtable _propertyTable_SlotEffectModify;

    private Action propertychangeAction;

    /// <summary>
    /// 临时修正
    /// </summary>
    private ChangeValue<float> _propertyTemp;

    public float PropertyMaxValue
    {
        get;
        private set;
    }

    public UnitLocalPropertyPool(UnitPropertyModifyKey propertyKey)
    {
        this.PropertyKey = propertyKey;
        _propertyTable_Modify = new Hashtable();
        _propertyTable_SlotEffectModify = new Hashtable();
        _propertyTemp = new ChangeValue<float>(0, float.MinValue, float.MaxValue);
        PropertyMaxValue = float.MaxValue;
    }

    public void BindChangeAction(Action action)
    {
        propertychangeAction += action;
    }

    public void UnBindChangeAction(Action action)
    {
        propertychangeAction -= action;
    }

    public bool AddToPool(LocalPropertyModifyType type, uint key, float value)
    {
        bool succ = false;
        switch (type)
        {
            case LocalPropertyModifyType.Modify:
                succ = AddPropertyModifyValue(key, value);
                break;
            case LocalPropertyModifyType.TempModify:
                succ = AddPropertyTempValue(value);
                break;
            case LocalPropertyModifyType.EffectSlotModify:
                succ = AddPropertyModify_SlotEffectValue(key, value);
                break;
        }
        if (succ)
        {
            propertychangeAction?.Invoke();
            ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
        }

        return true;
    }

    /// <summary>
    /// 设置限制最大值
    /// </summary>
    /// <param name="value"></param>
    public void SetProeprtyMaxValue(float value)
    {
        PropertyMaxValue = value;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(LocalPropertyModifyType type, uint key, float value)
    {
        switch (type)
        {
            case LocalPropertyModifyType.Modify:
                if (_propertyTable_Modify.ContainsKey(key))
                {
                    _propertyTable_Modify[key] = value;
                }
                else
                {
                    _propertyTable_Modify.Add(key, value);
                }
                break;

            case LocalPropertyModifyType.EffectSlotModify:
                if (_propertyTable_SlotEffectModify.ContainsKey(key))
                {
                    _propertyTable_SlotEffectModify[key] = value;
                }
                else
                {
                    _propertyTable_SlotEffectModify.Add(key, value);
                }
                break;
        }
        propertychangeAction?.Invoke();
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
    }

    public void RemoveFromPool_Modify(LocalPropertyModifyType type, uint key)
    {
        switch (type)
        {
            case LocalPropertyModifyType.Modify:
                _propertyTable_Modify.Remove(key);
                break;
            case LocalPropertyModifyType.EffectSlotModify:
                _propertyTable_SlotEffectModify.Remove(key);
                break;
        }

        propertychangeAction?.Invoke();
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
    }

    /// <summary>
    /// 移除临时修正属性
    /// </summary>
    public void ClearTempValue()
    {
        _propertyTemp.Set(0);
    }

    public void ClearModifySlotEffectValue()
    {
        _propertyTable_SlotEffectModify.Clear();
    }

    /// <summary>
    /// 获取最终属性
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetFinialValue()
    {
        ///Modify
        float result = 0;
        foreach (var value in _propertyTable_Modify.Values)
        {
            result += (float)value;
        }

        foreach(var value in _propertyTable_SlotEffectModify.Values)
        {
            result += (float)value;
        }

        var outResult = (result + _propertyTemp.Value);
        return Mathf.Clamp(outResult, float.MinValue, PropertyMaxValue);
    }

    private bool AddPropertyTempValue(float value)
    {
        var newValue = _propertyTemp.Value + value;
        _propertyTemp.Set(newValue);
        return true;
    }

    private bool AddPropertyModifyValue(uint key, float value)
    {
        if (_propertyTable_Modify.ContainsKey(key))
        {
            var newValue = (float)_propertyTable_Modify[key] + value;
            _propertyTable_Modify[key] = newValue;
        }
        else
        {
            _propertyTable_Modify.Add(key, value);
        }
        return true;
    }

    private bool AddPropertyModify_SlotEffectValue(uint key, float value)
    {
        if (_propertyTable_SlotEffectModify.ContainsKey(key))
        {
            var newValue = (float)_propertyTable_SlotEffectModify[key] + value;
            _propertyTable_SlotEffectModify[key] = newValue;
        }
        else
        {
            _propertyTable_SlotEffectModify.Add(key, value);
        }
        return true;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPropertyData 
{
    private Dictionary<PropertyModifyKey, UnitPropertyPool> PropertyPool = new Dictionary<PropertyModifyKey, UnitPropertyPool>();

    /// <summary>
    /// 初始属性
    /// </summary>
    private Dictionary<PropertyModifyKey, ChangeValue<float>> _propertyRow = new Dictionary<PropertyModifyKey, ChangeValue<float>>();

    public void RegisterRowProperty(PropertyModifyKey key, float rowValue, Action<float, float> changecallback = null)
    {
        if (!_propertyRow.ContainsKey(key))
        {
            ChangeValue<float> item = new ChangeValue<float>(rowValue, float.MaxValue, float.MaxValue);
            item.BindChangeAction(changecallback);
        }
    }

    /// <summary>
    /// 增加修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void AddPropertyModifyValue(PropertyModifyKey propertyKey, uint fromUID, float value)
    {
        var pool = GetOrCreatePorpertyPool(propertyKey);
        if (pool == null)
            return;

        pool.AddToPool(fromUID, value);
    }

    public float GetPropertyRowValue(PropertyModifyKey propertyKey)
    {
        if (_propertyRow.ContainsKey(propertyKey))
            return _propertyRow[propertyKey].Value;
        return 0;
    }

    /// <summary>
    /// 设置修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void SetPropertyModifyValue(PropertyModifyKey propertyKey, uint fromUID, float value)
    {
        var pool = GetOrCreatePorpertyPool(propertyKey);
        if (pool == null)
            return;

        pool.SetValue(fromUID, value);
    }

    /// <summary>
    /// 移除修正
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    public void RemovePropertyModifyValue(PropertyModifyKey propertyKey, uint fromUID)
    {
        var pool = GetOrCreatePorpertyPool(propertyKey);
        if (pool == null)
            return;

        pool.RemoveFromPool(fromUID);
    }

    /// <summary>
    /// 获取最终属性值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public float GetPropertyFinal(PropertyModifyKey key)
    {
        var rowValue = GetPropertyRowValue(key);
        return rowValue + GetPropertyModifyValue(key);
    }

    /// <summary>
    /// 获取属性修正值
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <returns></returns>
    public float GetPropertyModifyValue(PropertyModifyKey propertyKey)
    {
        if (PropertyPool.ContainsKey(propertyKey))
        {
            return PropertyPool[propertyKey].GetFinialValue();
        }
        return 0;
    }

    private UnitPropertyPool GetOrCreatePorpertyPool(PropertyModifyKey propertyKey)
    {
        if (propertyKey == PropertyModifyKey.NONE)
            return null;

        if (PropertyPool.ContainsKey(propertyKey))
            return PropertyPool[propertyKey];

        UnitPropertyPool pool = new UnitPropertyPool(propertyKey);
        PropertyPool.Add(propertyKey, pool);
        return pool;
    }


}

public class UnitPropertyPool
{
    public PropertyModifyKey PropertyKey;

    private Hashtable _propertyTable;

    private Action propertychangeAction;

    public UnitPropertyPool(PropertyModifyKey propertyKey)
    {
        this.PropertyKey = propertyKey;
        _propertyTable = new Hashtable();
    }

    public bool AddToPool(uint key, float value)
    {
        if (_propertyTable.ContainsKey(key))
            return false;

        _propertyTable.Add(key, value);
        return true;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(uint key, float value)
    {
        if (_propertyTable.ContainsKey(key))
        {
            _propertyTable[key] = value;
        }
        else
        {
            _propertyTable.Add(key, value);
        }
    }

    public void RemoveFromPool(uint key)
    {
        _propertyTable.Remove(key);
    }

    public float GetFinialValue()
    {
        float result = 0;
        foreach (var value in _propertyTable.Values)
        {
            result += (float)value;
        }
        return result;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPropertyData 
{
    private Dictionary<PropertyModifyKey, UnitPropertyPool> PropertyPool = new Dictionary<PropertyModifyKey, UnitPropertyPool>();

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

    public UnitPropertyPool(PropertyModifyKey propertyKey)
    {
        this.PropertyKey = propertyKey;
        _propertyTable = new Hashtable();
    }

    public void AddToPool(uint key, float value)
    {
        if (_propertyTable.ContainsKey(key))
            return;

        _propertyTable.Add(key, value);
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